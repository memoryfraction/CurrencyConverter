using System;
using System.Threading.Tasks;
using CurrencyConverter.Model;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace CurrencyConverter.Tests
{
    public class CurrencyConverterTest
    {
        readonly IExchangeRatesApiService _forexService;
        public CurrencyConverterTest()
        {
            var config = InitConfiguration();
            var Access_Key = config["Access_Key"];
            var BaseUrl = config["BaseUrl"];

            _forexService = new ExchangeRatesApiService();

            _forexService.BaseUrl = BaseUrl;
            _forexService.Access_Key = Access_Key;
        }


        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsetting.json")
                .AddUserSecrets(userSecretsId:"1216a805-8027-4acf-94fc-65b0daf883e1")
                .Build();
            return config;
        }

        [Test]
        public async Task GetLatestCurrencyRateAsync_Should_Work()
        {
            var request = new CurrencyConvertRequest()
            {
                DateTimeUtc = DateTime.UtcNow,
                CurrencyFrom = CurrencyType.USD,
                CurrencyTo = CurrencyType.CNY,
                Value = 100
            };
            var response = await _forexService.GetLatestCurrencyRateAsync(request);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Rate > 0);
        }

        [Test]
        public async Task GetHistoricalCurrencyRateAsync_Should_Work()
        {
            var request = new CurrencyConvertRequest()
            {
                DateTimeUtc = DateTime.UtcNow.AddDays(-2),
                CurrencyFrom = CurrencyType.USD,
                CurrencyTo = CurrencyType.CNY,
                Value = 100
            };
            var response = await _forexService.GetHistoricalCurrencyRateAsync(request);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Rate > 0);
        }
    }
}