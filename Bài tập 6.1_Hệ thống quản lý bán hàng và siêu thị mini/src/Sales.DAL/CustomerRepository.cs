using Microsoft.Data.SqlClient; using Sales.DTO;
namespace Sales.DAL;
public sealed class CustomerRepository {
 public async Task<List<Customer>> GetAllAsync(){var a=new List<Customer>(); await using var c=Db.CreateConnection(); await c.OpenAsync(); await using var cmd=new SqlCommand("SELECT MaKH,TenKH,DienThoai,DiemTichLuy FROM KhachHang ORDER BY TenKH",c); await using var r=await cmd.ExecuteReaderAsync(); while(await r.ReadAsync()) a.Add(new Customer{MaKH=r.GetInt32(0),TenKH=r.GetString(1),DienThoai=r.IsDBNull(2)?null:r.GetString(2),DiemTichLuy=r.GetInt32(3)}); return a;}
 public async Task SaveAsync(Customer x,bool isNew){await using var c=Db.CreateConnection(); await c.OpenAsync(); var sql=isNew?"INSERT INTO KhachHang(TenKH,DienThoai) VALUES(@n,@p)":"UPDATE KhachHang SET TenKH=@n,DienThoai=@p WHERE MaKH=@id"; await using var cmd=new SqlCommand(sql,c); cmd.Parameters.AddWithValue("@n",x.TenKH); cmd.Parameters.AddWithValue("@p",(object?)x.DienThoai??DBNull.Value); if(!isNew)cmd.Parameters.AddWithValue("@id",x.MaKH); await cmd.ExecuteNonQueryAsync();}
 public async Task DeleteAsync(int id){await using var c=Db.CreateConnection(); await c.OpenAsync(); await using var cmd=new SqlCommand("DELETE FROM KhachHang WHERE MaKH=@id",c);cmd.Parameters.AddWithValue("@id",id);await cmd.ExecuteNonQueryAsync();}
}
