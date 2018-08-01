using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using DiscountV2.Entity.MasterData;
using NLog;

namespace vdm.Base
{
    public class DatabaseHelper : SQLiteOpenHelper
    {
        public const int DbVersion = 1;
        private Context _context;

        Logger Logger
        {
            get
            {
                return LogManager.GetCurrentClassLogger();
            }
        }
        public static void SetVersion(Context context, int v)
        {
            context.GetSharedPreferences("DatabaseHelper", FileCreationMode.Private).Edit().PutInt("version", v).Commit();
        }
        public static int GetVersion(Context context)
        {
            return context.GetSharedPreferences("DatabaseHelper", FileCreationMode.Private).GetInt("version", 1);
        }
        public static string GetDatabaseFilePath(string appId)
        {
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            path = Path.Combine(path, appId);
            return path;
        }
        public DatabaseHelper(Context context, string name, int version) : base(context, name, null, version)
        {
            _context = context;
        }

        public override void OnCreate(SQLiteDatabase db)
        {
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
        }

        internal static List<Type> GetEntities()
        {
            return new List<Type>
            {
                typeof(CPrice),
                typeof(Customer),
                typeof(CustomerGroup),
                typeof(CustomerMainSubType),
                typeof(Goods),
                typeof(Discount),

            };
        }
        public void EmptyDb(string appId)
        {

            // query to obtain the names of all tables in your database
            ICursor c = WritableDatabase.RawQuery("SELECT name FROM sqlite_master WHERE type='table'", null);
            List<String> tables = new List<string>();

            // iterate over the result set, adding every table name to a list
            while (c.MoveToNext())
            {
                tables.Add(c.GetString(0));
            }

            // call DROP TABLE on every table name
            foreach (String table in tables)
            {
                String dropQuery = "DELETE FROM " + table;
                WritableDatabase.ExecSQL(dropQuery);
            }
        }

        public Byte[] GetByteArray()
        {
            var file = _context.GetDatabasePath(DatabaseName);
            Byte[] bytes = File.ReadAllBytes(file.AbsolutePath);
            return bytes;
        }

        internal void AttachDB(string SessionId, string appId)
        {
            var dbFile = new Java.IO.File(_context.ExternalCacheDir.AbsolutePath + "/" + SessionId);
            var db = WritableDatabase;
            try
            {
                db.ExecSQL("attach database '" + dbFile.Path + "' as externaldb;");
                // query to obtain the names of all tables in your database
                ICursor tc = db.RawQuery("SELECT name FROM sqlite_master WHERE type='table'", null);
                List<String> tables = new List<string>();
                // iterate over the result set, adding every table name to a list
                while (tc.MoveToNext())
                {
                    tables.Add(tc.GetString(0));
                }

                // call DROP TABLE on every table name
                foreach (String table in tables)
                {
                    ICursor cc = db.RawQuery("PRAGMA table_info(" + table + ")", null);
                    StringBuilder c1 = new StringBuilder();
                    StringBuilder c2 = new StringBuilder();
                    if (cc != null)
                    {
                        cc.MoveToFirst();
                        bool firstRow = true;
                        int size = cc.Count;
                        for (int i = 0; i < size; i++)
                        {
                            cc.MoveToPosition(i);
                            String columnName = cc.GetString(cc.GetColumnIndex("name"));
                            if (firstRow)
                            {
                                c1.Append(columnName);
                                if (columnName == "AppId")
                                    c2.Append("'" + appId + "' as " + columnName);
                                else
                                    c2.Append(columnName);
                            }
                            else
                            {
                                c1.Append(",").Append(columnName);
                                if (columnName == "AppId")
                                    c2.Append(",'" + appId + "' as " + columnName);
                                else
                                    c2.Append(",").Append(columnName);
                            }
                            firstRow = false;
                        }
                    }
                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO '")
                            .Append(table).Append("' (")
                            .Append(c1).Append(")")
                            .Append(" SELECT ").Append(c2).Append(" FROM ")
                            .Append("externaldb.").Append(table);
                    String raw = sql.ToString();
                    Logger.Debug("Executing sql : " + raw);
                    db.ExecSQL(raw);
                    try
                    {
                        ICursor result = ReadableDatabase.RawQuery("select changes();", null);
                        if (result != null)
                        {
                            result.MoveToFirst();
                            int c = result.GetInt(0);
                            Logger.Debug("Number of rows, inserted to table " + table + " = " + c);

                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                if (db != null)
                    db.Close();
                dbFile.Delete();
            }

        }

        internal string GetDBImportPath()
        {
            return _context.ExternalCacheDir.AbsolutePath;
        }
    }
}