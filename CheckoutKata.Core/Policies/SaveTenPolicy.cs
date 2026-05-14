using System;
using System.Collections.Generic;
using System.Text;

namespace CheckoutKata.Core.Policies
{
    using Interfaces;

    public class SaveTenPolicy : IDiscountVoucher
    {
        public int GetVoucherDiscount()
        {
                        return 10;
        }
    }
}
