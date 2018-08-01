using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace vdm.Services
{
    [Service]
    [IntentFilter(new string[] { "com.varanegar.vdmservice" })]
    public class VdmService : BaseService
    {
        private Messenger _messenger;

        public override void OnCreate()
        {
            base.OnCreate();
            Logger.Info("OnCreate");
            _messenger = new Messenger(new VdmServiceHandler(this));
        }
        public override IBinder OnBind(Intent intent)
        {
            Logger.Info("OnBind");
            return _messenger.Binder;
        }
        public override bool OnUnbind(Intent intent)
        {
            Logger.Info("OnUnbind");
            return base.OnUnbind(intent);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            Logger.Info("OnDestroy");
        }
    }
}