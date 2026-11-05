using Sales.DAL;
using Sales.DTO;
using System.Data;
namespace Sales.BUS;
public sealed class SalesService
{
    private readonly SalesRepository repo = new();
    public Task<int> CheckoutAsync(int employeeId, int? customerId, IReadOnlyCollection<CartItem> items)
        => repo.CheckoutAsync(employeeId, customerId, items);
    public Task<decimal> RevenueAsync(DateTime from, DateTime to)
    {
        if (to <= from) throw new ArgumentException("Khoảng thời gian thống kê không hợp lệ.");
        return repo.RevenueAsync(from, to);
    }
    public Task<DataTable> TopProductsAsync() => repo.TopProductsAsync();
}
