using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TraderToolServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly KrakenCrypto _krakenCrypto;
        private readonly ILogger<CryptoController> _logger;

        public CryptoController(KrakenCrypto krakenCrypto, ILogger<CryptoController> logger)
        {
            _krakenCrypto = krakenCrypto;
            _logger = logger;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCryptocurrencies()
        {
            _logger.LogInformation("Updating cryptocurrencies...");

            try
            {
                var availableCryptos = await _krakenCrypto.GetAvailableCryptocurrenciesInGermanyAsync();
                _krakenCrypto.SaveCryptocurrenciesToDatabase(availableCryptos);
                _logger.LogInformation("Cryptocurrencies data updated and saved to SQLite database.");
                return Ok("Cryptocurrencies updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating cryptocurrencies data.");
                return StatusCode(500, "An error occurred while updating cryptocurrencies data.");
            }
        }

        [HttpGet("currencies")]
        public IActionResult GetCurrencies()
        {
            _logger.LogInformation("Fetching list of cryptocurrencies...");

            try
            {
                var currencies = _krakenCrypto.GetCryptocurrenciesFromDatabase();
                return Ok(currencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the list of cryptocurrencies.");
                return StatusCode(500, "An error occurred while fetching the list of cryptocurrencies.");
            }
        }
    }
}

