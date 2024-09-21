using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class CryptoDataService : IHostedService
{
    private readonly KrakenCrypto _krakenCrypto;
    private readonly ILogger<CryptoDataService> _logger;

    public CryptoDataService(KrakenCrypto krakenCrypto, ILogger<CryptoDataService> logger)
    {
        _krakenCrypto = krakenCrypto;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting CryptoDataService...");

        try
        {
            var availableCryptos = await _krakenCrypto.GetAvailableCryptocurrenciesInGermanyAsync();
            _krakenCrypto.SaveCryptocurrenciesToDatabase(availableCryptos);
            _logger.LogInformation("Cryptocurrencies data saved to SQLite database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching and saving cryptocurrencies data.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping CryptoDataService...");
        return Task.CompletedTask;
    }
}
