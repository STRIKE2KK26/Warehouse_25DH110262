using System.Collections.Generic;

namespace Warehouse_25DH110262.Models
{
    public class EditVM
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string Error { get; set; }
        public string Success { get; set; }
    }

    public class FilterVM
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Category { get; set; }
        public string Sort { get; set; }
        public List<string> Categories { get; set; }
        public List<Product> Items { get; set; }
        public string Error { get; set; }
    }

    public class AdjustVM
    {
        public string Mode { get; set; }          // "Increase" | "Decrease"
        public int? ProductID { get; set; }
        public string Percent { get; set; }
        public List<Product> Products { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public Product Result { get; set; }
    }

    public class DiscountTwoVM
    {
        public int? ProductA { get; set; }
        public int? ProductB { get; set; }
        public string PercentA { get; set; }
        public string PercentB { get; set; }
        public List<Product> Products { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class CreateVM
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Price { get; set; }
        public string Quantity { get; set; }
        public string Error { get; set; }
    }

    public class StatsVM
    {
        public int Total { get; set; }
        public int HighCount { get; set; }
        public decimal TotalValue { get; set; }
    }
}
