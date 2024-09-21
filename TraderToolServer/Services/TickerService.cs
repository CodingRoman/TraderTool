using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;

public class TickerService : BackgroundService
{
    private readonly KrakenCrypto _krakenCrypto;
    private readonly ILogger<TickerService> _logger;
    private readonly IHubContext<TickerHub> _hubContext;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Adjust the interval as needed

    public TickerService(KrakenCrypto krakenCrypto, ILogger<TickerService> logger, IHubContext<TickerHub> hubContext)
    {
        _krakenCrypto = krakenCrypto;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Fetching ticker data...");

            try
            {
                var availableCryptos = await _krakenCrypto.GetAvailableCryptocurrenciesInGermanyAsync();
                foreach (var crypto in availableCryptos)
                {
                    var tickerData = await _krakenCrypto.GetTickerDataAsync(crypto);
                    _krakenCrypto.SaveTickerDataToDatabase(crypto, tickerData);
                }
                _logger.LogInformation("Ticker data fetched and saved to SQLite database.");

                // Notify clients that new data is available
                await _hubContext.Clients.All.SendAsync("NewTickerDataAvailable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching ticker data.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
