using System.Data;
using Microsoft.Data.SqlClient;
using HRM.DTO;
namespace HRM.DAL;

public sealed class DepartmentRepository
{
    public List<Department> GetAll(){var a=new List<Department>(); using var c=Db.CreateConnection(); c.Open(); using var q=new SqlCommand("SELECT MaPB,TenPB,MoTa FROM PhongBan ORDER BY TenPB",c); using var r=q.ExecuteReader(); while(r.Read()) a.Add(new(){MaPB=r.GetInt32(0),TenPB=r.GetString(1),MoTa=r.IsDBNull(2)?null:r.GetString(2)}); return a;}
    public void Save(Department d,bool isNew){using var c=Db.CreateConnection();c.Open();using var q=new SqlCommand(isNew?"INSERT INTO PhongBan(TenPB,MoTa) VALUES(@n,@m)":"UPDATE PhongBan SET TenPB=@n,MoTa=@m WHERE MaPB=@id",c);q.Parameters.Add("@n",SqlDbType.NVarChar,100).Value=d.TenPB.Trim();q.Parameters.Add("@m",SqlDbType.NVarChar,255).Value=(object?)d.MoTa??DBNull.Value;if(!isNew)q.Parameters.Add("@id",SqlDbType.Int).Value=d.MaPB;q.ExecuteNonQuery();}
    public void Delete(int id){using var c=Db.CreateConnection();c.Open();using var q=new SqlCommand("DELETE FROM PhongBan WHERE MaPB=@id",c);q.Parameters.Add("@id",SqlDbType.Int).Value=id;q.ExecuteNonQuery();}
}
public sealed class EmployeeRepository
{
    private const string SelectSql="SELECT nv.MaNV,nv.HoTen,nv.NgaySinh,nv.CCCD,nv.MaPB,nv.LuongCoBan,nv.HeSoLuong,p.TenPB FROM NhanVien nv INNER JOIN PhongBan p ON p.MaPB=nv.MaPB";
    public List<Employee> Search(string keyword="",int? departmentId=null){var a=new List<Employee>();using var c=Db.CreateConnection();c.Open();using var q=new SqlCommand(SelectSql+" WHERE (@kw=N'' OR nv.HoTen LIKE @like OR nv.CCCD LIKE @like OR CONVERT(varchar(12),nv.MaNV) LIKE @like) AND (@pb IS NULL OR nv.MaPB=@pb) ORDER BY nv.MaNV DESC",c);q.Parameters.Add("@kw",SqlDbType.NVarChar,100).Value=keyword.Trim();q.Parameters.Add("@like",SqlDbType.NVarChar,120).Value="%"+keyword.Trim()+"%";q.Parameters.Add("@pb",SqlDbType.Int).Value=(object?)departmentId??DBNull.Value;using var r=q.ExecuteReader();while(r.Read())a.Add(new(){MaNV=r.GetInt32(0),HoTen=r.GetString(1),NgaySinh=r.IsDBNull(2)?null:r.GetDateTime(2),CCCD=r.IsDBNull(3)?null:r.GetString(3),MaPB=r.GetInt32(4),LuongCoBan=r.GetDecimal(5),HeSoLuong=Convert.ToDouble(r.GetValue(6)),TenPB=r.GetString(7)});return a;}
    public void Save(Employee e,bool isNew){using var c=Db.CreateConnection();c.Open();using var q=new SqlCommand(isNew?"INSERT INTO NhanVien(HoTen,NgaySinh,CCCD,MaPB,LuongCoBan,HeSoLuong) VALUES(@name,@dob,@cccd,@pb,@salary,@factor)":"UPDATE NhanVien SET HoTen=@name,NgaySinh=@dob,CCCD=@cccd,MaPB=@pb,LuongCoBan=@salary,HeSoLuong=@factor WHERE MaNV=@id",c);q.Parameters.Add("@name",SqlDbType.NVarChar,100).Value=e.HoTen.Trim();q.Parameters.Add("@dob",SqlDbType.DateTime).Value=(object?)e.NgaySinh??DBNull.Value;q.Parameters.Add("@cccd",SqlDbType.VarChar,20).Value=(object?)e.CCCD??DBNull.Value;q.Parameters.Add("@pb",SqlDbType.Int).Value=e.MaPB;q.Parameters.Add("@salary",SqlDbType.Decimal).Value=e.LuongCoBan;q.Parameters["@salary"].Precision=18;q.Parameters["@salary"].Scale=2;q.Parameters.Add("@factor",SqlDbType.Float).Value=e.HeSoLuong;if(!isNew)q.Parameters.Add("@id",SqlDbType.Int).Value=e.MaNV;q.ExecuteNonQuery();}
    public void Delete(int id){using var c=Db.CreateConnection();c.Open();using var q=new SqlCommand("DELETE FROM NhanVien WHERE MaNV=@id",c);q.Parameters.Add("@id",SqlDbType.Int).Value=id;q.ExecuteNonQuery();}
}
public sealed class AttendanceRepository
{
    public DataTable LoadMonth(string month)
    {
        using var c=Db.CreateConnection(); using var da=new SqlDataAdapter("SELECT MaCong,MaNV,ThangNam,SoNgayCong,SoGioOT FROM BangChamCong WHERE ThangNam=@month ORDER BY MaNV",c);
        da.SelectCommand!.Parameters.Add("@month",SqlDbType.VarChar,7).Value=month;
        da.MissingSchemaAction=MissingSchemaAction.AddWithKey;
        var dataSet=new DataSet("HRMDataSet"); da.FillSchema(dataSet,SchemaType.Source,"BangChamCong"); da.Fill(dataSet,"BangChamCong");
        var table=dataSet.Tables["BangChamCong"]!;
        if(!table.Columns.Contains("HoTen")) table.Columns.Add("HoTen",typeof(string));
        var employees=new List<(int Id,string Name)>(); using(var ec=Db.CreateConnection()){ec.Open();using var q=new SqlCommand("SELECT MaNV,HoTen FROM NhanVien ORDER BY MaNV",ec);using var r=q.ExecuteReader();while(r.Read())employees.Add((r.GetInt32(0),r.GetString(1)));}
        foreach(var emp in employees){var found=table.AsEnumerable().FirstOrDefault(x=>x.Field<int>("MaNV")==emp.Id);if(found==null){var row=table.NewRow();row["MaNV"]=emp.Id;row["ThangNam"]=month;row["SoNgayCong"]=0d;row["SoGioOT"]=0d;row["HoTen"]=emp.Name;table.Rows.Add(row);}else found["HoTen"]=emp.Name;}
        return table;
    }
    public int Save(DataTable table,string month)
    {
        using var c=Db.CreateConnection(); using var da=new SqlDataAdapter("SELECT MaCong,MaNV,ThangNam,SoNgayCong,SoGioOT FROM BangChamCong WHERE ThangNam=@month",c);
        da.SelectCommand!.Parameters.Add("@month",SqlDbType.VarChar,7).Value=month; da.MissingSchemaAction=MissingSchemaAction.AddWithKey;
        using var builder=new SqlCommandBuilder(da){ConflictOption=ConflictOption.OverwriteChanges};
        da.InsertCommand=builder.GetInsertCommand(); da.UpdateCommand=builder.GetUpdateCommand(); da.DeleteCommand=builder.GetDeleteCommand();
        foreach(DataRow row in table.Rows){if(row.RowState==DataRowState.Deleted)continue;row["ThangNam"]=month;}
        c.Open(); return da.Update(table);
    }
    public bool IsLocked(string month){using var c=Db.CreateConnection();c.Open();using var q=new SqlCommand("SELECT CASE WHEN EXISTS(SELECT 1 FROM BangLuong WHERE ThangNam=@m) THEN 1 ELSE 0 END",c);q.Parameters.Add("@m",SqlDbType.VarChar,7).Value=month;return (int)q.ExecuteScalar()! == 1;}
}
public sealed class PayrollRepository
{
    public int CloseMonth(string month)
    {
        using var c=Db.CreateConnection();c.Open();using var tx=c.BeginTransaction(IsolationLevel.Serializable);
        try{
            using(var check=new SqlCommand("SELECT COUNT(*) FROM BangLuong WHERE ThangNam=@m",c,tx)){check.Parameters.Add("@m",SqlDbType.VarChar,7).Value=month;if((int)check.ExecuteScalar()!>0)throw new InvalidOperationException("Tháng này đã được chốt lương, dữ liệu chấm công đã khóa.");}
            using(var check=new SqlCommand("SELECT COUNT(*) FROM NhanVien nv WHERE NOT EXISTS(SELECT 1 FROM BangChamCong bc WHERE bc.MaNV=nv.MaNV AND bc.ThangNam=@m)",c,tx)){check.Parameters.Add("@m",SqlDbType.VarChar,7).Value=month;int missing=(int)check.ExecuteScalar()!;if(missing>0)throw new InvalidOperationException($"Còn {missing} nhân viên chưa có dòng chấm công. Hãy tải và lưu bảng chấm công trước khi chốt lương.");}
            using var cmd=new SqlCommand(@"INSERT INTO BangLuong(MaNV,ThangNam,ThucLinh)
SELECT nv.MaNV,@m,CAST(ROUND((nv.LuongCoBan*CAST(nv.HeSoLuong AS decimal(18,4))/22.0)*bc.SoNgayCong + (bc.SoGioOT*50000.0),2) AS decimal(18,2))
FROM NhanVien nv INNER JOIN BangChamCong bc ON bc.MaNV=nv.MaNV AND bc.ThangNam=@m;",c,tx);
            cmd.Parameters.Add("@m",SqlDbType.VarChar,7).Value=month;int count=cmd.ExecuteNonQuery();tx.Commit();return count;
        }catch{tx.Rollback();throw;}
    }
    public List<PayrollRow> GetMonth(string month){var a=new List<PayrollRow>();using var c=Db.CreateConnection();c.Open();using var q=new SqlCommand("SELECT l.MaNV,n.HoTen,l.ThangNam,l.ThucLinh FROM BangLuong l JOIN NhanVien n ON n.MaNV=l.MaNV WHERE l.ThangNam=@m ORDER BY l.MaNV",c);q.Parameters.Add("@m",SqlDbType.VarChar,7).Value=month;using var r=q.ExecuteReader();while(r.Read())a.Add(new(){MaNV=r.GetInt32(0),HoTen=r.GetString(1),ThangNam=r.GetString(2),ThucLinh=r.GetDecimal(3)});return a;}
}
