using System;

namespace CurrencyConverter.Model
{
    public class CurrencyConvertRequest
    {
        public CurrencyConvertRequest()
        {
            Value = 100;
        }
        public CurrencyType CurrencyFrom { get; set; }

        public CurrencyType CurrencyTo { get; set; }

        public DateTime DateTimeUtc { get; set; }
        public decimal Value { get; set; }
    }
}
