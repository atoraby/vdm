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
using DiscountV2.Calculator.Helper;
using DiscountV2.Entity.Internal;

namespace vdm.Base
{
    public class AdvancedConditionHelper : AdvanceConditionHelperInterface
    {
        public override CalcData CalcPeriodicDiscount(CalcData data)
        {
            return data;
        }

        public override CalcData ValidateAdvanceCondition(CalcData data)
        {
            return data;
        }
    }
}