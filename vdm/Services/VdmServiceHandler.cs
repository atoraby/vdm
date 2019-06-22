using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Android.OS;
using DiscountV2.Entity.Internal;
using DiscountV2.Entity.MasterData;
using DiscountV2.Entity.SDS.Evc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using RuleEngine;
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

            string json = msg.Data.GetString("CalcData");
            if (json == null || json.Length == 0)
                return;


            var calcData = JsonConvert.DeserializeObject<CalcData>(json);
            fillInitData(calcData);
            var ch = new DiscountCalculatorHandler();
            calcData.AdvanceConditionHelper = new AdvancedConditionHelper();
            var result = ch.Calculate(calcData);

            var jb = JsonConvert.SerializeObject(cleanCalcData(result), new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            Bundle bundle = new Bundle();
            bundle.PutString("CalcData", jb);
            bundle.PutString("RequestId", requestId);
            bundle.PutString("SessionId", sessionId);
            replyMsg.Data = bundle;
        }

        private CalcData cleanCalcData(CalcData calcData)
        {
            calcData.SaleItms = new List<SaleItm>();
            calcData.OrderPrizes = new List<OrderPrize>();
            calcData.DisAccs = new List<DisAcc>();
            calcData.StockGoods = new List<StockGoods>();
            //calcData.Customers = new List<Customer>();
            calcData.SaleHdrs = new List<SaleHdr>();
            calcData.Packages = new List<Package>();
            calcData.GoodsMainSubTypes = new List<GoodsMainSubType>();
            calcData.CustomerGroups = new List<CustomerGroup>();
            calcData.DisSalePrizePackages = new List<DisSalePrizePackage>();
            calcData.CustomerMainSubTypes = new List<CustomerMainSubType>();
            calcData.Goods = new List<Goods>();
            calcData.CPrices = new List<CPrice>();
            calcData.Discounts = new List<Discount>();
            calcData.EvcPrize = new List<EvcPrize>();
            calcData.Prices = new List<Price>();
            calcData.EvcPrizePackage = new List<EvcPrizePackage>();
            calcData.FreeReasons = new List<FreeReason>();
            calcData.DiscountGoodsPackageItems = new List<DiscountGoodsPackageItem>();
            return calcData;
        }

        private void fillInitData(CalcData calcData)
        {
            var connection = new SQLiteConnection(new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid(), _db.GetDatabasePath());
            calcData.SaleItms = new List<SaleItm>();
            foreach (var item in connection.Table<SaleItm>())
            {
                calcData.SaleItms.Add(item);
            }
            calcData.OrderPrizes = new List<OrderPrize>();
            foreach (var item in connection.Table<OrderPrize>())
            {
                calcData.OrderPrizes.Add(item);
            }
            calcData.DisAccs = new List<DisAcc>();
            foreach (var item in connection.Table<DisAcc>())
            {
                calcData.DisAccs.Add(item);
            }
            calcData.StockGoods = new List<StockGoods>();
            foreach (var item in connection.Table<StockGoods>())
            {
                calcData.StockGoods.Add(item);
            }
            //calcData.Customers = new List<Customer>();
            //foreach (var item in connection.Table<Customer>())
            //{
            //    calcData.Customers.Add(item);
            //}
            calcData.SaleHdrs = new List<SaleHdr>();
            foreach (var item in connection.Table<SaleHdr>())
            {
                calcData.SaleHdrs.Add(item);
            }
            calcData.Packages = new List<Package>();
            foreach (var item in connection.Table<Package>())
            {
                calcData.Packages.Add(item);
            }
            calcData.GoodsMainSubTypes = new List<GoodsMainSubType>();
            foreach (var item in connection.Table<GoodsMainSubType>())
            {
                calcData.GoodsMainSubTypes.Add(item);
            }
            calcData.CustomerGroups = new List<CustomerGroup>();
            foreach (var item in connection.Table<CustomerGroup>())
            {
                calcData.CustomerGroups.Add(item);
            }
            calcData.DisSalePrizePackages = new List<DisSalePrizePackage>();
            foreach (var item in connection.Table<DisSalePrizePackage>())
            {
                calcData.DisSalePrizePackages.Add(item);
            }
            calcData.CustomerMainSubTypes = new List<CustomerMainSubType>();
            foreach (var item in connection.Table<CustomerMainSubType>())
            {
                calcData.CustomerMainSubTypes.Add(item);
            }
            calcData.GoodsGroups = new List<GoodsGroup>();
            foreach (var item in connection.Table<GoodsGroup>())
            {
                calcData.GoodsGroups.Add(item);
            }
            calcData.Goods = new List<Goods>();
            foreach (var item in connection.Table<Goods>())
            {
                calcData.Goods.Add(item);
            }
            calcData.CPrices = new List<CPrice>();
            foreach (var item in connection.Table<CPrice>())
            {
                calcData.CPrices.Add(item);
            }
            calcData.Discounts = new List<Discount>();
            foreach (var item in connection.Table<Discount>())
            {
                calcData.Discounts.Add(item);
            }
            calcData.EvcPrize = new List<EvcPrize>();
            foreach (var item in connection.Table<EvcPrize>())
            {
                calcData.EvcPrize.Add(item);
            }
            calcData.Prices = new List<Price>();
            foreach (var item in connection.Table<Price>())
            {
                calcData.Prices.Add(item);
            }
            calcData.EvcPrizePackage = new List<EvcPrizePackage>();
            foreach (var item in connection.Table<EvcPrizePackage>())
            {
                calcData.EvcPrizePackage.Add(item);
            }
            calcData.FreeReasons = new List<FreeReason>();
            foreach (var item in connection.Table<FreeReason>())
            {
                calcData.FreeReasons.Add(item);
            }
            calcData.DiscountGoodsPackageItems = new List<DiscountGoodsPackageItem>();
            foreach (var item in connection.Table<DiscountGoodsPackageItem>())
            {
                calcData.DiscountGoodsPackageItems.Add(item);
            }
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