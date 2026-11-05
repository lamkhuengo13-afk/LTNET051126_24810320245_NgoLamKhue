using Sales.DAL;
using Sales.DTO;
namespace Sales.BUS;
public sealed class CustomerService
{
    private readonly CustomerRepository repo = new();
    public Task<List<Customer>> GetAllAsync() => repo.GetAllAsync();
    public Task SaveAsync(Customer x, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(x.TenKH)) throw new ArgumentException("Tên khách hàng không được để trống.");
        if (x.TenKH.Trim().Length > 100) throw new ArgumentException("Tên khách hàng tối đa 100 ký tự.");
        if (x.DienThoai?.Length > 15) throw new ArgumentException("Số điện thoại tối đa 15 ký tự.");
        return repo.SaveAsync(x, isNew);
    }
    public Task DeleteAsync(int id) => repo.DeleteAsync(id);
}
