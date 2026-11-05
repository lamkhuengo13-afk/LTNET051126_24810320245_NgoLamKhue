/* BAI TAP 6.2 - HRMDB. Chay script nay mot lan trong SSMS. */
IF DB_ID(N'HRMDB') IS NULL CREATE DATABASE HRMDB;
GO
USE HRMDB;
GO
IF OBJECT_ID(N'dbo.PhongBan',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.PhongBan(MaPB int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhongBan PRIMARY KEY,TenPB nvarchar(100) NOT NULL,MoTa nvarchar(255) NULL);
END;
GO
IF OBJECT_ID(N'dbo.NhanVien',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.NhanVien(
  MaNV int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
  HoTen nvarchar(100) NOT NULL,NgaySinh datetime NULL,CCCD varchar(20) NULL,
  MaPB int NOT NULL,LuongCoBan decimal(18,2) NOT NULL,HeSoLuong float NOT NULL CONSTRAINT DF_NhanVien_HeSoLuong DEFAULT(1.0),
  CONSTRAINT UQ_NhanVien_CCCD UNIQUE(CCCD),
  CONSTRAINT CK_NhanVien_Luong CHECK(LuongCoBan>0),
  CONSTRAINT CK_NhanVien_HeSo CHECK(HeSoLuong>0),
  CONSTRAINT FK_NhanVien_PhongBan FOREIGN KEY(MaPB) REFERENCES dbo.PhongBan(MaPB)
 );
END;
GO
IF OBJECT_ID(N'dbo.BangChamCong',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.BangChamCong(
  MaCong int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BangChamCong PRIMARY KEY,
  MaNV int NOT NULL,ThangNam varchar(7) NOT NULL,
  SoNgayCong float NOT NULL CONSTRAINT DF_BangChamCong_NgayCong DEFAULT(0),
  SoGioOT float NOT NULL CONSTRAINT DF_BangChamCong_OT DEFAULT(0),
  CONSTRAINT CK_BangChamCong_NgayCong CHECK(SoNgayCong>=0 AND SoNgayCong<=31),
  CONSTRAINT CK_BangChamCong_OT CHECK(SoGioOT>=0),
  CONSTRAINT UQ_BangChamCong_NhanVien_Thang UNIQUE(MaNV,ThangNam),
  CONSTRAINT FK_BangChamCong_NhanVien FOREIGN KEY(MaNV) REFERENCES dbo.NhanVien(MaNV)
 );
END;
GO
IF OBJECT_ID(N'dbo.BangLuong',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.BangLuong(
  MaLuong int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BangLuong PRIMARY KEY,
  MaNV int NOT NULL,ThangNam varchar(7) NOT NULL,ThucLinh decimal(18,2) NOT NULL,
  CONSTRAINT UQ_BangLuong_NhanVien_Thang UNIQUE(MaNV,ThangNam),
  CONSTRAINT FK_BangLuong_NhanVien FOREIGN KEY(MaNV) REFERENCES dbo.NhanVien(MaNV)
 );
END;
GO
/* Trigger khoa du lieu cham cong sau khi thang da duoc chot luong. */
CREATE OR ALTER TRIGGER dbo.TR_BangChamCong_LockAfterPayroll
ON dbo.BangChamCong
AFTER INSERT,UPDATE,DELETE
AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.BangLuong l ON l.MaNV=i.MaNV AND l.ThangNam=i.ThangNam)
    OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.BangLuong l ON l.MaNV=d.MaNV AND l.ThangNam=d.ThangNam)
 BEGIN
  ROLLBACK TRANSACTION;
  THROW 51001,N'Thang cham cong da chot luong, khong the them/sua/xoa.',1;
 END
END;
GO
/* Du lieu mau - chi them neu bang dang rong. */
IF NOT EXISTS(SELECT 1 FROM dbo.PhongBan)
BEGIN
 INSERT dbo.PhongBan(TenPB,MoTa) VALUES
 (N'Ban Giám đốc',N'Quản lý và điều hành'),
 (N'Phòng Kỹ thuật',N'Vận hành hệ thống kỹ thuật'),
 (N'Phòng Kinh doanh',N'Kinh doanh và chăm sóc khách hàng');
END;
GO
IF NOT EXISTS(SELECT 1 FROM dbo.NhanVien)
BEGIN
 DECLARE @PBKT int=(SELECT TOP 1 MaPB FROM dbo.PhongBan WHERE TenPB=N'Phòng Kỹ thuật');
 DECLARE @PBKD int=(SELECT TOP 1 MaPB FROM dbo.PhongBan WHERE TenPB=N'Phòng Kinh doanh');
 DECLARE @PBGD int=(SELECT TOP 1 MaPB FROM dbo.PhongBan WHERE TenPB=N'Ban Giám đốc');
 INSERT dbo.NhanVien(HoTen,NgaySinh,CCCD,MaPB,LuongCoBan,HeSoLuong) VALUES
 (N'Nguyễn Minh Anh','2001-03-12','001201000001',@PBKT,12000000,1.5),
 (N'Trần Quốc Bảo','2000-07-22','001200000002',@PBKD,11000000,1.3),
 (N'Lê Thu Hà','1999-11-05','001199000003',@PBGD,18000000,2.0),
 (N'Phạm Đức Long','2002-01-18','001202000004',@PBKT,10000000,1.2),
 (N'Hoàng Ngọc Mai','2001-09-30','001201000005',@PBKD,10500000,1.25);
END;
GO
