using System;

namespace CurrencyConverter.Model
{
    public class CurrencyConvertResponse
    {
        public decimal Value { get; set; }

        public CurrencyType CurrencyFrom { get; set; }

        public CurrencyType CurrencyTo { get; set; }


        /// <summary>
        /// 表示: 1单位From货币能兑换的To货币的数量
        /// </summary>
        public double Rate { get; set; }


        public DateTime DateTimeUtc { get; set; }
    }
}
