/*
  SalesDB - Script khoi tao an toan.
  Script chi tao database/bang/index neu chua ton tai; KHONG DROP va KHONG XOA DU LIEU.
  Neu SalesDB da duoc tao va dang su dung, khong can chay lai script nay.
*/
IF DB_ID(N'SalesDB') IS NULL
    CREATE DATABASE SalesDB;
GO
USE SalesDB;
GO

IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhanVien
    (
        MaNV INT IDENTITY(1,1) PRIMARY KEY,
        TenNV NVARCHAR(100) NOT NULL,
        SoDienThoai VARCHAR(15) NULL,
        ChucVu NVARCHAR(50) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang
    (
        MaKH INT IDENTITY(1,1) PRIMARY KEY,
        TenKH NVARCHAR(100) NOT NULL,
        DienThoai VARCHAR(15) NULL,
        DiemTichLuy INT NOT NULL CONSTRAINT DF_KhachHang_Diem DEFAULT 0
            CONSTRAINT CK_KhachHang_Diem CHECK (DiemTichLuy >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.SanPham', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SanPham
    (
        MaSP VARCHAR(20) NOT NULL PRIMARY KEY,
        TenSP NVARCHAR(150) NOT NULL,
        GiaBan DECIMAL(18,2) NOT NULL CHECK (GiaBan > 0),
        SoLuongTon INT NOT NULL CONSTRAINT DF_SanPham_Ton DEFAULT 0 CHECK (SoLuongTon >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon
    (
        MaHD INT IDENTITY(1,1) PRIMARY KEY,
        NgayLap DATETIME NOT NULL CONSTRAINT DF_HoaDon_Ngay DEFAULT GETDATE(),
        MaNV INT NOT NULL,
        MaKH INT NULL,
        TongTien DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_Tong DEFAULT 0 CHECK (TongTien >= 0),
        CONSTRAINT FK_HoaDon_NhanVien FOREIGN KEY (MaNV) REFERENCES dbo.NhanVien(MaNV),
        CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
    );
END;
GO

IF OBJECT_ID(N'dbo.ChiTietHoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietHoaDon
    (
        MaHD INT NOT NULL,
        MaSP VARCHAR(20) NOT NULL,
        SoLuong INT NOT NULL CHECK (SoLuong > 0),
        DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia > 0),
        CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY (MaHD, MaSP),
        CONSTRAINT FK_ChiTiet_HoaDon FOREIGN KEY (MaHD) REFERENCES dbo.HoaDon(MaHD),
        CONSTRAINT FK_ChiTiet_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP)
    );
END;
GO


/* Cho phep nhieu ban ghi khong co so dien thoai, nhung so da nhap van duy nhat. */
DECLARE @uqKhachHang sysname;
SELECT TOP (1) @uqKhachHang = kc.name
FROM sys.key_constraints kc
JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
JOIN sys.columns col ON col.object_id = ic.object_id AND col.column_id = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID(N'dbo.KhachHang') AND kc.type = 'UQ' AND col.name = N'DienThoai';
IF @uqKhachHang IS NOT NULL
    EXEC(N'ALTER TABLE dbo.KhachHang DROP CONSTRAINT ' + QUOTENAME(@uqKhachHang));

DECLARE @uqNhanVien sysname;
SELECT TOP (1) @uqNhanVien = kc.name
FROM sys.key_constraints kc
JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
JOIN sys.columns col ON col.object_id = ic.object_id AND col.column_id = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID(N'dbo.NhanVien') AND kc.type = 'UQ' AND col.name = N'SoDienThoai';
IF @uqNhanVien IS NOT NULL
    EXEC(N'ALTER TABLE dbo.NhanVien DROP CONSTRAINT ' + QUOTENAME(@uqNhanVien));

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_KhachHang_DienThoai' AND object_id=OBJECT_ID(N'dbo.KhachHang'))
    CREATE UNIQUE INDEX UX_KhachHang_DienThoai ON dbo.KhachHang(DienThoai) WHERE DienThoai IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_NhanVien_SoDienThoai' AND object_id=OBJECT_ID(N'dbo.NhanVien'))
    CREATE UNIQUE INDEX UX_NhanVien_SoDienThoai ON dbo.NhanVien(SoDienThoai) WHERE SoDienThoai IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_SanPham_TenSP' AND object_id=OBJECT_ID(N'dbo.SanPham'))
    CREATE INDEX IX_SanPham_TenSP ON dbo.SanPham(TenSP);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_HoaDon_NgayLap' AND object_id=OBJECT_ID(N'dbo.HoaDon'))
    CREATE INDEX IX_HoaDon_NgayLap ON dbo.HoaDon(NgayLap);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ChiTietHoaDon_MaSP' AND object_id=OBJECT_ID(N'dbo.ChiTietHoaDon'))
    CREATE INDEX IX_ChiTietHoaDon_MaSP ON dbo.ChiTietHoaDon(MaSP);
GO
