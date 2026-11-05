namespace HRM.DTO;
public sealed class Department { public int MaPB { get; set; } public string TenPB { get; set; } = ""; public string? MoTa { get; set; } }
public sealed class Employee { public int MaNV { get; set; } public string HoTen { get; set; } = ""; public DateTime? NgaySinh { get; set; } public string? CCCD { get; set; } public int MaPB { get; set; } public decimal LuongCoBan { get; set; } public double HeSoLuong { get; set; } public string TenPB { get; set; } = ""; }
public sealed class PayrollRow { public int MaNV { get; set; } public string HoTen { get; set; } = ""; public string ThangNam { get; set; } = ""; public decimal ThucLinh { get; set; } }
