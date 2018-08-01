using System;

namespace vdm.Base
{
    internal class BackupInfo
    {
        public DateTime Date { get; internal set; }
        public Guid UniqueId { get; internal set; }
        public int DatabaseVersion { get; internal set; }
        public string PackageName { get; internal set; }
        public string AppVersionName { get; internal set; }
        public int AppVersionCode { get; internal set; }
    }
}