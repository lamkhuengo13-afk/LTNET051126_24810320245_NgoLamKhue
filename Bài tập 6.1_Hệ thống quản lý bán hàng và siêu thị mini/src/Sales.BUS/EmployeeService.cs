using Sales.DAL;
using Sales.DTO;
namespace Sales.BUS;
public sealed class EmployeeService
{
    private readonly EmployeeRepository repo = new();
    public Task<List<Employee>> GetAllAsync() => repo.GetAllAsync();
    public Task SaveAsync(Employee x, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(x.TenNV)) throw new ArgumentException("Tên nhân viên không được để trống.");
        if (x.TenNV.Trim().Length > 100) throw new ArgumentException("Tên nhân viên tối đa 100 ký tự.");
        if (x.SoDienThoai?.Length > 15) throw new ArgumentException("Số điện thoại tối đa 15 ký tự.");
        if (x.ChucVu?.Length > 50) throw new ArgumentException("Chức vụ tối đa 50 ký tự.");
        return repo.SaveAsync(x, isNew);
    }
    public Task DeleteAsync(int id) => repo.DeleteAsync(id);
}
