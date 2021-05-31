using CurrencyConverter.Model;
using System.Threading.Tasks;


namespace CurrencyConverter
{
    public interface IExchangeRatesApiService
    {
        string BaseUrl { get; set; }
        string Access_Key { get; set; }

        Task<CurrencyConvertResponse> GetLatestCurrencyRateAsync(CurrencyConvertRequest currencyConvertRequest);

        Task<CurrencyConvertResponse> GetHistoricalCurrencyRateAsync(CurrencyConvertRequest currencyConvertRequest);
    }
}
