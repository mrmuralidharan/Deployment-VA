using System.Collections.Generic;
using System.Threading.Tasks;
using SkillServiceLibrary.Models.AzureMaps;

namespace SkillServiceLibrary.Services
{
    public interface IWeatherService
    {
        Task<(double, double)> GetCoordinatesByQueryAsync(string query);

        Task<List<SearchResult>> GetCoordinatesByQueryAsyncForZipCode(string query);

        Task<SearchAddress> GetLocationByQueryAsync(string query);

        Task<DailyForecastResponse> GetOneDayForecastAsync(string query);

        Task<DailyForecastResponse> GetThreeDayForecastAsync(string query);

        Task<DailyForecastResponse> GetFiveDayForecastAsync(string query);

        Task<DailyForecastResponse> GetTenDayForecastAsync(string query);

        Task<HourlyForecastResponse> GetTwelveHourForecastAsync(string query);
    }
}
