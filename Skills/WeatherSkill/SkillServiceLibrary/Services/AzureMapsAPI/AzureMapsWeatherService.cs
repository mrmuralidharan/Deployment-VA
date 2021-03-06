// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkillServiceLibrary.Models.AzureMaps;
using SkillServiceLibrary.Services;

namespace WeatherSkill.Services
{
    public sealed class AzureMapsWeatherService : IWeatherService
    {
        private static readonly string FindByAddressNoCoordinatesQueryUrl = $"https://atlas.microsoft.com/search/address/json?api-version=1.0&query={{0}}&subscription-key={{1}}&language={{2}}&limit=1";
        private static readonly string FindAddressByCoordinateUrl = $"https://atlas.microsoft.com/search/address/reverse/json?api-version=1.0&query={{0}}&subscription-key={{1}}&language={{2}}&limit=1";
        private static readonly string ForecastDailyUrl = $"https://atlas.microsoft.com/weather/forecast/daily/json?api-version=1.0&query={{0}}&duration={{1}}&subscription-key={{2}}&language={{3}}&limit=1";
        private static readonly string ForecastHourlyUrl = $"https://atlas.microsoft.com/weather/forecast/hourly/json?api-version=1.0&query={{0}}&duration={{1}}&subscription-key={{2}}&language={{3}}&limit=1";
        private static readonly string FindByAddressNoCoordinatesQueryUrlForZipCode = $"https://atlas.microsoft.com/search/address/json?api-version=1.0&query={{0}}&subscription-key={{1}}&language={{2}}";
        private static readonly string FindByAddressNoCoordinatesQueryUrlByCountryUS = $"https://atlas.microsoft.com/search/address/structured/json?subscription-key={{1}}&api-version=1.0&countryCode=USA&postalCode={{0}}";
        private static readonly string FindByAddressNoCoordinatesQueryUrlByCountryCAN = $"https://atlas.microsoft.com/search/address/structured/json?subscription-key={{1}}&api-version=1.0&countryCode=CAN&postalCode={{0}}";
        private static HttpClient _httpClient;
        private static string _apiKey;
        private static string _locale;

        public AzureMapsWeatherService(string apikey, string locale = "en-us", HttpClient client = null)
        {
            _apiKey = apikey;
            _locale = locale;
            if (client != null)
            {
                _httpClient = client;
            }
            else
            {
                _httpClient = new HttpClient();
            }
        }

        public async Task<(double, double)> GetCoordinatesByQueryAsync(string query)
        {
            var url = string.Format(CultureInfo.InvariantCulture, FindByAddressNoCoordinatesQueryUrl, query, _apiKey, _locale);
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonConvert.DeserializeObject<SearchResultSet>(response);
                return (apiResponse.Results[0].Position.Latitude, apiResponse.Results[0].Position.Longitude);
            }
            catch
            {
                return (double.NaN, double.NaN);
            }
        }

        public async Task<List<SearchResult>> GetCoordinatesByQueryAsyncForZipCode(string query)
        {
            //var url = string.Format(CultureInfo.InvariantCulture, FindByAddressNoCoordinatesQueryUrlForZipCode, query, _apiKey, _locale);
            var url = string.Format(CultureInfo.InvariantCulture, FindByAddressNoCoordinatesQueryUrlByCountryUS, query, _apiKey, _locale);
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonConvert.DeserializeObject<SearchResultSet>(response);
                //return (apiResponse.Results[0].Position.Latitude, apiResponse.Results[0].Position.Longitude);

                if (apiResponse.Results.Count <= 0)
                {
                    url = string.Format(CultureInfo.InvariantCulture, FindByAddressNoCoordinatesQueryUrlByCountryCAN, query, _apiKey, _locale);
                    response = await _httpClient.GetStringAsync(url);
                    apiResponse = JsonConvert.DeserializeObject<SearchResultSet>(response);

                    if (apiResponse.Results.Count <= 0)
                    {
                        url = string.Format(CultureInfo.InvariantCulture, FindByAddressNoCoordinatesQueryUrlForZipCode, query, _apiKey, _locale);
                        response = await _httpClient.GetStringAsync(url);
                        apiResponse = JsonConvert.DeserializeObject<SearchResultSet>(response);
                    }
                }

                return (apiResponse.Results);
            }
            catch
            {
                return (null);
            }
        }

        //public async Task<string> GetLocationByQueryAsync(string query)
        public async Task<SearchAddress> GetLocationByQueryAsync(string query)
        {
            var url = string.Format(CultureInfo.InvariantCulture, FindAddressByCoordinateUrl, query, _apiKey, _locale);

            try
            {
                var response = await _httpClient.GetStringAsync(url);

                var apiResponse = JsonConvert.DeserializeObject<SearchAddressReverseResponse>(response);

                if (!string.IsNullOrEmpty(apiResponse.Addresses[0].Address.Municipality))
                {
                    return apiResponse.Addresses[0].Address;
                }

                if (!string.IsNullOrEmpty(apiResponse.Addresses[0].Address.CountrySecondarySubdivision))
                {
                    return apiResponse.Addresses[0].Address;
                }

                if (!string.IsNullOrEmpty(apiResponse.Addresses[0].Address.MunicipalitySubdivision))
                {
                    return apiResponse.Addresses[0].Address;
                }

                if (!string.IsNullOrEmpty(apiResponse.Addresses[0].Address.CountrySubdivisionName))
                {
                    return apiResponse.Addresses[0].Address;
                }

                return apiResponse.Addresses[0].Address;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DailyForecastResponse> GetOneDayForecastAsync(string query)
        {
            var url = string.Format(CultureInfo.InvariantCulture, ForecastDailyUrl, query, "1", _apiKey, _locale);

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonConvert.DeserializeObject<DailyForecastResponse>(response);
                return apiResponse;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DailyForecastResponse> GetThreeDayForecastAsync(string query)
        {
            var url = string.Format(CultureInfo.InvariantCulture, ForecastDailyUrl, query, "3", _apiKey, _locale);

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonConvert.DeserializeObject<DailyForecastResponse>(response);
                return apiResponse;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DailyForecastResponse> GetFiveDayForecastAsync(string query)
        {
            var url = string.Format(CultureInfo.InvariantCulture, ForecastDailyUrl, query, "5", _apiKey, _locale);

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonConvert.DeserializeObject<DailyForecastResponse>(response);
                return apiResponse;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DailyForecastResponse> GetTenDayForecastAsync(string query)
        {
            var url = string.Format(CultureInfo.InvariantCulture, ForecastDailyUrl, query, "10", _apiKey, _locale);

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonConvert.DeserializeObject<DailyForecastResponse>(response);
                return apiResponse;
            }
            catch
            {
                return null;
            }
        }

        public async Task<HourlyForecastResponse> GetTwelveHourForecastAsync(string query)
        {
            var url = string.Format(CultureInfo.InvariantCulture, ForecastHourlyUrl, query, "12", _apiKey, _locale);

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonConvert.DeserializeObject<HourlyForecastResponse>(response);
                return apiResponse;
            }
            catch
            {
                return null;
            }
        }
    }
}
