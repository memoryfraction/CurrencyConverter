using CurrencyConverter.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    public class ExchangeRatesApiService : IExchangeRatesApiService
    {
        private string baseUrl;
        private string access_Key;
        public ExchangeRatesApiService()
        {
            baseUrl = "https://api.exchangeratesapi.io";
        }

        public string BaseUrl 
        {
            get 
            {
                return BaseUrl;
            }
            set 
            {
                baseUrl = value;
            }
        }

        public string Access_Key 
        {
            get 
            {
                return access_Key;
            }
            set
            {
                access_Key = value;
            }
        }

        public async Task<CurrencyConvertResponse> GetLatestCurrencyRateAsync(CurrencyConvertRequest currencyConvertRequest)
        {
            if (currencyConvertRequest == null)
                throw new ArgumentNullException();

            var response = new CurrencyConvertResponse()
            {
                CurrencyFrom = currencyConvertRequest.CurrencyFrom,
                CurrencyTo = currencyConvertRequest.CurrencyTo,
                Rate = 1.0,
                Value = currencyConvertRequest.Value,
                DateTimeUtc = currencyConvertRequest.DateTimeUtc == default(DateTime)? DateTime.UtcNow : currencyConvertRequest.DateTimeUtc
            };

            if (currencyConvertRequest.CurrencyFrom == currencyConvertRequest.CurrencyTo)
                return response;

            var url = baseUrl + "/latest?symbols=";
            var sb = new StringBuilder();
            sb.Append(url);
            sb.Append(currencyConvertRequest.CurrencyFrom.ToString().ToUpper());
            sb.Append(",");
            sb.Append(currencyConvertRequest.CurrencyTo.ToString().ToUpper());
            sb.Append("&access_key=" + access_Key);
            url = sb.ToString();

            using (var client = new HttpClient())
            {
                var remoteResponse = await client.GetAsync(url);

                // Parse 
                dynamic dynamicResponse = JsonConvert.DeserializeObject(remoteResponse.Content.ReadAsStringAsync().Result);
                response = ParseRemoteResponse(dynamicResponse, currencyConvertRequest);
            }
            return response;
        }

        private CurrencyConvertResponse ParseRemoteResponse(dynamic remoteResponse, CurrencyConvertRequest currencyConvertRequest)
        {
            var ratePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(remoteResponse.rates.ToString());
            var currencyPairs = new List<CurrencyPair>();
            foreach (var pair in ratePairs)
            {
                var currencyPair = ParseCurrencyPair(pair);
                currencyPairs.Add(currencyPair);
            }
            var response = new CurrencyConvertResponse();
            response.CurrencyFrom = currencyConvertRequest.CurrencyFrom;
            response.CurrencyTo = currencyConvertRequest.CurrencyTo;
            var cestDt = DateTime.Parse(remoteResponse.date.ToString());
            var cestTZI = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"); //欧洲央行位于德国;     
            response.DateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(cestDt, cestTZI);
            response.Rate = CalcuRate(currencyPairs, currencyConvertRequest.CurrencyFrom, currencyConvertRequest.CurrencyTo);
            response.Value = (decimal)response.Rate * currencyConvertRequest.Value;
            return response;
        }


        #region
        //   {
        //     "rates": {
        //         "CNY": 7.9287,
        //         "USD": 1.129
        //     },
        //     "base": "EUR",
        //     "date": "2020-07-07"
        //   }

        //        {
        //    "rates": {
        //        "2020-01-02": {
        //            "CNY": 7.7946,
        //            "USD": 1.1193
        //        }
        //    },
        //    "start_at": "2020-01-01",
        //    "base": "EUR",
        //    "end_at": "2020-01-02"
        //}
        #endregion


        public async Task<CurrencyConvertResponse> GetHistoricalCurrencyRateAsync(CurrencyConvertRequest currencyConvertRequest)
        {
            if (currencyConvertRequest == null)
                throw new ArgumentNullException();

            var resonse = new CurrencyConvertResponse()
            {
                CurrencyFrom = currencyConvertRequest.CurrencyFrom,
                CurrencyTo = currencyConvertRequest.CurrencyTo,
                Rate = 1.0,
                Value = currencyConvertRequest.Value,
                DateTimeUtc = DateTime.UtcNow
            };

            if (currencyConvertRequest.CurrencyFrom == currencyConvertRequest.CurrencyTo)
                return resonse;

            // Convert to  centeral eur datetime
            var centralEuropStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var cetDateTime = TimeZoneInfo.ConvertTime(currencyConvertRequest.DateTimeUtc, centralEuropStandardTime);

            var url = baseUrl + "/v1/";
            var sb = new StringBuilder();
            sb.Append(url);
            sb.Append(cetDateTime.ToString("yyyy-MM-dd") + "?");
            sb.Append("&symbols=");
            sb.Append(currencyConvertRequest.CurrencyFrom.ToString().ToUpper());
            sb.Append(",");
            sb.Append(currencyConvertRequest.CurrencyTo.ToString().ToUpper());
            sb.Append("&access_key=" + access_Key);
            url = sb.ToString();

            using (var client = new HttpClient())
            {
                var remoteResponse = await client.GetAsync(url);

                // Parse 
                var responseString =  await remoteResponse.Content.ReadAsStringAsync();
                resonse = ParseRemoteResponseWithDateRange(responseString, currencyConvertRequest);
            }
            return resonse;
        }




        /// <summary>
        /// ParseRemoteResponseWithDateRange
        /// </summary>
        /// <param name="remoteResponse"></param>
        /// <param name="currencyConvertRequest"></param>
        /// <returns></returns>
        private CurrencyConvertResponse ParseRemoteResponseWithDateRange(string remoteResponseString, CurrencyConvertRequest currencyConvertRequest)
        {
            var jObject = JObject.Parse(remoteResponseString);
            List<CurrencyPair> currencyPairs = new List<CurrencyPair>();
            var ratePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(jObject["rates"].ToString());

            foreach (var tmpPair in ratePairs)
            {
                var currencyPair = ParseCurrencyPair(tmpPair);
                currencyPairs.Add(currencyPair);
            }
            
            var response = new CurrencyConvertResponse();
            response.CurrencyFrom = currencyConvertRequest.CurrencyFrom;
            response.CurrencyTo = currencyConvertRequest.CurrencyTo;
            response.DateTimeUtc = DateTime.Parse(jObject["date"].ToString());
            response.Rate = CalcuRate(currencyPairs, currencyConvertRequest.CurrencyFrom, currencyConvertRequest.CurrencyTo);
            response.Value = (decimal)response.Rate * currencyConvertRequest.Value;
            return response;
        }


        private CurrencyPair ParseCurrencyPair(dynamic pair)
        {
            var currencyType = (CurrencyType)Enum.Parse(typeof(CurrencyType), pair.Key);
            var rate = Convert.ToDouble(pair.Value);
            return new CurrencyPair() { 
                CurrencyType = currencyType,
                Rate = rate
            };
        }

        private double CalcuRate(List<CurrencyPair> currencyPairs, CurrencyType from, CurrencyType to)
        {
            var ToRate = currencyPairs.Where(x => x.CurrencyType == to).Select(x => x.Rate).FirstOrDefault();
            var FromRate = currencyPairs.Where(x => x.CurrencyType == from).Select(x => x.Rate).FirstOrDefault();
            if (ToRate != 0 && FromRate != 0)
                return ToRate / FromRate;
            else throw new ArgumentOutOfRangeException("return rate is not valid.");
        }

        
    }
}
