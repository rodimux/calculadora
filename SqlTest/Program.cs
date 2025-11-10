using Microsoft.Data.SqlClient;

Console.WriteLine("Testing connection...");
var connectionString = "Server=.\\SQLEXPRESS;Database=CalculadoraCostesDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True";
using var conn = new SqlConnection(connectionString);
conn.Open();
Console.WriteLine("Opened successfully");
