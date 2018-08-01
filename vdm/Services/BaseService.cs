using Android.App;
using NLog;
using vdm.Base;

namespace vdm.Services
{
    public abstract class BaseService : Service
    {
        protected Logger Logger { get { return NLog.LogManager.GetCurrentClassLogger(); } }
        public override void OnCreate()
        {
            base.OnCreate();
            GlobalClass.ConfigNLog(this);
        }
    }
}