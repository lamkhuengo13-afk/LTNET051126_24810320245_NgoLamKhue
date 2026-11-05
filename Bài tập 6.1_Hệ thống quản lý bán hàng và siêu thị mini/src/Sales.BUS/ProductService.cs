using Sales.DAL;
using Sales.DTO;
namespace Sales.BUS;
public sealed class ProductService
{
    private readonly ProductRepository repo = new();
    public Task<List<Product>> GetAllAsync(string keyword = "") => repo.GetAllAsync(keyword);
    public Task SaveAsync(Product p, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(p.MaSP) || string.IsNullOrWhiteSpace(p.TenSP))
            throw new ArgumentException("Mã và tên sản phẩm không được để trống.");
        if (p.MaSP.Trim().Length > 20) throw new ArgumentException("Mã sản phẩm tối đa 20 ký tự.");
        if (p.TenSP.Trim().Length > 150) throw new ArgumentException("Tên sản phẩm tối đa 150 ký tự.");
        if (p.GiaBan <= 0 || p.SoLuongTon < 0) throw new ArgumentException("Giá phải lớn hơn 0 và tồn kho không được âm.");
        return repo.SaveAsync(p, isNew);
    }
    public Task DeleteAsync(string id) => repo.DeleteAsync(id);
}
