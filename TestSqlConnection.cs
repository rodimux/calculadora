using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        var connectionString = "Server=.\\SQLEXPRESS;Database=CalculadoraCostesDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True";
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        Console.WriteLine("Connection opened successfully");
    }
}
