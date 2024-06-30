// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using MySql.Data.MySqlClient;

string connectionString = "server=localhost;port=3306;database=hw16;user=root;password=Root0123456789";

using var connection = new MySqlConnection(connectionString);

connection.Open();

const int N = 100_000;

var sw = Stopwatch.StartNew();

for (int i = 1; i <= N; i++)
{
    var id = Guid.NewGuid();

    using var command = new MySqlCommand(
        "INSERT INTO users (`username`, `age`, `email`, `height`, `weight`) " +
        $"VALUES ('{id}', {Random.Shared.Next(0, 100)}, 'email', {Random.Shared.Next(0, 200)}, {Random.Shared.Next(0, 100)})",
        connection);

    command.ExecuteNonQuery();
    
    if (i % 1000 == 0)
    {
        Console.WriteLine($"Inserted {i} / {N} records successfully.");
    }
}

sw.Stop();

Console.WriteLine($"Inserted {N} records successfully in {sw.Elapsed}.");