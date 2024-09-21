using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace TraderToolServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TickerController : ControllerBase
    {
        private readonly ILogger<TickerController> _logger;

        public TickerController(ILogger<TickerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTickerData()
        {
            _logger.LogInformation("Fetching all ticker data...");

            try
            {
                var tickerData = await GetTickerDataFromDatabase();
                return Ok(tickerData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all ticker data.");
                return StatusCode(500, "An error occurred while fetching all ticker data.");
            }
        }

        [HttpGet("{crypto}")]
        public async Task<IActionResult> GetTickerData(string crypto)
        {
            _logger.LogInformation($"Fetching ticker data for {crypto}...");

            try
            {
                var tickerData = await GetTickerDataFromDatabase(crypto);
                return Ok(tickerData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching ticker data for {crypto}.");
                return StatusCode(500, $"An error occurred while fetching ticker data for {crypto}.");
            }
        }

        private async Task<List<TickerData>> GetTickerDataFromDatabase(string crypto = null)
        {
            var tickerDataList = new List<TickerData>();

            using (var connection = new SqliteConnection("Data Source=tradingtool.db"))
            {
                await connection.OpenAsync();

                var queryCmd = connection.CreateCommand();
                if (crypto == null)
                {
                    queryCmd.CommandText = "SELECT Crypto, Price, Timestamp FROM TickerData";
                }
                else
                {
                    queryCmd.CommandText = "SELECT Crypto, Price, Timestamp FROM TickerData WHERE Crypto = @crypto";
                    queryCmd.Parameters.AddWithValue("@crypto", crypto);
                }

                using (var reader = await queryCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tickerDataList.Add(new TickerData
                        {
                            Crypto = reader.GetString(0),
                            Price = reader.GetDecimal(1),
                            Timestamp = reader.GetDateTime(2)
                        });
                    }
                }
            }

            return tickerDataList;
        }
    }

    public class TickerData
    {
        public string Crypto { get; set; }
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
