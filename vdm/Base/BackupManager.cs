using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Java.Interop;
using Java.IO;
using Newtonsoft.Json;
using vdm.Services;

namespace vdm.Base
{
    public class BackupManager
    {
        public static void ExportData(Context context)
        {
            BackupInfo backupInfo = new BackupInfo();
            backupInfo.Date = DateTime.Now;
            backupInfo.UniqueId = Guid.NewGuid();
            backupInfo.DatabaseVersion = DatabaseHelper.GetVersion(context);
            backupInfo.PackageName = context.PackageName;
            try
            {
                backupInfo.AppVersionCode = context.ApplicationContext.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode;
                backupInfo.AppVersionName = context.ApplicationContext.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex);
            }
            String backupInfoFileName = SaveBackupInfo(context, backupInfo);
            Java.IO.File file = new Java.IO.File(GlobalClass.GetLogDirectory(context));
            List<String> logFilePaths = new List<string>();
            if (file.Exists())
            {
                Java.IO.File[] logFiles = file.ListFiles(new FileNameFilter());
                foreach (Java.IO.File f in
                    logFiles)
                {
                    logFilePaths.Add(f.AbsolutePath);
                }
            }
            List<String> files = new List<string>();
            files.Add(backupInfoFileName);
            foreach (var appId in VdmServiceHandler.GetAppIds(context))
            {
                files.Add(DatabaseHelper.GetDatabaseFilePath(appId));
            }
            files.AddRange(logFilePaths);
            string zipFilename = CreateBackupName();
            Zip(context, files, zipFilename);
            String path = Path.Combine(context.ExternalCacheDir.AbsolutePath);
            MediaScannerConnection.ScanFile(context, new String[] { path }, null, null);

        }

        static string ToString(int number)
        {
            if (number > 9)
                return number.ToString();
            else
                return "0" + number.ToString();
        }
        private static string CreateBackupName()
        {
            String date = DateTime.Now.Year.ToString() + ToString(DateTime.Now.Month) + ToString(DateTime.Now.Day) + "_"
                + ToString(DateTime.Now.Hour) + ToString(DateTime.Now.Minute) + ToString(DateTime.Now.Second);
            String zipFilename = date + ".backup";
            return zipFilename;
        }

        private static void Zip(Context context, List<string> files, string zipFileName)
        {
            FileStream fsOut = System.IO.File.Create(Path.Combine(context.ExternalCacheDir.AbsolutePath, zipFileName));
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);

            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

            // This setting will strip the leading part of the folder path in the entries, to
            // make the entries relative to the starting folder.
            // To include the full path for each entry up to the drive root, assign folderOffset = 0.
            foreach (string f in files)
            {

                FileInfo fi = new FileInfo(f);
                if (fi.Exists)
                {
                    string entryName = f.Substring(f.LastIndexOf("/") + 1);
                    ZipEntry newEntry = new ZipEntry(entryName);
                    newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                    // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                    // A password on the ZipOutputStream is required if using AES.
                    //   newEntry.AESKeySize = 256;

                    // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                    // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                    // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                    // but the zip will be in Zip64 format which not all utilities can understand.
                    //   zipStream.UseZip64 = UseZip64.Off;
                    newEntry.Size = fi.Length;

                    zipStream.PutNextEntry(newEntry);

                    // Zip the file in buffered chunks
                    // the "using" will close the stream even if an exception occurs
                    byte[] buffer = new byte[4096];
                    using (FileStream streamReader = System.IO.File.OpenRead(f))
                    {
                        StreamUtils.Copy(streamReader, zipStream, buffer);
                    }
                    zipStream.CloseEntry();
                }
                else
                {
                    NLog.LogManager.GetCurrentClassLogger().Debug(f + " not found!");
                }
            }

            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        private static string SaveBackupInfo(Context context, BackupInfo backupInfo)
        {
            OutputStreamWriter streamWriter = new OutputStreamWriter(context.OpenFileOutput("backup.json", FileCreationMode.Private));
            streamWriter.Write(JsonConvert.SerializeObject(backupInfo));
            streamWriter.Close();
            return context.FilesDir + "/" + "backup.json";
        }

        public class FileNameFilter : Java.Lang.Object, IFilenameFilter
        {
            public bool Accept(Java.IO.File dir, string name)
            {
                return name.EndsWith(".log");
            }
        }
    }
}