using Microsoft.Data.SqlClient;
namespace HRM.DAL;
public static class Db
{
    public static string ConnectionString { get; set; } = @"Server=(localdb)\MSSQLLocalDB;Database=HRMDB;Integrated Security=True;TrustServerCertificate=True;";
    public static SqlConnection CreateConnection() => new(ConnectionString);
    public static void TestConnection() { using var c=CreateConnection(); c.Open(); }
}
