using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using NLog;

namespace vdm.Base
{
    public class GlobalClass
    {
        public static void ConfigNLog(Context context)
        {
            var config = new NLog.Config.LoggingConfiguration();
            DateTime now = DateTime.Now;
            string logName = now.Year.ToString() + "_" + now.Month.ToString() + "_" + now.Day.ToString() + ".log";
            string logfilePath = Path.Combine(GetLogDirectory(context), logName);
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = logfilePath };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }
        private static Java.IO.File GetAbsoluteFile(String relativePath, Context context)
        {
            String state = Android.OS.Environment.ExternalStorageState;
            if (state == Android.OS.Environment.MediaMounted)
            {
                Java.IO.File file = new Java.IO.File(context.GetExternalFilesDir(null), relativePath);
                return file;
            }
            else
            {
                Java.IO.File file = new Java.IO.File(context.FilesDir, relativePath);
                return file;
            }
        }

        public static String GetLogDirectory(Context context)
        {
            return GetAbsoluteFile("/logs", context).AbsolutePath;
        }
    }
}