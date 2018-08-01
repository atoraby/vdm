using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Android.OS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using vdm.Base;
namespace vdm.Services
{
    internal class VdmServiceHandler : Handler
    {
        private const int MSG_GET_VERSION = 100;
        private const int MSG_INIT_REQUEST = 101;
        private const int MSG_ATCH_NOTIFICATION = 103;
        private const int MSG_CALC_REQUEST = 104;
        private VdmService _vdmService;
        private DatabaseHelper _db;
        Logger Logger { get { return LogManager.GetCurrentClassLogger(); } }
        public VdmServiceHandler(VdmService vdmService)
        {
            _vdmService = vdmService;
        }
        public override void HandleMessage(Message msg)
        {
            try
            {
                Logger.Info("Message received. Code = " + msg.What);
                Messenger messenger = msg.ReplyTo;
                Message replyMsg = Message.Obtain(null, msg.What);
                string appId = msg.Data.GetString("AppId");
                if (!string.IsNullOrEmpty(appId))
                {
                    ConfigDatabase(appId);
                    switch (msg.What)
                    {
                        case MSG_GET_VERSION:
                            ProcessGetVersion(msg, replyMsg);
                            break;
                        case MSG_INIT_REQUEST:
                            ProcessInitRequest(msg, replyMsg);
                            break;
                        case MSG_ATCH_NOTIFICATION:
                            ProcessAtachment(msg, replyMsg);
                            break;
                        case MSG_CALC_REQUEST:
                            ProcessCalcRequest(msg, replyMsg);
                            break;
                        default:
                            base.HandleMessage(msg);
                            break;
                    }
                }
                else
                {
                    Logger.Error("AppId not dound!");
                }
                messenger.Send(replyMsg);
            }
            catch (RemoteException e)
            {
                Logger.Error(e);
            }
        }
        private void ProcessCalcRequest(Message msg, Message replyMsg)
        {
            string clientSessionId = msg.Data.GetString("SessionId", null);
            string appId = msg.Data.GetString("AppId", null);
            if (string.IsNullOrEmpty(clientSessionId))
                return;
            if (string.IsNullOrEmpty(appId))
                return;

            var sessionId = GetSessionId(_vdmService, appId);
            if (sessionId != clientSessionId)
                return;

            string requestId = msg.Data.GetString("RequestId", null);
            if (requestId == null)
                return;

            string json = msg.Data.GetString("CallData");
            if (json == null || json.Length == 0)
                return;
            var callData = JsonConvert.DeserializeObject<CallData>(json);
            Bundle bundle = new Bundle();
            bundle.PutString("CallData", JsonConvert.SerializeObject(callData, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
            bundle.PutString("RequestId", requestId);
            bundle.PutString("SessionId", sessionId);
            replyMsg.Data = bundle;
        }
        private void ProcessAtachment(Message msg, Message replyMsg)
        {
            string appId = msg.Data.GetString("AppId", null);
            if (string.IsNullOrEmpty(appId))
                return;
            string clientSessionId = msg.Data.GetString("SessionId", null);
            if (clientSessionId == null)
                return;
            string SessionId = GetSessionId(_vdmService, appId);
            if (SessionId == null)
                return;
            if (SessionId != clientSessionId)
                return;
            else
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SessionId", SessionId.ToString());
                _db.AttachDB(SessionId, appId);
                replyMsg.Arg1 = 1;
                replyMsg.Data = bundle;
            }
        }
        private void ProcessInitRequest(Message msg, Message replyMsg)
        {
            string appId = msg.Data.GetString("AppId", null);
            if (string.IsNullOrEmpty(appId))
                return;
            else
            {
                Bundle bundle = new Bundle();
                var SessionId = Guid.NewGuid().ToString();
                Random rand = new Random();
                for (int i = 0; i < 5; i++)
                {
                    int p = rand.Next(SessionId.Length - 1);
                    SessionId = SessionId.Remove(p, 1);
                }
                SessionId = SessionId.Replace("-", "");
                SaveSessionId(_vdmService, appId, SessionId);
                bundle.PutString("SessionId", SessionId.ToString());
                _db.EmptyDb(appId);
                byte[] bytes = _db.GetByteArray();
                bundle.PutByteArray("Database", bytes);
                bundle.PutString("DestinationPath", _db.GetDBImportPath());
                replyMsg.Data = bundle;
            }
        }
        private void ProcessGetVersion(Message msg, Message replyMsg)
        {
            string PackageName = _vdmService.PackageName;
            string version = "";
            try
            {
                version = _vdmService.ApplicationContext.PackageManager.GetPackageInfo(_vdmService.PackageName, 0).VersionName;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            Bundle bundle = new Bundle();
            bundle.PutString("version", version);
            replyMsg.Data = bundle;
        }
        private void SaveSessionId(Context context, string appId, string SessionId)
        {
            ISharedPreferences sharedPreferences = context.GetSharedPreferences("VdmServiceHandler", FileCreationMode.Private);
            sharedPreferences.Edit().PutString(appId, SessionId).Apply();
        }
        private static string GetSessionId(Context context, string appId)
        {
            ISharedPreferences sharedPreferences = context.GetSharedPreferences("VdmServiceHandler", FileCreationMode.Private);
            return sharedPreferences.GetString(appId, null);
        }
        private List<Type> GetEntities()
        {
            return DatabaseHelper.GetEntities();
        }
        private void ConfigDatabase(string appId)
        {
            if (_db != null)
                return;
            var path = DatabaseHelper.GetDatabaseFilePath(appId);
            if (!File.Exists(path))
            {
                var connection = new SQLiteConnection(new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid(), path);
                List<Type> types = GetEntities();
                foreach (var item in types)
                {
                    connection.CreateTable(item);
                }
                connection.Close();
                DatabaseHelper.SetVersion(_vdmService, DatabaseHelper.DbVersion);
            }
            else
            {
                int version = DatabaseHelper.GetVersion(_vdmService);
                if (version != DatabaseHelper.DbVersion)
                {
                    var connection = new SQLiteConnection(new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid(), path);
                    List<Type> types = GetEntities();
                    foreach (var item in types)
                    {
                        connection.DropTable(item);
                        connection.CreateTable(item);
                    }
                    DatabaseHelper.SetVersion(_vdmService, DatabaseHelper.DbVersion);
                }
            }
            _db = new DatabaseHelper(_vdmService, path, 1);
            SaveAppId(appId);
        }

        private void SaveAppId(string appId)
        {
            var sp = _vdmService.GetSharedPreferences("VdmServiceHandler", FileCreationMode.Private);
            var appIds = sp.GetStringSet("AppIds", null);
            if (appIds == null)
                appIds = new List<String>();
            if (!appIds.Contains(appId))
                appIds.Add(appId);
            sp.Edit().PutStringSet("AppIds", appIds).Apply();
        }
        public static ICollection<string> GetAppIds(Context context)
        {
            var sp = context.GetSharedPreferences("VdmServiceHandler", FileCreationMode.Private);
            var list = sp.GetStringSet("AppIds", null);
            if (list == null)
                list = new List<String>();
            return list;
        }
    }
}