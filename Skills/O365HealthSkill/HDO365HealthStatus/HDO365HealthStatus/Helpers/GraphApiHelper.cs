using HDO365HealthStatus.Helpers.Interfaces;
using HDO365HealthStatus.Models;
using HDO365HealthStatus.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HDO365HealthStatus.Helpers
{
    public class GraphApiHelper: IGraphApiHelper
    {
        /// <summary>
        /// A factory abstraction for a component that can create HttpClient instances with custom configuration for a given logical name.
        /// </summary>
        private readonly IHttpClientFactory clientFactory;

        /// <summary>
        /// Telemetry client for logging events and errors.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphApiHelper"/> class.
        /// </summary>
        /// <param name="clientFactory">A factory abstraction for a component that can create HttpClient instances with custom configuration for a given logical name.</param>
        /// <param name = "telemetryClient" > Telemetry client for logging events and errors.</param>
        public GraphApiHelper(IHttpClientFactory clientFactory, TelemetryClient telemetryClient)
        {
            this.clientFactory = clientFactory;
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Method to perform HTTP GET requests in Microsoft Graph APIs.
        /// </summary>
        /// <typeparam name="T">Generic type class.</typeparam>
        /// <param name="url">URL to append on base URL for GET.(Example /api/messages).</param>
        /// <param name="token">Authentication token.</param>
        /// <param name="headers">Header parameters.</param>
        /// <returns>API response instance for GET request.</returns>
        public async Task<HttpResponseMessage> GetAsync(string url, string token, Dictionary<string, string> headers = null)
        {
            using (var client = this.clientFactory.CreateClient("GraphApiHelper"))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return await client.SendAsync(request).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Method to perform HTTP POST requests in Microsoft Graph APIs.
        /// </summary>
        /// <typeparam name="T">Generic type class.</typeparam>
        /// <param name="url">Url to append on base Url for GET.(Example /api/messages).</param>
        /// <param name="token">Authentication token.</param>
        /// <param name="payload">input JSON.</param>
        /// <param name="headers">Header parameters.</param>
        /// <returns>API response instance for GET request.</returns>
        public async Task<HttpResponseMessage> PostAsync(string url, string token, string payload, Dictionary<string, string> headers = null)
        {
            using (var client = this.clientFactory.CreateClient("GraphApiHelper"))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                if (!string.IsNullOrEmpty(payload))
                {
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                }

                return await client.SendAsync(request).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Method to Get authenticated graph client.
        /// </summary>
        /// <param name="configuration">Configuration parameters.</param>
        /// <returns>Graph client.</returns>
        public GraphServiceClient GetAuthenticatedGraphClient(IConfiguration configuration)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider(
                 async (request) =>
                 {
                     TokenResponse tokenResponse = this.GetTokenAsync(configuration).Result;
                     request.Headers.Authorization =
                         new AuthenticationHeaderValue("Bearer", tokenResponse.Access_token);
                     await Task.CompletedTask;
                 }));
        }

        /// <summary>
        /// Method to Get token to access graph api.
        /// </summary>
        /// <param name="configuration">Configuration parameters.</param>
        /// <returns>token responsse.</returns>
        public async Task<TokenResponse> GetTokenAsync(IConfiguration configuration)
        {
            string uri = configuration.GetValue<string>("MicrosoftLoginUrl") + configuration.GetValue<string>("TenantId") + "/oauth2/v2.0/token";
            FormUrlEncodedContent data = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Content-Type", "application/x-www-form-urlencoded" },
                { "client_id", configuration.GetValue<string>("MicrosoftAppId") },
                { "scope", configuration.GetValue<string>("GraphDefaultScope") },
                { "client_secret", configuration.GetValue<string>("MicrosoftAppPassword") },
                { "grant_type", "client_credentials" },
            });
            string content = string.Empty;
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync(uri, data);
            response.EnsureSuccessStatusCode();
            content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<TokenResponse>(content));
        }

        public static async Task<TokenResponse> GetTokenAsync(BotSettings botSettings)
        {
            string uri = "https://login.microsoftonline.com/" + "73e2dc65-e18d-4421-8da7-e96d82b63aae" + "/oauth2/v2.0/token";
            FormUrlEncodedContent data = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Content-Type", "application/x-www-form-urlencoded" },
                { "client_id", "08cea0c3-9454-4a1c-9d40-5947e997022d" },
                { "scope", "https://graph.microsoft.com/.default" },
                { "client_secret", "d7Ft.b8Mot4-m_SGQSjsTAwX3.fAKZ4uV4" },
                { "grant_type", "client_credentials" },
            });
            string content = string.Empty;
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync(uri, data);
            response.EnsureSuccessStatusCode();
            content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<TokenResponse>(content));
        }
    }
}
