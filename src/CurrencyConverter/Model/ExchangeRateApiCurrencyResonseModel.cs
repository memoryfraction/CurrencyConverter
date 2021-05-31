using System;
using System.Collections.Generic;

namespace CurrencyConverter.Model
{
    public class ExchangeRateApiCurrencyPair
    {
        public String CurrencyName { get; set; }
        public double Rate { get; set; }
    }
    public class ExchangeRateApiCurrencyResonseModel
    {
        public string DateString { get; set; }
        public List<ExchangeRateApiCurrencyPair> ExchangeRateApiCurrencyPairs { get; set; }
    }
}
