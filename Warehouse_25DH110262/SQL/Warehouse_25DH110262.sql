
IF DB_ID(N'Warehouse_25DH110262') IS NULL CREATE DATABASE Warehouse_25DH110262;
GO
USE Warehouse_25DH110262;
GO
IF OBJECT_ID(N'dbo.Products') IS NOT NULL DROP TABLE dbo.Products;
GO
CREATE TABLE dbo.Products(
    ProductID   int IDENTITY(1,1) PRIMARY KEY,
    ProductName nvarchar(100) NOT NULL,
    Category    nvarchar(100) NOT NULL,
    Price       decimal(18,2) NOT NULL,
    Quantity    int NOT NULL
);
GO
INSERT INTO dbo.Products(ProductName, Category, Price, Quantity) VALUES
(N'Bút bi Nguyen Truong Vu',         N'Văn phòng phẩm Nguyen Truong Vu',  90000,   10),  -- Giá thấp
(N'Sổ tay Nguyen Truong Vu',         N'Văn phòng phẩm Nguyen Truong Vu',  200000,  5),   -- Giá trung bình
(N'Tai nghe Nguyen Truong Vu',       N'Điện tử Nguyen Truong Vu',         500000,  3),   -- Giá cao (biên 500.000)
(N'Chuột không dây Nguyen Truong Vu',N'Điện tử Nguyen Truong Vu',         100000,  7),   -- biên 100.000
(N'Bàn phím cơ Nguyen Truong Vu',    N'Điện tử Nguyen Truong Vu',         1500000, 2),
(N'Gôm tẩy Nguyen Truong Vu',        N'Văn phòng phẩm Nguyen Truong Vu',  1500,    50);  -- test giảm 50% => 750 (bị từ chối)
GO
SELECT * FROM dbo.Products;
GO


