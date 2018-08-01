using System;
using System.Collections.Generic;

namespace vdm.Services
{
    internal class CallData
    {
        public Guid UniqueId;
        public Guid CustomerId;
        public int CustomerRef;
        public string CustomerName;
        public string CustomerCode;
        public decimal? RemainDebit;
        public decimal? RemainCredit;
        public Guid OrderPaymentTypeId;
        public int OrderPaymentTypeRef;
        public string OrderPaymentTypeName;
        public Guid OrderTypeId;
        public int OrderTypeRef;
        public string OrderTypeName;
        public int DistBackOfficeId;
        public int DisType;
        public string Comment;
        public int LocalPaperNo;
        public string BackOfficeOrderNo;
        public DateTime SaleDate;
        public string BackOfficeOrderId;
        public decimal? RoundOrderAmount;
        public decimal? RoundOrderOtherDiscount;
        public decimal? RoundOrderDis1;
        public decimal? RoundOrderDis2;
        public decimal? RoundOrderDis3;
        public decimal? RoundOrderTax;
        public decimal? RoundOrderCharge;
        public decimal? RoundOrderAdd1;
        public decimal? RoundOrderAdd2;
        public int BackOfficeInvoiceId;
        public string BackOfficeInvoiceNo;
        public decimal? RoundInvoiceAmount;
        public decimal? RoundInvoiceOtherDiscount;
        public decimal? RoundInvoiceTax;
        public decimal? RoundInvoiceCharge;
        public decimal? RoundInvoiceDis1;
        public decimal? RoundInvoiceDis2;
        public decimal? RoundInvoiceDis3;
        public decimal? RoundInvoiceAdd1;
        public decimal? RoundInvoiceAdd2;
        public Guid InvoicePaymentTypeUniqueId;
        public int IsPromotion;
        public Guid PromotionUniqueId;
        public int SupervisorRefSDS;
        public string DcCodeSDS;
        public string SaleIdSDS;
        public int SaleNoSDS;
        public int DealerRefSDS;
        public string OrderNoSDS;
        public int AccYearSDS;
        public decimal? TotalOrderDis1;
        public decimal? TotalOrderDis2;
        public decimal? TotalOrderDis3;
        public decimal? TotalOrderAdd1;
        public decimal? TotalOrderAdd2;
        public decimal? TotalInvoiceDis1;
        public decimal? TotalInvoiceDis2;
        public decimal? TotalInvoiceDis3;
        public decimal? TotalInvoiceAdd1;
        public decimal? TotalInvoiceAdd2;
        public decimal? TotalInvoiceTax;
        public decimal? TotalInvoiceCharge;
        public decimal? TotalInvoiceDiscount;
        public decimal? TotalInvoiceAdd;
        public decimal? TotalPrice;
        public decimal? TotalPriceWithPromo;
        public OrderTypeEnum DocumentType;
        public List<CustomerCallOrderLineGoodPackage> goodPackages;
        public List<CustomerCallOrderLinePromotion> Lines;
        public List<DiscountEvcItemStatuteData> discountEvcItemStatuteData;
        public List<CustomerCallOrderLinePromotion> LinesWithPromo;
    }

    public class DiscountEvcItemStatuteData
    {
        public int EvcItemRef;
        public int DisRef;
        public long Discount;
        public long AddAmount;
        public long SupAmount;
    }

    public class CustomerCallOrderLinePromotion
    {
        public Guid OrderId;
        public Guid ProductId;
        public string ProductCode;
        public string ProductName;
        public int ProductRef;

        public string UnitName;
        public string QtyCaption;
        public int ConvertFactory;

        public decimal? UnitPrice;
        public string PriceId;
        public int SortId;
        public bool IsRequestFreeItem;
        public decimal? RequestBulkQty;
        public Guid RequesBulkQtyUnitUniqueId;
        public Guid UnitUniqieId;

        public decimal? RequestAmount;
        public decimal? RequestNetAmount;
        public decimal? RequestDis1;
        public decimal? RequestDis2;
        public decimal? RequestDis3;
        public decimal? RequestAdd1;
        public decimal? RequestAdd2;
        public decimal? RequestTax;
        public decimal? RequestCharge;
        public decimal? RequestOtherDiscount;
        public decimal? TotalRequestQty;

        public decimal? InvoiceAmount;
        public decimal? InvoiceNetAmount;
        public decimal? InvoiceDis1;
        public decimal? InvoiceDis2;
        public decimal? InvoiceDis3;
        public decimal? InvoiceAdd1;
        public decimal? InvoiceAdd2;
        public decimal? InvoiceTax;
        public decimal? InvoiceCharge;
        public decimal? InvoiceOtherDiscount;
        public decimal? TotalInvoiceDis;
        public decimal? TotalInvoiceAdd;
        public decimal? TotalInvoiceQty;

        public string Comment;
        public Guid EVCId;
        public Guid FreeReasonId;
        public string FreeReasonName;
        public decimal? InvoiceBulkQty;
        public Guid InvoiceBulkQtyUnitUniqueId;

        public bool IsRequestPrizeItem;
        public int DiscountRef;
        public Guid DiscountId;
        public Guid UniqueId;
    }

    public class CustomerCallOrderLineGoodPackage
    {
        public string EvcId;
        public string OrderUniqueId;
        public int MainGoodsPackageItemRef;
        public int ReplaceGoodsPackageItemRef;
        public string DiscountRef;
        public decimal? PrizeQty;
        public int PrizeCount;
    }

    public enum OrderTypeEnum
    {
        ORDER,
        INVOICE,
        PRESALE
    }
}