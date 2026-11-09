using System;
using System.Globalization;

namespace Warehouse_25DH110262.Models
{
    public static class PriceHelper
    {
        public const decimal MinPriceAfterDiscount = 1000m;
        public const decimal MaxPrice = 9999999999999m;

        public static string GetLevel(decimal price)
        {
            if (price < 100000m) return "Giá thấp";
            if (price < 500000m) return "Giá trung bình";
            return "Giá cao";
        }

        public static string Money(decimal v)
        {
            return v.ToString("N0", new CultureInfo("vi-VN")) + " đ";
        }

        // Nhập số kiểu 200000 hoặc 200000.5 (dấu , cũng được chấp nhận)
        public static bool TryParseDecimal(string s, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim().Replace(",", ".");
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        public static decimal Round2(decimal v)
        {
            return Math.Round(v, 2, MidpointRounding.AwayFromZero);
        }
    }
}
