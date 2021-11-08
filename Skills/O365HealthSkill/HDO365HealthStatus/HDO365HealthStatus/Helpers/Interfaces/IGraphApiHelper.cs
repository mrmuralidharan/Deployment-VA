using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using HDO365HealthStatus.Models;
using HDO365HealthStatus.Services;

namespace HDO365HealthStatus.Helpers.Interfaces
{
    public interface IGraphApiHelper
    {
        /// <summary>
        /// Method to perform HTTP GET requests in Microsoft Graph APIs.
        /// </summary>
        /// <typeparam name="T">Generic type class.</typeparam>
        /// <param name="url">Url to append on base Url for GET.(Example /api/messages).</param>
        /// <param name="token">Authentication token.</param>
        /// <param name="headers">Header parameters.</param>
        /// <returns>API response instance for GET request.</returns>
        Task<HttpResponseMessage> GetAsync(string url, string token, Dictionary<string, string> headers = null);

        /// <summary>
        /// Method to perform HTTP POST requests in Microsoft Graph APIs.
        /// </summary>
        /// <typeparam name="T">Generic type class.</typeparam>
        /// <param name="url">Url to append on base Url for GET.(Example /api/messages).</param>
        /// <param name="token">Authentication token.</param>
        /// <param name="payload">input JSON.</param>
        /// <param name="headers">Header parameters.</param>
        /// <returns>API response instance for GET request.</returns>
        Task<HttpResponseMessage> PostAsync(string url, string token, string payload, Dictionary<string, string> headers = null);

        /// <summary>
        /// Method to get Token for Microsoft Graph APIs.
        /// </summary>
        /// <typeparam name="T">Generic type class.</typeparam>
        /// <param name="configuration">configuration.</param>
        /// <returns>Token response using Microsoft App Id and App password.</returns>
        Task<TokenResponse> GetTokenAsync(IConfiguration configuration);

        //Task<TokenResponse> GetTokenAsync(BotSettings botSettings);

        /// <summary>
        /// Method to get graph authenticated client.
        /// </summary>
        /// <typeparam name="T">Generic type class.</typeparam>
        /// <param name="configuration">configuration.</param>
        /// <returns>Authentication graph client using token.</returns>
        GraphServiceClient GetAuthenticatedGraphClient(IConfiguration configuration);
    }
}
