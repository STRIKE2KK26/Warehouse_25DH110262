using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warehouse_25DH110262.Models;

namespace Warehouse_25DH110262.Controllers
{
    public class ProductsController : Controller
    {
        // Tên context do wizard EF Database First tạo ra (đặt tên connection là WarehouseEntities)
        private WarehouseEntities db = new WarehouseEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // ===================== CÂU 2a: DANH SÁCH =====================
        public ActionResult Index()
        {
            var list = db.Products.OrderBy(p => p.ProductID).ToList();
            return View(list);
        }

        // ===================== CÂU 2b: SỬA =====================
        public ActionResult Edit(int id)
        {
            var p = db.Products.Find(id);
            if (p == null) return HttpNotFound();
            return View(new EditVM
            {
                ProductID = p.ProductID, ProductName = p.ProductName, Category = p.Category,
                Price = p.Price.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                Quantity = p.Quantity
            });
        }

        [HttpPost]
        public ActionResult Edit(int id, string productName, string category, string price)
        {
            var p = db.Products.Find(id);
            if (p == null) return HttpNotFound();

            var vm = new EditVM
            {
                ProductID = p.ProductID, Quantity = p.Quantity,       // ID & Quantity luôn lấy từ CSDL
                ProductName = productName, Category = category, Price = price
            };

            decimal pr;
            if (string.IsNullOrWhiteSpace(productName)) vm.Error = "Tên sản phẩm không được để trống.";
            else if (string.IsNullOrWhiteSpace(category)) vm.Error = "Danh mục không được để trống.";
            else if (!PriceHelper.TryParseDecimal(price, out pr)) vm.Error = "Giá không hợp lệ.";
            else if (pr <= 0) vm.Error = "Giá mới phải lớn hơn 0.";
            else if (pr > PriceHelper.MaxPrice) vm.Error = "Giá quá lớn.";
            else
            {
                p.ProductName = productName.Trim();
                p.Category = category.Trim();
                p.Price = PriceHelper.Round2(pr);      // KHÔNG đụng tới ProductID, Quantity
                db.SaveChanges();
                vm.Success = "Đã cập nhật sản phẩm vào SQL Server.";
            }
            return View(vm);
        }

        // ===================== CÂU 3: LỌC + SẮP XẾP =====================
        public ActionResult Filter(string from, string to, string category, string sort)
        {
            var vm = new FilterVM
            {
                From = from, To = to, Category = category, Sort = string.IsNullOrEmpty(sort) ? "asc" : sort,
                Categories = db.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList(),
                Items = new List<Product>()
            };

            decimal f = 0, t = 0;
            bool hasFrom = !string.IsNullOrWhiteSpace(from);
            bool hasTo = !string.IsNullOrWhiteSpace(to);

            if (hasFrom && !PriceHelper.TryParseDecimal(from, out f)) vm.Error = "Từ giá không hợp lệ.";
            else if (hasTo && !PriceHelper.TryParseDecimal(to, out t)) vm.Error = "Đến giá không hợp lệ.";
            else if (hasFrom && hasTo && f > t) vm.Error = "Khoảng giá không hợp lệ: Từ giá lớn hơn Đến giá.";

            if (vm.Error != null) return View(vm);        // không chạy truy vấn lọc

            IQueryable<Product> q = db.Products;
            if (hasFrom) q = q.Where(p => p.Price >= f);   // gồm đầu mút
            if (hasTo) q = q.Where(p => p.Price <= t);
            if (!string.IsNullOrEmpty(category)) q = q.Where(p => p.Category == category);

            q = (vm.Sort == "desc") ? q.OrderByDescending(p => p.Price) : q.OrderBy(p => p.Price);
            vm.Items = q.ToList();
            return View(vm);
        }

        // ===================== CÂU 4: TĂNG / GIẢM GIÁ =====================
        private AdjustVM NewAdjust(string mode, int? id, string pct)
        {
            return new AdjustVM { Mode = mode, ProductID = id, Percent = pct };
        }

        private ActionResult AdjustView(AdjustVM vm)
        {
            vm.Products = db.Products.OrderBy(p => p.ProductID).ToList();
            return View("Adjust", vm);
        }

        public ActionResult Increase() { return AdjustView(NewAdjust("Increase", null, null)); }
        public ActionResult Decrease() { return AdjustView(NewAdjust("Decrease", null, null)); }

        [HttpPost]
        public ActionResult Increase(int? productId, string percent)
        {
            var vm = NewAdjust("Increase", productId, percent);
            var p = productId.HasValue ? db.Products.Find(productId.Value) : null;
            decimal pct;

            if (p == null) vm.Message = "Vui lòng chọn sản phẩm.";
            else if (!PriceHelper.TryParseDecimal(percent, out pct) || pct <= 0 || pct > 100)
                vm.Message = "Tỷ lệ tăng phải lớn hơn 0 và không vượt quá 100%. Giá không thay đổi.";
            else
            {
                decimal oldPrice = p.Price;
                p.Price = PriceHelper.Round2(p.Price * (1 + pct / 100m));
                db.SaveChanges();
                vm.Success = true; vm.Result = p;
                vm.Message = "Tăng giá thành công: " + PriceHelper.Money(oldPrice) + " → " + PriceHelper.Money(p.Price);
            }
            return AdjustView(vm);
        }

        [HttpPost]
        public ActionResult Decrease(int? productId, string percent)
        {
            var vm = NewAdjust("Decrease", productId, percent);
            var p = productId.HasValue ? db.Products.Find(productId.Value) : null;
            decimal pct;

            if (p == null) vm.Message = "Vui lòng chọn sản phẩm.";
            else if (!PriceHelper.TryParseDecimal(percent, out pct) || pct <= 0 || pct >= 100)
                vm.Message = "Tỷ lệ giảm phải lớn hơn 0 và nhỏ hơn 100%. Giá không thay đổi.";
            else
            {
                decimal oldPrice = p.Price;
                decimal newPrice = PriceHelper.Round2(p.Price * (1 - pct / 100m));
                if (newPrice < PriceHelper.MinPriceAfterDiscount)
                    vm.Message = "Từ chối: giá sau giảm " + PriceHelper.Money(newPrice) + " nhỏ hơn 1.000 đ. Giá trong CSDL giữ nguyên " + PriceHelper.Money(oldPrice) + ".";
                else
                {
                    p.Price = newPrice;
                    db.SaveChanges();
                    vm.Success = true; vm.Result = p;
                    vm.Message = "Giảm giá thành công: " + PriceHelper.Money(oldPrice) + " → " + PriceHelper.Money(p.Price);
                }
            }
            return AdjustView(vm);
        }

        // ===================== CÂU 5: GIẢM GIÁ 2 SẢN PHẨM (TRANSACTION) =====================
        private ActionResult DiscountView(DiscountTwoVM vm)
        {
            vm.Products = db.Products.OrderBy(p => p.ProductID).ToList();
            return View("DiscountTwo", vm);
        }

        public ActionResult DiscountTwo() { return DiscountView(new DiscountTwoVM()); }

        [HttpPost]
        public ActionResult DiscountTwo(int? productA, int? productB, string percentA, string percentB)
        {
            var vm = new DiscountTwoVM { ProductA = productA, ProductB = productB, PercentA = percentA, PercentB = percentB };

            if (!productA.HasValue || !productB.HasValue) { vm.Message = "Vui lòng chọn đủ hai sản phẩm."; return DiscountView(vm); }
            if (productA == productB) { vm.Message = "Hai sản phẩm phải khác nhau."; return DiscountView(vm); }

            var a = db.Products.Find(productA.Value);
            var b = db.Products.Find(productB.Value);
            decimal pa, pb;
            if (a == null || b == null) { vm.Message = "Không tìm thấy sản phẩm."; return DiscountView(vm); }

            if (!PriceHelper.TryParseDecimal(percentA, out pa) || pa <= 0 || pa >= 100 ||
                !PriceHelper.TryParseDecimal(percentB, out pb) || pb <= 0 || pb >= 100)
            { vm.Message = "Mỗi tỷ lệ giảm phải lớn hơn 0 và nhỏ hơn 100%. Từ chối toàn bộ yêu cầu."; return DiscountView(vm); }

            decimal newA = PriceHelper.Round2(a.Price * (1 - pa / 100m));
            decimal newB = PriceHelper.Round2(b.Price * (1 - pb / 100m));

            if (newA < PriceHelper.MinPriceAfterDiscount || newB < PriceHelper.MinPriceAfterDiscount)
            {
                vm.Message = "Từ chối toàn bộ yêu cầu: giá dự kiến A = " + PriceHelper.Money(newA) + ", B = " + PriceHelper.Money(newB)
                           + " (mỗi giá phải ≥ 1.000 đ). Không sản phẩm nào bị thay đổi.";
                return DiscountView(vm);
            }

            // Cả hai hợp lệ -> cập nhật trong MỘT transaction
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    a.Price = newA;
                    b.Price = newB;
                    db.SaveChanges();
                    tx.Commit();
                    vm.Success = true;
                    vm.Message = "Thành công (transaction commit): " + a.ProductName + " = " + PriceHelper.Money(newA)
                               + "; " + b.ProductName + " = " + PriceHelper.Money(newB);
                }
                catch (Exception ex)
                {
                    tx.Rollback();                                   // lỗi -> hoàn tác cả hai
                    vm.Message = "Lỗi, đã rollback, không sản phẩm nào bị thay đổi: " + ex.Message;
                }
            }
            return DiscountView(vm);
        }

        // ===================== CÂU 6: THÊM SẢN PHẨM =====================
        public ActionResult Create() { return View(new CreateVM()); }

        [HttpPost]
        public ActionResult Create(string productName, string category, string price, string quantity)
        {
            var vm = new CreateVM { ProductName = productName, Category = category, Price = price, Quantity = quantity };
            decimal pr; int qty;

            if (string.IsNullOrWhiteSpace(productName)) vm.Error = "Tên sản phẩm không được để trống.";
            else if (string.IsNullOrWhiteSpace(category)) vm.Error = "Danh mục không được để trống.";
            else if (!PriceHelper.TryParseDecimal(price, out pr) || pr <= 0) vm.Error = "Đơn giá phải lớn hơn 0.";
            else if (pr > PriceHelper.MaxPrice) vm.Error = "Đơn giá quá lớn.";
            else if (!int.TryParse((quantity ?? "").Trim(), out qty) || qty < 0) vm.Error = "Số lượng phải là số nguyên không âm.";
            else
            {
                db.Products.Add(new Product
                {
                    ProductName = productName.Trim(),
                    Category = category.Trim(),
                    Price = PriceHelper.Round2(pr),
                    Quantity = qty                         // ProductID để SQL Server tự tăng
                });
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        // ===================== CÂU 7: THỐNG KÊ =====================
        public ActionResult Stats()
        {
            var vm = new StatsVM
            {
                Total = db.Products.Count(),
                HighCount = db.Products.Count(p => p.Price >= 500000m),
                TotalValue = db.Products.Sum(p => (decimal?)(p.Price * p.Quantity)) ?? 0m
            };
            return View(vm);
        }
    }
}
