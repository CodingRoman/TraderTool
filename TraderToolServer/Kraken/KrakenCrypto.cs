using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

public class KrakenCrypto
{
    private static readonly HttpClient client = new HttpClient();

    public async Task<List<string>> GetAvailableCryptocurrenciesInGermanyAsync()
    {
        var url = "https://api.kraken.com/0/public/AssetPairs";
        var response = await client.GetFromJsonAsync<KrakenResponse>(url);

        if (response == null || response.Result == null)
        {
            throw new Exception("Failed to fetch data from Kraken API.");
        }

        var availableCryptos = new List<string>();

        foreach (var pair in response.Result)
        {
            if (pair.Value.Wsname.Contains("EUR"))
            {
                availableCryptos.Add(pair.Key);
            }
        }

        return availableCryptos;
    }

    public async Task<TickerData> GetTickerDataAsync(string crypto)
    {
        var url = $"https://api.kraken.com/0/public/Ticker?pair={crypto}";
        var response = await client.GetFromJsonAsync<KrakenTickerResponse>(url);

        if (response == null || response.Result == null)
        {
            throw new Exception($"Failed to fetch ticker data for {crypto} from Kraken API.");
        }

        var tickerInfo = response.Result.Values.First();
        return new TickerData
        {
            Price = decimal.Parse(tickerInfo.C[0])
        };
    }

    public void SaveCryptocurrenciesToDatabase(List<string> cryptocurrencies)
    {
        using (var connection = new SqliteConnection("Data Source=tradingtool.db"))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cryptocurrencies (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE
                )";
            tableCmd.ExecuteNonQuery();

            foreach (var crypto in cryptocurrencies)
            {
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(1) FROM Cryptocurrencies WHERE Name = @name";
                checkCmd.Parameters.AddWithValue("@name", crypto);

                var exists = (long)checkCmd.ExecuteScalar() > 0;

                if (!exists)
                {
                    var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO Cryptocurrencies (Name) VALUES (@name)";
                    insertCmd.Parameters.AddWithValue("@name", crypto);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
    }

    public void SaveTickerDataToDatabase(string crypto, TickerData tickerData)
    {
        using (var connection = new SqliteConnection("Data Source=tradingtool.db"))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS TickerData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Crypto TEXT NOT NULL,
                    Price DECIMAL NOT NULL,
                    Timestamp DATETIME NOT NULL,
                    UNIQUE(Crypto, Timestamp)
                )";
            tableCmd.ExecuteNonQuery();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(1) FROM TickerData WHERE Crypto = @crypto AND Timestamp = @timestamp";
            checkCmd.Parameters.AddWithValue("@crypto", crypto);
            checkCmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);

            var exists = (long)checkCmd.ExecuteScalar() > 0;

            if (!exists)
            {
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO TickerData (Crypto, Price, Timestamp) VALUES (@crypto, @price, @timestamp)";
                insertCmd.Parameters.AddWithValue("@crypto", crypto);
                insertCmd.Parameters.AddWithValue("@price", tickerData.Price);
                insertCmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);
                insertCmd.ExecuteNonQuery();
            }
        }
    }

    public List<string> GetCryptocurrenciesFromDatabase()
    {
        var cryptocurrencies = new List<string>();

        using (var connection = new SqliteConnection("Data Source=tradingtool.db"))
        {
            connection.Open();
            var command = new SqliteCommand("SELECT Name FROM Cryptocurrencies", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    cryptocurrencies.Add(reader.GetString(0));
                }
            }
        }

        return cryptocurrencies;
    }
}



public class TickerData
{
    public decimal Price { get; set; }
}

public class KrakenResponse
{
    public Dictionary<string, AssetPair> Result { get; set; }
}

public class AssetPair
{
    public string Wsname { get; set; }
}

public class KrakenTickerResponse
{
    public Dictionary<string, TickerInfo> Result { get; set; }
}

public class TickerInfo
{
    public string[] C { get; set; } // Closing price
}
