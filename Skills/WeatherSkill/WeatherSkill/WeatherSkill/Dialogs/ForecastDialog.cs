// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Connector;
using WeatherSkill.Models;
using WeatherSkill.Responses.Shared;
using WeatherSkill.Services;
using WeatherSkill.Utilities;
using WeatherSkill.Models.Action;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace WeatherSkill.Dialogs
{
    public class ForecastDialog : SkillDialogBase
    {
        private readonly IHttpContextAccessor _httpContext;
        private IConfiguration _configuration { get; set; }
        public ForecastDialog(
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContext)
            : base(nameof(ForecastDialog), serviceProvider)
        {
            _httpContext = httpContext;
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

            var fullForecastDialogWithPrompt = new WaterfallStep[]
            {
                RouteToGeographyPromptOrForecastResponseAsync,
                GeographyPromptAsync,
                GetWeatherResponseAsync,
                EndAsync,
            };

            var getForecastResponseDialog = new WaterfallStep[]
            {
                GetWeatherResponseAsync,
                EndAsync,
            };

            var getCurrentWeatherResponseDialog = new WaterfallStep[]
            {
                RouteToGeographyPromptOrForecastResponseAsync,
                GeographyPromptAsync,
                GetCurrentWeatherResponseAsync,
                EndAsync,
            };

            //AddDialog(new WaterfallDialog(nameof(ForecastDialog), fullForecastDialogWithPrompt));
            //AddDialog(new WaterfallDialog(DialogIds.GetForecastResponseDialog, getForecastResponseDialog));
            //AddDialog(new WaterfallDialog(DialogIds.GetCurrentWeatherResponseDialog, getCurrentWeatherResponseDialog));
            //AddDialog(new TextPrompt(DialogIds.GeographyPrompt, GeographyLuisValidatorAsync));

            AddDialog(new WaterfallDialog(nameof(ForecastDialog), getForecastResponseDialog));
            InitialDialogId = nameof(ForecastDialog);
        }

        /// <summary>
        /// Check if geography is stored in state and route to prompt or go to API call.
        /// </summary>
        private async Task<DialogTurnResult> RouteToGeographyPromptOrForecastResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await StateAccessor.GetAsync(stepContext.Context, cancellationToken: cancellationToken);
            var geography = state.Geography;

            if (string.IsNullOrEmpty(geography) && double.IsNaN(state.Latitude))
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(DialogIds.GetForecastResponseDialog, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Ask user for current location.
        /// </summary>
        private async Task<DialogTurnResult> GeographyPromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prompt = TemplateManager.GenerateActivity(SharedResponses.LocationPrompt);
            return await stepContext.PromptAsync(DialogIds.GeographyPrompt, new PromptOptions { Prompt = prompt }, cancellationToken);
        }

        /// <summary>
        /// Validate that a geography entity is pulled from the LUIS result.
        /// </summary>
        private async Task<bool> GeographyLuisValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (!promptContext.Recognized.Succeeded)
            {
                var prompt = TemplateManager.GenerateActivity(SharedResponses.LocationPrompt);
                await promptContext.Context.SendActivityAsync(prompt, cancellationToken: cancellationToken);
                return false;
            }

            var state = await StateAccessor.GetAsync(promptContext.Context, cancellationToken: cancellationToken);

            // check if geography is in state from processed LUIS result
            if (string.IsNullOrEmpty(state.Geography))
            {
                state.Geography = promptContext.Recognized.Value;
                return true;
            }

            return true;
        }

        /// <summary>
        /// Look up the five days forecast using Accuweather.
        /// </summary>
        private async Task<DialogTurnResult> GetWeatherResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await StateAccessor.GetAsync(stepContext.Context, cancellationToken: cancellationToken);

            var service = ServiceManager.InitService(Settings);
            var appSection = _configuration.GetSection("countryUsesFahrenheit");
            string[] countryUsesFahrenheit = appSection.Get<string[]>();

            if (!string.IsNullOrEmpty(state.Country))
            {
                if (!string.IsNullOrEmpty(state.Geography) && !string.IsNullOrEmpty(state.Country))
                {
                    (state.Latitude, state.Longitude) = await service.GetCoordinatesByQueryAsync(state.Geography + "," + state.Country);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(state.Geography))
                {
                    (state.Latitude, state.Longitude) = await service.GetCoordinatesByQueryAsync(state.Geography);
                }
            }

            if (double.IsNaN(state.Latitude) && double.IsNaN(state.Longitude))
            {
                if (!string.IsNullOrEmpty(state.zipCode) && state.zipCode != state.Geography)
                {
                    #region Commented on 27-09-2021
                    //if (Regex.IsMatch(state.zipCode, @"[-\s]"))
                    //{
                    //    string qryStr = Regex.Replace(state.zipCode, @"[-\s]", "");
                    //    state.IsZipWithSpace = true;
                    //    List<SkillServiceLibrary.Models.AzureMaps.SearchResult> res = await service.GetCoordinatesByQueryAsyncForZipCode(qryStr);
                    //    Console.WriteLine(res.Count);
                    //    if (res.Count > 0)
                    //    {
                    //        FilterAPIResponse.ExtractPostalCode(state, res);
                    //    }
                    //    else
                    //    {
                    //        res = await service.GetCoordinatesByQueryAsyncForZipCode(state.zipCode);
                    //        if (res.Count > 0)
                    //        {
                    //            FilterAPIResponse.ExtractPostalCode(state, res);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    List<SkillServiceLibrary.Models.AzureMaps.SearchResult> res = await service.GetCoordinatesByQueryAsyncForZipCode(state.zipCode);
                    //    Console.WriteLine(res.Count);
                    //    if (res.Count > 0)
                    //    {
                    //        FilterAPIResponse.ExtractPostalCode(state, res);
                    //    }

                    //}
                    #endregion

                    List<SkillServiceLibrary.Models.AzureMaps.SearchResult> res = await service.GetCoordinatesByQueryAsyncForZipCode(state.zipCode);

                    Console.WriteLine(res.Count);
                    if (res.Count > 0)
                    {
                        FilterAPIResponse.ExtractPostalCode(state, res);
                    }
                    if (double.IsNaN(state.Latitude) && double.IsNaN(state.Longitude))
                    {
                        string qryStr = Regex.IsMatch(state.zipCode, @"[\s]") ? Regex.Replace(state.zipCode, @"[-\s]", "") : state.zipCode;
                        state.IsZipWithSpace = true;
                        List<SkillServiceLibrary.Models.AzureMaps.SearchResult> secondSearch = await service.GetCoordinatesByQueryAsyncForZipCode(qryStr);
                        Console.WriteLine(secondSearch.Count);
                        if (secondSearch.Count > 0)
                        {
                            //state.Latitude = res[0].Position.Latitude;
                            //state.Longitude = res[0].Position.Longitude;
                            FilterAPIResponse.ExtractPostalCode(state, secondSearch);
                        }
                    }

                }
            }

            if (!double.IsNaN(state.Latitude) && !double.IsNaN(state.Longitude))
            {

                // The applicable query is specified as a comma separated string composed by latitude followed by longitude.
                // e.g. "47.641268,-122.125679".
                var queryString = string.Format("{0},{1}", state.Latitude.ToString(), state.Longitude.ToString());

            var location = new AddressResponse().Address;
            if (!double.IsNaN(state.Latitude) && !double.IsNaN(state.Longitude))
            {
                location = await service.GetLocationByQueryAsync(queryString);
            }
            else
            {
                throw new Exception("Must have Geography or Latitude & Longitude!");
            }

            var oneDayForecast = await service.GetOneDayForecastAsync(queryString);
            var threeDayForecast = await service.GetFiveDayForecastAsync(queryString);
            bool useFile = Channel.GetChannelId(stepContext.Context) == Channels.Msteams;

            var searchResult = Array.Find(countryUsesFahrenheit, element => element == location.CountryCodeISO3);

            var forecastModelFor3Days = new ThreeDaysForecastCard()
            {
                Speak = threeDayForecast.Forecasts[0].Day.ShortPhrase,
                Location = location.Municipality + "," + location.CountryCodeISO3,
                DayIcon = GetWeatherIcon(threeDayForecast.Forecasts[0].Day.IconCode, useFile),
                Date = $"{DateTime.Parse(threeDayForecast.Forecasts[0].Date).DayOfWeek} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Parse(threeDayForecast.Forecasts[0].Date).Month)} {DateTime.Parse(threeDayForecast.Forecasts[0].Date).Day}",
                MinimumTemperature = Convert.ToInt32(oneDayForecast.Forecasts[0].Temperature.Minimum.Value),
                MaximumTemperature = Convert.ToInt32(oneDayForecast.Forecasts[0].Temperature.Maximum.Value),
                TempUnit = searchResult == location.CountryCodeISO3 ? "°F" : "°C",
                ShortPhrase = oneDayForecast.Forecasts[0].Day.ShortPhrase,
                WindDescription = $"Winds {threeDayForecast.Forecasts[0].Day.Wind.Speed.Value} {threeDayForecast.Forecasts[0].Day.Wind.Speed.Unit} {threeDayForecast.Forecasts[0].Day.Wind.Direction.LocalizedDescription}",
                IconDay1 = GetWeatherIcon(threeDayForecast.Forecasts[1].Day.IconCode, useFile),
                Day1 = $"{DateTime.Parse(threeDayForecast.Forecasts[1].Date).DayOfWeek}",
                MinimumTemperatureDay1 = Convert.ToInt32(threeDayForecast.Forecasts[1].Temperature.Minimum.Value),
                MaximumTemperatureDay1 = Convert.ToInt32(threeDayForecast.Forecasts[1].Temperature.Maximum.Value),
                TempUnit1 = searchResult == location.CountryCodeISO3 ? "°F" : "°C",
                Day2 = $"{DateTime.Parse(threeDayForecast.Forecasts[2].Date).DayOfWeek}",
                IconDay2 = GetWeatherIcon(threeDayForecast.Forecasts[2].Day.IconCode, useFile),
                MinimumTemperatureDay2 = Convert.ToInt32(threeDayForecast.Forecasts[2].Temperature.Minimum.Value),
                MaximumTemperatureDay2 = Convert.ToInt32(threeDayForecast.Forecasts[2].Temperature.Maximum.Value),
                TempUnit2 = searchResult == location.CountryCodeISO3 ? "°F" : "°C",
                Day3 = $"{DateTime.Parse(threeDayForecast.Forecasts[3].Date).DayOfWeek}",
                IconDay3 = GetWeatherIcon(threeDayForecast.Forecasts[3].Day.IconCode, useFile),
                MinimumTemperatureDay3 = Convert.ToInt32(threeDayForecast.Forecasts[3].Temperature.Minimum.Value),
                MaximumTemperatureDay3 = Convert.ToInt32(threeDayForecast.Forecasts[3].Temperature.Maximum.Value),
                TempUnit3 = searchResult == location.CountryCodeISO3 ? "°F" : "°C"
            };


            if (forecastModelFor3Days.TempUnit == "°F")
            {
                forecastModelFor3Days.MinimumTemperature = (int)((oneDayForecast.Forecasts[0].Temperature.Minimum.Value * 1.8) + 32);
                forecastModelFor3Days.MaximumTemperature = (int)((oneDayForecast.Forecasts[0].Temperature.Maximum.Value * 1.8) + 32);
                forecastModelFor3Days.MinimumTemperatureDay1 = (int)((threeDayForecast.Forecasts[1].Temperature.Minimum.Value * 1.8) + 32);
                forecastModelFor3Days.MaximumTemperatureDay1 = (int)((threeDayForecast.Forecasts[1].Temperature.Maximum.Value * 1.8) + 32);
                forecastModelFor3Days.MinimumTemperatureDay2 = (int)((threeDayForecast.Forecasts[2].Temperature.Minimum.Value * 1.8) + 32);
                forecastModelFor3Days.MaximumTemperatureDay2 = (int)((threeDayForecast.Forecasts[2].Temperature.Maximum.Value * 1.8) + 32);
                forecastModelFor3Days.MinimumTemperatureDay3 = (int)((threeDayForecast.Forecasts[3].Temperature.Minimum.Value * 1.8) + 32);
                forecastModelFor3Days.MaximumTemperatureDay3 = (int)((threeDayForecast.Forecasts[3].Temperature.Maximum.Value * 1.8) + 32);
            }

            if (state.IsAction)
            {
                var summary = forecastModelFor3Days.Speak;
                var actionResult = new ActionResult()
                {
                    Summary = summary,
                    ActionSuccess = true
                };
                return await stepContext.EndDialogAsync(actionResult, cancellationToken: cancellationToken);
            }

            //#region 3 days forecast
            var templateIdForThreeDayForecast = SharedResponses.ThreeDaysForecast;
            var cardForThreeDaysForecast = new Card(GetDivergedCardName(stepContext.Context, "ThreeDaysForecast"), forecastModelFor3Days);
            var responseForThreeDaysForecast = TemplateManager.GenerateActivity(templateIdForThreeDayForecast, cardForThreeDaysForecast, tokens: null);
            //#endregion

            await stepContext.Context.SendActivityAsync(responseForThreeDaysForecast, cancellationToken);

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var response = TemplateManager.GenerateActivityForLocale(SharedResponses.DidntUnderstandMessage);
                await stepContext.Context.SendActivityAsync(response, cancellationToken);
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

        }

        /// <summary>
        /// Look up the five days forecast using Accuweather.
        /// </summary>
        private async Task<DialogTurnResult> GetCurrentWeatherResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await StateAccessor.GetAsync(stepContext.Context, cancellationToken: cancellationToken);

            var service = ServiceManager.InitService(Settings);

            if (!string.IsNullOrEmpty(state.Country))
            {
                if (!string.IsNullOrEmpty(state.Geography) && !string.IsNullOrEmpty(state.Country))
                {
                    (state.Latitude, state.Longitude) = await service.GetCoordinatesByQueryAsync(state.Geography + "," + state.Country);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(state.Geography))
                {
                    (state.Latitude, state.Longitude) = await service.GetCoordinatesByQueryAsync(state.Geography);
                }
            }

            // The applicable query is specified as a comma separated string composed by latitude followed by longitude.
            // e.g. "47.641268,-122.125679".
            var queryString = string.Format("{0},{1}", state.Latitude.ToString(), state.Longitude.ToString());

            var location = new AddressResponse().Address;
            if (!double.IsNaN(state.Latitude) && !double.IsNaN(state.Longitude))
            {
                location = await service.GetLocationByQueryAsync(queryString);
            }
            else
            {
                throw new Exception("Must have Geography or Latitude & Longitude!");
            }

            var oneDayForecast = await service.GetOneDayForecastAsync(queryString);
            bool useFile = Channel.GetChannelId(stepContext.Context) == Channels.Msteams;

            var forecastModel = new TodayWeatherCard()
            {
                Speak = oneDayForecast.Forecasts[0].Day.ShortPhrase,
                Location = location.Municipality + "," + location.CountryCodeISO3,
                DayIcon = GetWeatherIcon(oneDayForecast.Forecasts[0].Day.IconCode, useFile),
                Date = $"{DateTime.Parse(oneDayForecast.Forecasts[0].Date).DayOfWeek} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Parse(oneDayForecast.Forecasts[0].Date).Month)} {DateTime.Parse(oneDayForecast.Forecasts[0].Date).Day}",
                MinimumTemperature = Convert.ToInt32(oneDayForecast.Forecasts[0].Temperature.Minimum.Value),
                MaximumTemperature = Convert.ToInt32(oneDayForecast.Forecasts[0].Temperature.Maximum.Value),
                ShortPhrase = oneDayForecast.Forecasts[0].Day.ShortPhrase,
                WindDescription = $"Winds {oneDayForecast.Forecasts[0].Day.Wind.Speed.Value} {oneDayForecast.Forecasts[0].Day.Wind.Speed.Unit} {oneDayForecast.Forecasts[0].Day.Wind.Direction.LocalizedDescription}"
            };

            if (state.IsAction)
            {
                var summary = forecastModel.Speak;
                var actionResult = new ActionResult()
                {
                    Summary = summary,
                    ActionSuccess = true
                };
                return await stepContext.EndDialogAsync(actionResult, cancellationToken: cancellationToken);
            }

            var templateId = SharedResponses.TodayWeather;
            var card = new Card(GetDivergedCardName(stepContext.Context, "TodayWeather"), forecastModel);
            var response = TemplateManager.GenerateActivity(templateId, card, tokens: null);

            await stepContext.Context.SendActivityAsync(response, cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// AccuWeather returns an icon id, correlate those to custom assets.
        /// https://apidev.accuweather.com/developers/weatherIcons.
        /// </summary>
        /// <returns>Returns an svg string for the icon.</returns>
        private string GetWeatherIcon(int iconValue, bool useFile)
        {
            switch (iconValue)
            {
                // sunny
                case 1:
                // mostly sunny
                case 2:
                // partly sunny
                case 3:
                    // return sun icon
                    return useFile ? GetImageUri("sun.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTE4LjU3LDIxLjVIMTUuMlYyMC4zOGgzLjM3Wm0xLjA4LTMuNzQtMi4zOC0yLjM5Ljc5LS44TDIwLjQ1LDE3Wm0wLDYuMzYuOC44TDE4LjA2LDI3LjNsLS43OS0uNzlabTQtNy4xMmEzLjc4LDMuNzgsMCwwLDEsMS41NC4zMSwzLjkxLDMuOTEsMCwwLDEsMi4wOSwyLjA5LDQsNCwwLDAsMSwwLDMuMDcsMy43MiwzLjcyLDAsMCwxLS44NCwxLjI1LDQuMTMsNC4xMywwLDAsMS0xLjI1Ljg1LDQsNCwwLDAsMS0zLjA3LDAsNCw0LDAsMCwxLTEuMjUtLjg1LDMuODMsMy44MywwLDAsMS0xLjE1LTIuNzhBNCw0LDAsMCwxLDIwLDE5LjRhNC4xMyw0LjEzLDAsMCwxLC44NS0xLjI1LDMuODIsMy44MiwwLDAsMSwxLjI1LS44NEEzLjc3LDMuNzcsMCwwLDEsMjMuNjMsMTdabTAsNi43NWEyLjY2LDIuNjYsMCwwLDAsMS4wOS0uMjIsMy4wOCwzLjA4LDAsMCwwLC45LS42LDMsMywwLDAsMCwuNi0uOSwyLjcsMi43LDAsMCwwLC4yMy0xLjA5LDIuNjYsMi42NiwwLDAsMC0uMjMtMS4wOSwyLjg1LDIuODUsMCwwLDAtMS41LTEuNSwyLjY2LDIuNjYsMCwwLDAtMS4wOS0uMjMsMi43LDIuNywwLDAsMC0xLjA5LjIzLDIuODUsMi44NSwwLDAsMC0xLjUsMS41LDIuNjYsMi42NiwwLDAsMC0uMjIsMS4wOUEyLjY5LDIuNjksMCwwLDAsMjEsMjJhMywzLDAsMCwwLC42LjksMy4wOCwzLjA4LDAsMCwwLC45LjZBMi42OSwyLjY5LDAsMCwwLDIzLjYzLDIzLjc1Wm0uNTctNy44N0gyMy4wN1YxMi41SDI0LjJaTTIzLjA3LDI2SDI0LjJ2My4zOEgyMy4wN1ptNC41NS04LjI0LS44LS44LDIuMzktMi4zOS43OS44Wm0wLDYuMzZMMzAsMjYuNTFsLS43OS43OS0yLjM5LTIuMzhabTQuNDUtMy43NFYyMS41SDI4LjdWMjAuMzhaIi8+PC9zdmc+";

                // intermittent clouds
                case 4:
                // hazy sunshine
                case 5:
                    // cloud icon
                    return useFile ? GetImageUri("cloud.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTI5Ljc5LDIyLjM4YTMuMzIsMy4zMiwwLDAsMSwxLjMzLjI2LDMuMzMsMy4zMywwLDAsMSwxLjgxLDEuNzgsMy4yOSwzLjI5LDAsMCwxLC4yNywxLjMzLDMuMTcsMy4xNywwLDAsMS0uMjcsMS4zMSwzLjMsMy4zLDAsMCwxLS43MiwxLjA4LDMuNDYsMy40NiwwLDAsMS0xLjA4LjcyLDMuMjMsMy4yMywwLDAsMS0xLjMxLjI2SDE5LjdhNC4zNiw0LjM2LDAsMCwxLTEuNzYtLjM1LDQuNSw0LjUsMCwwLDEtMS40My0xLDQuMzksNC4zOSwwLDAsMS0xLTEuNDMsNC41OCw0LjU4LDAsMCwxLDAtMy41MSw0LjM5LDQuMzksMCwwLDEsMS0xLjQzLDQuNSw0LjUsMCwwLDEsMS40My0xLDQuMzYsNC4zNiwwLDAsMSwxLjc2LS4zNiw0LjQ1LDQuNDUsMCwwLDEsLjgxLjA4LDUuNTQsNS41NCwwLDAsMSwuODItMSw0Ljc1LDQuNzUsMCwwLDEsMS0uNzNBNS4yOSw1LjI5LDAsMCwxLDIzLjQ5LDE4YTUuMzgsNS4zOCwwLDAsMSwxLjI3LS4xNSw1LDUsMCwwLDEsMy4zNywxLjI4LDUsNSwwLDAsMSwxLjExLDEuNDNBNS4xOSw1LjE5LDAsMCwxLDI5Ljc5LDIyLjM4Wm0wLDUuNjJhMi4yLDIuMiwwLDAsMCwuODgtLjE4LDIsMiwwLDAsMCwuNzEtLjQ4LDIuMTQsMi4xNCwwLDAsMCwuNDktLjcyLDIuMzEsMi4zMSwwLDAsMCwwLTEuNzQsMi4xNCwyLjE0LDAsMCwwLS40OS0uNzIsMiwyLDAsMCwwLS43MS0uNDgsMi4yLDIuMiwwLDAsMC0uODgtLjE4SDI4Ljd2LS41NmE0LDQsMCwwLDAtLjMxLTEuNTQsNC4xMyw0LjEzLDAsMCwwLS44NS0xLjI1LDMuNzIsMy43MiwwLDAsMC0xLjI1LS44NEEzLjc3LDMuNzcsMCwwLDAsMjQuNzYsMTlhMy45NCwzLjk0LDAsMCwwLTEuMTkuMTgsNC4yOSw0LjI5LDAsMCwwLTEuMDUuNTIsNC4wNyw0LjA3LDAsMCwwLS44NS44MSw0LjEzLDQuMTMsMCwwLDAtLjU5LDEsMy4xMywzLjEzLDAsMCwwLTEuMzgtLjMxLDMuNDEsMy40MSwwLDAsMC0xLjMyLjI2LDMuNTcsMy41NywwLDAsMC0xLjA3LjczLDMuNDksMy40OSwwLDAsMC0uNzMsMS4wOCwzLjM1LDMuMzUsMCwwLDAtLjI2LDEuMywzLjI5LDMuMjksMCwwLDAsMSwyLjM5LDMuNTcsMy41NywwLDAsMCwxLjA3LjczQTMuNDEsMy40MSwwLDAsMCwxOS43LDI4WiIvPjwvc3ZnPg==";

                // mostly cloudy
                case 6:
                // cloudy
                case 7:
                // dreary (overcast)
                case 8:
                    // cloud2x icon
                    return useFile ? GetImageUri("cloud2x.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTMwLjY3LDIzLjYxYTMuMzMsMy4zMywwLDAsMSwxLC40NywzLjI0LDMuMjQsMCwwLDEsLjc5LjczLDMuMTksMy4xOSwwLDAsMSwuNTIsMSwzLjQsMy40LDAsMCwxLC4xOSwxLjExLDMuNCwzLjQsMCwwLDEtMSwyLjM4LDMuNDksMy40OSwwLDAsMS0xLjA4LjczLDMuNCwzLjQsMCwwLDEtMS4zMS4yNkgyMC4yNmEzLjcsMy43LDAsMCwxLTEuNTMtLjMxLDMuODcsMy44NywwLDAsMS0xLjI2LS44NCw0LjEsNC4xLDAsMCwxLS44NC0xLjI1LDMuOTQsMy45NCwwLDAsMS0uMS0yLjgsMy41LDMuNSwwLDAsMS0xLTEuMTgsMy4yMywzLjIzLDAsMCwxLS4zNS0xLjQ5LDMuMTksMy4xOSwwLDAsMSwuMjYtMS4zMUEzLjQ5LDMuNDksMCwwLDEsMTYuMTksMjBhMy4yOSwzLjI5LDAsMCwxLDIuMzgtMWguMTlMMTksMTlhMy43NiwzLjc2LDAsMCwxLC42MS0uOTUsNC4wNiw0LjA2LDAsMCwxLC44NC0uNzEsMy40OCwzLjQ4LDAsMCwxLDEtLjQ1LDMuNzcsMy43NywwLDAsMSwxLjExLS4xNiwzLjg3LDMuODcsMCwwLDEsMS40LjI2LDQuMDksNC4wOSwwLDAsMSwxLjE5LjcxQTMuODMsMy44MywwLDAsMSwyNiwxOC44YTMuNywzLjcsMCwwLDEsLjQ0LDEuMzUsNC44OCw0Ljg4LDAsMCwxLDEuNDEuMzYsNS4wOSw1LjA5LDAsMCwxLDEuMjMuNzQsNS40Myw1LjQzLDAsMCwxLDEsMS4wNUE0LjgzLDQuODMsMCwwLDEsMzAuNjcsMjMuNjFabS0xMi4xLTMuNDlhMi4xMiwyLjEyLDAsMCwwLS44Ny4xOCwyLjM5LDIuMzksMCwwLDAtLjcyLjQ4LDIuMzMsMi4zMywwLDAsMC0uNDguNzIsMi4xMywyLjEzLDAsMCwwLS4xOC44OCwyLjE3LDIuMTcsMCwwLDAsLjE5LjksMi40NywyLjQ3LDAsMCwwLC41NC43NSwzLjg4LDMuODgsMCwwLDEsMy4yMS0xLjY1LDMuNjcsMy42NywwLDAsMSwuNjYuMDUsMy40MiwzLjQyLDAsMCwxLC42My4xNiw0LjY0LDQuNjQsMCwwLDEsLjY5LS45MSw1LjU5LDUuNTksMCwwLDEsLjg4LS43Myw1Ljg1LDUuODUsMCwwLDEsMS0uNTIsNS41NCw1LjU0LDAsMCwxLDEuMTItLjI3LDIuODcsMi44NywwLDAsMC0uMzUtLjkxLDIuNTksMi41OSwwLDAsMC0uNjItLjczLDIuNzEsMi43MSwwLDAsMC0uODMtLjQ3LDIuNjMsMi42MywwLDAsMC0xLS4xNywyLjczLDIuNzMsMCwwLDAtMSwuMTksMi44LDIuOCwwLDAsMC0uODcuNTUsMywzLDAsMCwwLS42MS44MiwyLjg2LDIuODYsMCwwLDAtLjI4LDEsMi43LDIuNywwLDAsMC0uNTUtLjI0QTIsMiwwLDAsMCwxOC41NywyMC4xMlptMTEuMjUsOUEyLjE5LDIuMTksMCwwLDAsMzAuNywyOWEyLjI5LDIuMjksMCwwLDAsMS4yLTEuMiwyLjI5LDIuMjksMCwwLDAsLjE3LS44NywyLjE5LDIuMTksMCwwLDAtLjE4LS45LDIsMiwwLDAsMC0uNS0uNzIsMi4yMywyLjIzLDAsMCwwLS43My0uNDcsMi40NCwyLjQ0LDAsMCwwLS45LS4xNyw0LDQsMCwwLDAtMS4zMS0yLjQxLDMuODEsMy44MSwwLDAsMC0yLjU3LTEsMy42MywzLjYzLDAsMCwwLTEuMjguMjIsNC4xOSw0LjE5LDAsMCwwLTEuMTIuNjEsMy42NSwzLjY1LDAsMCwwLS44Ni45MywzLjkyLDMuOTIsMCwwLDAtLjUzLDEuMTgsMywzLDAsMCwwLS44Ni0uNTEsMi42NSwyLjY1LDAsMCwwLTEtLjE4LDIuNzQsMi43NCwwLDAsMC0xLjEuMjIsMi45MSwyLjkxLDAsMCwwLS44OS42LDIuNzMsMi43MywwLDAsMC0uNi45LDIuNjksMi42OSwwLDAsMC0uMjIsMS4wOSwyLjc0LDIuNzQsMCwwLDAsLjIyLDEuMSwyLjU5LDIuNTksMCwwLDAsLjYuODksMi43NywyLjc3LDAsMCwwLC44OS42MSwyLjkxLDIuOTEsMCwwLDAsMS4xLjIxWiIvPjwvc3ZnPg==";

                // fog
                case 11:
                    // cloud fog icon
                    return useFile ? GetImageUri("cloudfog.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTMwLjY3LDIxLjYxYTMuMzYsMy4zNiwwLDAsMSwxLC40NiwzLjUyLDMuNTIsMCwwLDEsLjguNzQsMy40NywzLjQ3LDAsMCwxLC43MSwyLjA3LDIuNzYsMi43NiwwLDAsMS0uMDYuNTdBNS42Miw1LjYyLDAsMCwxLDMzLDI2SDMxLjc2YTIuMTQsMi4xNCwwLDAsMCwuMjMtLjU0LDIuMzIsMi4zMiwwLDAsMCwuMDgtLjU4LDIuMjMsMi4yMywwLDAsMC0uMTgtLjksMi4wOCwyLjA4LDAsMCwwLS41LS43MSwyLjQsMi40LDAsMCwwLTEuNjItLjY1LDMuODEsMy44MSwwLDAsMC0uNDUtMS4zMyw0LDQsMCwwLDAtLjg2LTEuMDgsNCw0LDAsMCwwLTEuMTgtLjcxLDMuODIsMy44MiwwLDAsMC0xLjQtLjI1LDMuNjMsMy42MywwLDAsMC0xLjI4LjIyLDQuMTksNC4xOSwwLDAsMC0xLjEyLjYxLDMuNjUsMy42NSwwLDAsMC0uODYuOTMsMy45MiwzLjkyLDAsMCwwLS41MywxLjE4LDMsMywwLDAsMC0uODYtLjUxLDIuNjUsMi42NSwwLDAsMC0xLS4xOCwyLjc0LDIuNzQsMCwwLDAtMS4xLjIyLDIuOTEsMi45MSwwLDAsMC0uODkuNiwyLjczLDIuNzMsMCwwLDAtLjYuOSwyLjc3LDIuNzcsMCwwLDAtLjA3LDJBMi44MiwyLjgyLDAsMCwwLDE4LDI2SDE2LjcyYTQsNCwwLDAsMS0uMy0uODIsMy42MywzLjYzLDAsMCwxLS4xLS44Nyw0LDQsMCwwLDEsLjMxLTEuNTMsNCw0LDAsMCwxLDIuMS0yLjA5LDMuNywzLjcsMCwwLDEsMS41My0uMzEsNC4xNSw0LjE1LDAsMCwxLDEuMjkuMjEsNS4xNyw1LjE3LDAsMCwxLDEuODQtMS44LDQuNTksNC41OSwwLDAsMSwxLjE5LS40OSw0Ljc0LDQuNzQsMCwwLDEsMS4zLS4xOCw0LjgzLDQuODMsMCwwLDEsMS41Ny4yNiw1LjEsNS4xLDAsMCwxLDEuMzkuNzIsNC43OCw0Ljc4LDAsMCwxLDEuMSwxLjFBNC43Miw0LjcyLDAsMCwxLDMwLjY3LDIxLjYxWm0uODQsNS41MWEuNTQuNTQsMCwwLDEsLjM5LjE3LjUyLjUyLDAsMCwxLC4xNy40LjUuNSwwLDAsMS0uMTcuMzkuNTQuNTQsMCwwLDEtLjM5LjE3SDE4YS41Ni41NiwwLDAsMS0uNC0uMTcuNTMuNTMsMCwwLDEtLjE2LS4zOS41NS41NSwwLDAsMSwuMTYtLjQuNTYuNTYsMCwwLDEsLjQtLjE3Wm0tMy4zOCwyLjI2YS41NC41NCwwLDAsMSwuNC4xNi41Ni41NiwwLDAsMSwuMTcuNC41NC41NCwwLDAsMS0uMTcuMzkuNTQuNTQsMCwwLDEtLjQuMTdIMjAuMjZhLjU2LjU2LDAsMCwxLS40LS4xNy41My41MywwLDAsMS0uMTYtLjM5LjU1LjU1LDAsMCwxLC41Ni0uNTZaTTIxLjM4LDI2YS41OC41OCwwLDAsMS0uNTYtLjU2QS41Ni41NiwwLDAsMSwyMSwyNWEuNTMuNTMsMCwwLDEsLjM5LS4xNmg3Ljg4YS41My41MywwLDAsMSwuMzkuMTYuNTIuNTIsMCwwLDEsLjE3LjQuNS41LDAsMCwxLS4xNy4zOS41NC41NCwwLDAsMS0uMzkuMTdaIi8+PC9zdmc+";

                // showers
                case 12:
                // rain
                case 18:
                    // rain icon
                    return useFile ? GetImageUri("rain.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTMwLjY3LDIzLjYxYTMuMzYsMy4zNiwwLDAsMSwxLC40NiwzLjMxLDMuMzEsMCwwLDEsLjguNzQsMy40MSwzLjQxLDAsMCwxLC43MSwyLjA3QTMuMjUsMy4yNSwwLDAsMSwzMywyOGEzLjM0LDMuMzQsMCwwLDEtMi4zOSwyLjE1bC0uNTMtMS4wNWEyLjUxLDIuNTEsMCwwLDAsLjc5LS4yNCwyLjMyLDIuMzIsMCwwLDAsLjYzLS40OSwyLjM1LDIuMzUsMCwwLDAsLjQyLS42OCwyLjE1LDIuMTUsMCwwLDAsLjE1LS44LDIuMTksMi4xOSwwLDAsMC0uMTgtLjksMiwyLDAsMCwwLS41LS43MSwyLjIzLDIuMjMsMCwwLDAtLjczLS40NywyLjI2LDIuMjYsMCwwLDAtLjktLjE4LDQsNCwwLDAsMC0xLjMxLTIuNDEsMy44MSwzLjgxLDAsMCwwLTIuNTctMSwzLjYzLDMuNjMsMCwwLDAtMS4yOC4yMiw0LjE5LDQuMTksMCwwLDAtMS4xMi42MSwzLjY1LDMuNjUsMCwwLDAtLjg2LjkzLDMuOTIsMy45MiwwLDAsMC0uNTMsMS4xOCwzLDMsMCwwLDAtLjg2LS41MSwyLjY1LDIuNjUsMCwwLDAtMS0uMTgsMi43NCwyLjc0LDAsMCwwLTEuMS4yMiwyLjkxLDIuOTEsMCwwLDAtLjg5LjYsMi43MywyLjczLDAsMCwwLS42LjksMi42OSwyLjY5LDAsMCwwLS4yMiwxLjA5LDIuNzQsMi43NCwwLDAsMCwuMTUuOUEyLjgzLDIuODMsMCwwLDAsMTgsMjhhMy4wOSwzLjA5LDAsMCwwLC42NC42MSwzLDMsMCwwLDAsLjgzLjM5TDE5LDMwYTMuNDksMy40OSwwLDAsMS0xLjA4LS41NywzLjc2LDMuNzYsMCwwLDEtLjg0LS44NSw0LjExLDQuMTEsMCwwLDEtLjU1LTEuMDYsNCw0LDAsMCwxLS4xOS0xLjIxLDQsNCwwLDAsMSwuMzEtMS41Myw0LDQsMCwwLDEsMi4xLTIuMDksMy43LDMuNywwLDAsMSwxLjUzLS4zMSw0LjE1LDQuMTUsMCwwLDEsMS4yOS4yMSw1LjE3LDUuMTcsMCwwLDEsMS44NC0xLjgsNC41OSw0LjU5LDAsMCwxLDEuMTktLjQ5LDQuNzQsNC43NCwwLDAsMSwxLjMtLjE4LDQuODMsNC44MywwLDAsMSwxLjU3LjI2LDUuMSw1LjEsMCwwLDEsMS4zOS43Miw1LDUsMCwwLDEsMS44MywyLjUxWm0tOCw3YTEuMjQsMS4yNCwwLDAsMSwuMTIuNTMsMS40MiwxLjQyLDAsMCwxLS4xMS41NSwxLjM4LDEuMzgsMCwwLDEtLjc1Ljc1LDEuNDEsMS40MSwwLDAsMS0xLjA5LDAsMS4zOCwxLjM4LDAsMCwxLS43NS0uNzUsMS4yNiwxLjI2LDAsMCwxLS4xMS0uNTUsMS4yNCwxLjI0LDAsMCwxLC4xMi0uNTNMMjEuMzgsMjhaTTI2LDI4LjMxYTEuMjUsMS4yNSwwLDAsMSwuMTMuNTMsMS40MiwxLjQyLDAsMCwxLS4xMS41NSwxLjU3LDEuNTcsMCwwLDEtLjMxLjQ1LDEuMzcsMS4zNywwLDAsMS0uNDUuMywxLjQxLDEuNDEsMCwwLDEtLjU0LjExLDEuNDYsMS40NiwwLDAsMS0uNTUtLjExLDEuMjMsMS4yMywwLDAsMS0uNDQtLjMsMS41NywxLjU3LDAsMCwxLS4zMS0uNDUsMS40MiwxLjQyLDAsMCwxLS4xMS0uNTUsMS4xMiwxLjEyLDAsMCwxLC4xMy0uNTNsMS4yOC0yLjU2Wm0zLjM4LDIuMjVhMS4yNCwxLjI0LDAsMCwxLC4xMi41MywxLjQyLDEuNDIsMCwwLDEtLjExLjU1LDEuMzgsMS4zOCwwLDAsMS0uNzUuNzUsMS40MSwxLjQxLDAsMCwxLTEuMDksMCwxLjM4LDEuMzgsMCwwLDEtLjc1LS43NSwxLjI2LDEuMjYsMCwwLDEtLjExLS41NSwxLjI0LDEuMjQsMCwwLDEsLjEyLS41M0wyOC4xMywyOFoiLz48L3N2Zz4=";

                // mostly cloudy w showers
                case 13:
                // partly sunny w showers
                case 14:
                    // cloudsunrain icon
                    return useFile ? GetImageUri("cloudsunrain.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTE3LjQ1LDIyLjM4SDE1LjJWMjEuMjVoMi4yNVptMTMuMjIsMS4yM2EzLjM2LDMuMzYsMCwwLDEsMSwuNDYsMy4zMSwzLjMxLDAsMCwxLC44Ljc0LDMuNDEsMy40MSwwLDAsMSwuNzEsMi4wNywzLjQsMy40LDAsMCwxLTEsMi4zOCwzLjQ5LDMuNDksMCwwLDEtMS4wOC43MywzLjQsMy40LDAsMCwxLTEuMzEuMjZoLS45NWEyLjYxLDIuNjEsMCwwLDAsLjI0LS41NCwyLjk0LDIuOTQsMCwwLDAsLjEyLS41OWguNTlBMi4xOSwyLjE5LDAsMCwwLDMwLjcsMjlhMi4yOSwyLjI5LDAsMCwwLDEuMi0xLjIsMi4yOSwyLjI5LDAsMCwwLC4xNy0uODcsMi4xOSwyLjE5LDAsMCwwLS4xOC0uOSwyLDIsMCwwLDAtLjUtLjcxLDIuMjMsMi4yMywwLDAsMC0uNzMtLjQ3LDIuMjYsMi4yNiwwLDAsMC0uOS0uMTgsNCw0LDAsMCwwLTEuMzEtMi40MSwzLjgxLDMuODEsMCwwLDAtMi41Ny0xLDMuNjMsMy42MywwLDAsMC0xLjI4LjIyLDQuMTksNC4xOSwwLDAsMC0xLjEyLjYxLDMuNjUsMy42NSwwLDAsMC0uODYuOTMsMy45MiwzLjkyLDAsMCwwLS41MywxLjE4LDMsMywwLDAsMC0uODYtLjUxLDIuNjUsMi42NSwwLDAsMC0xLS4xOCwyLjc0LDIuNzQsMCwwLDAtMS4xLjIyLDIuOTEsMi45MSwwLDAsMC0uODkuNiwyLjczLDIuNzMsMCwwLDAtLjYuOSwyLjY5LDIuNjksMCwwLDAtLjIyLDEuMDksMi43NCwyLjc0LDAsMCwwLC4yMiwxLjEsMi41OSwyLjU5LDAsMCwwLC42Ljg5LDIuNzcsMi43NywwLDAsMCwuODkuNjEsMi45MSwyLjkxLDAsMCwwLDEuMS4yMWguODRsLS41NiwxLjEzaC0uMjhhMy43LDMuNywwLDAsMS0xLjUzLS4zMSwzLjksMy45LDAsMCwxLTEuMjUtLjg1LDMuOTEsMy45MSwwLDAsMS0xLTMuOTIsNC4yOSw0LjI5LDAsMCwxLC40Ny0xLDMuOTQsMy45NCwwLDAsMSwuNzUtLjgzLDMuODgsMy44OCwwLDAsMSwxLS42MSwzLjgyLDMuODIsMCwwLDEtLjExLS45LDQsNCwwLDAsMSwuMzEtMS41M0E0LDQsMCwwLDEsMjEsMTguMTlhMy43LDMuNywwLDAsMSwxLjUzLS4zMSw0LDQsMCwwLDEsMS4xLjE1LDQuMTIsNC4xMiwwLDAsMSwxLjgzLDEuMTcsNC4zMiw0LjMyLDAsMCwxLC42MS45Miw0Ljg5LDQuODksMCwwLDEsMS41Mi4zLDUuMzQsNS4zNCwwLDAsMSwxLjMzLjcyQTUsNSwwLDAsMSwzMCwyMi4yMyw0LjczLDQuNzMsMCwwLDEsMzAuNjcsMjMuNjFabS0xMi4xNC01TDE2Ljk0LDE3bC44LS44LDEuNTksMS41OVptMyw0YTUsNSwwLDAsMSwzLjI4LTIuMzUsMi43NiwyLjc2LDAsMCwwLTEtLjkxQTIuNjgsMi42OCwwLDAsMCwyMi41MSwxOWEyLjc0LDIuNzQsMCwwLDAtMS4xLjIyLDIuOTMsMi45MywwLDAsMC0uODkuNjEsMi43OSwyLjc5LDAsMCwwLS44MiwyLDIuNDQsMi40NCwwLDAsMCwuMDcuNTksMy45MywzLjkzLDAsMCwxLC40OSwwQTQuMTUsNC4xNSwwLDAsMSwyMS41NSwyMi41OVptMi44LDhhMS4xMiwxLjEyLDAsMCwxLC4xMy41MywxLjQyLDEuNDIsMCwwLDEtLjExLjU1LDEuNTcsMS41NywwLDAsMS0uMzEuNDUsMS4yMywxLjIzLDAsMCwxLS40NC4zLDEuNDYsMS40NiwwLDAsMS0uNTUuMTEsMS40MSwxLjQxLDAsMCwxLS41NC0uMTEsMS4zNywxLjM3LDAsMCwxLS40NS0uMywxLjU3LDEuNTcsMCwwLDEtLjMxLS40NSwxLjQyLDEuNDIsMCwwLDEtLjEtLjU1LDEuMjQsMS4yNCwwLDAsMSwuMTItLjUzTDIzLjA3LDI4Wk0yMy4wNywxNi43NUgyMlYxNC41aDEuMTJabTQuNjYsMTEuNTZhMS4yNCwxLjI0LDAsMCwxLC4xMi41MywxLjI2LDEuMjYsMCwwLDEtLjExLjU1LDEuMzgsMS4zOCwwLDAsMS0uNzUuNzUsMS40MSwxLjQxLDAsMCwxLTEuMDksMCwxLjM4LDEuMzgsMCwwLDEtLjc1LS43NSwxLjQyLDEuNDIsMCwwLDEtLjExLS41NSwxLjI0LDEuMjQsMCwwLDEsLjEyLS41M2wxLjI5LTIuNTZabS0xLjI0LTkuNjgtLjgtLjgsMS41OS0xLjU5LjguOFoiLz48L3N2Zz4=";

                // thunderstorms
                case 15:
                // mostly cloudy w thunderstorms
                case 16:
                // partly sunny w thunderstorms
                case 17:
                    // lightning icon
                    return useFile ? GetImageUri("lightning.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTMwLjY3LDIzLjYxYTMuMzYsMy4zNiwwLDAsMSwxLC40NiwzLjMxLDMuMzEsMCwwLDEsLjguNzQsMy40MSwzLjQxLDAsMCwxLC43MSwyLjA3LDMuNCwzLjQsMCwwLDEtMSwyLjM4LDMuNDksMy40OSwwLDAsMS0xLjA4LjczLDMuNCwzLjQsMCwwLDEtMS4zMS4yNkgyNi40NWwxLjEyLTEuMTNoMi4yNUEyLjE5LDIuMTksMCwwLDAsMzAuNywyOWEyLjI5LDIuMjksMCwwLDAsMS4yLTEuMiwyLjI5LDIuMjksMCwwLDAsLjE3LS44NywyLjIzLDIuMjMsMCwwLDAtLjE4LS45LDIuMDgsMi4wOCwwLDAsMC0uNS0uNzEsMi40LDIuNCwwLDAsMC0xLjYyLS42NSwzLjkzLDMuOTMsMCwwLDAtLjQ1LTEuMzQsNC4wOSw0LjA5LDAsMCwwLS44Ni0xLjA3LDQsNCwwLDAsMC0xLjE4LS43MSwzLjgyLDMuODIsMCwwLDAtMS40LS4yNSwzLjYzLDMuNjMsMCwwLDAtMS4yOC4yMiw0LjE5LDQuMTksMCwwLDAtMS4xMi42MSwzLjY1LDMuNjUsMCwwLDAtLjg2LjkzLDMuOTIsMy45MiwwLDAsMC0uNTMsMS4xOCwzLDMsMCwwLDAtLjg2LS41MSwyLjY1LDIuNjUsMCwwLDAtMS0uMTgsMi43NCwyLjc0LDAsMCwwLTEuMS4yMiwyLjkxLDIuOTEsMCwwLDAtLjg5LjYsMi43MywyLjczLDAsMCwwLS42LjksMi42OSwyLjY5LDAsMCwwLS4yMiwxLjA5LDIuNzQsMi43NCwwLDAsMCwuMjIsMS4xLDIuNTksMi41OSwwLDAsMCwuNi44OSwyLjc3LDIuNzcsMCwwLDAsLjg5LjYxLDIuOTEsMi45MSwwLDAsMCwxLjEuMjFoMi4yNUwyMiwzMC4yNUgyMC4yNmEzLjc4LDMuNzgsMCwwLDEtMS41NC0uMzEsMy45MywzLjkzLDAsMCwxLTIuNC0zLjYzLDMuNzksMy43OSwwLDAsMSwuMjEtMS4yNiwzLjUsMy41LDAsMCwxLTEtMS4xOCwzLjIzLDMuMjMsMCwwLDEtLjM1LTEuNDksMy4xOSwzLjE5LDAsMCwxLC4yNi0xLjMxQTMuNDksMy40OSwwLDAsMSwxNi4xOSwyMGEzLjI5LDMuMjksMCwwLDEsMi4zOC0xaC4xOUwxOSwxOWEzLjc2LDMuNzYsMCwwLDEsLjYxLS45NSw0LjI4LDQuMjgsMCwwLDEsLjgzLS43MSwzLjY3LDMuNjcsMCwwLDEsMS0uNDUsMy43NywzLjc3LDAsMCwxLDEuMTEtLjE2LDMuODcsMy44NywwLDAsMSwxLjQuMjYsNC4wOSw0LjA5LDAsMCwxLDEuMTkuNzFBMy44MywzLjgzLDAsMCwxLDI2LDE4LjhhMy43LDMuNywwLDAsMSwuNDQsMS4zNSw0Ljg5LDQuODksMCwwLDEsMS40MS4zNyw1LjE5LDUuMTksMCwwLDEsMS4yMi43Myw1LjQxLDUuNDEsMCwwLDEsMSwxLjA1QTQuODYsNC44NiwwLDAsMSwzMC42NywyMy42MVptLTkuMTItMWE0Ljc5LDQuNzksMCwwLDEsLjctLjkxLDQuNjgsNC42OCwwLDAsMSwuODgtLjczLDUuNDQsNS40NCwwLDAsMSwxLS41Miw1LjIzLDUuMjMsMCwwLDEsMS4xMi0uMjcsMi44NywyLjg3LDAsMCwwLS4zNS0uOTEsMi43NiwyLjc2LDAsMCwwLS42Mi0uNzMsMi44MywyLjgzLDAsMCwwLS44My0uNDcsMi42MywyLjYzLDAsMCwwLTEtLjE3LDIuNzMsMi43MywwLDAsMC0xLC4xOSwyLjY0LDIuNjQsMCwwLDAtLjg2LjU1LDIuODIsMi44MiwwLDAsMC0uNjIuODIsMi44NiwyLjg2LDAsMCwwLS4yOCwxLDIuNywyLjcsMCwwLDAtLjU1LS4yNCwyLDIsMCwwLDAtLjYtLjA5LDIuMTIsMi4xMiwwLDAsMC0uODcuMTgsMi4zOSwyLjM5LDAsMCwwLS43Mi40OCwyLjMzLDIuMzMsMCwwLDAtLjQ4LjcyLDIuMTMsMi4xMywwLDAsMC0uMTguODgsMi4xNywyLjE3LDAsMCwwLC4xOS45LDIuMTUsMi4xNSwwLDAsMCwuNTUuNzUsMy44OCwzLjg4LDAsMCwxLDMuMi0xLjY1QTQuMTUsNC4xNSwwLDAsMSwyMS41NSwyMi41OVptMSw5LjkxTDI0Ljc2LDI4aC0ybDEuNjktMy4zOGgybC0xLjEzLDIuMjZoMi44MVoiLz48L3N2Zz4=";

                // flurries
                case 19:
                // mostly cloudy w flurries
                case 20:
                // partly sunny w flurries
                case 21:
                // snow
                case 22:
                // mostly cloudy w snow
                case 23:
                // ice
                case 24:
                // sleet
                case 25:
                // freezing rain
                case 26:
                    // suncloudhail icon
                    return useFile ? GetImageUri("suncloudhail.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTE3LjQ1LDIwLjM4SDE1LjJWMTkuMjVoMi4yNVptMTMuMjIsMS4yM2EzLjM2LDMuMzYsMCwwLDEsMSwuNDYsMy41MiwzLjUyLDAsMCwxLC44Ljc0LDMuNDEsMy40MSwwLDAsMSwuNzEsMi4wNywzLjQsMy40LDAsMCwxLTEsMi4zOCwzLjQ5LDMuNDksMCwwLDEtMS4wOC43MywzLjQsMy40LDAsMCwxLTEuMzEuMjZIMjguN1YyNy4xMmgxLjEyQTIuMTksMi4xOSwwLDAsMCwzMC43LDI3YTIuMjksMi4yOSwwLDAsMCwxLjItMS4yLDIuMjksMi4yOSwwLDAsMCwuMTctLjg3LDIuMTksMi4xOSwwLDAsMC0uMTgtLjksMiwyLDAsMCwwLS41LS43MSwyLjQsMi40LDAsMCwwLTEuNjItLjY1LDMuODEsMy44MSwwLDAsMC0uNDUtMS4zMyw0LDQsMCwwLDAtLjg2LTEuMDgsNCw0LDAsMCwwLTEuMTgtLjcxLDMuODIsMy44MiwwLDAsMC0xLjQtLjI1LDMuNjMsMy42MywwLDAsMC0xLjI4LjIyLDQuMTksNC4xOSwwLDAsMC0xLjEyLjYxLDMuNjUsMy42NSwwLDAsMC0uODYuOTMsMy45MiwzLjkyLDAsMCwwLS41MywxLjE4LDMsMywwLDAsMC0uODYtLjUxLDIuNjUsMi42NSwwLDAsMC0xLS4xOCwyLjc0LDIuNzQsMCwwLDAtMS4xLjIyLDIuOTEsMi45MSwwLDAsMC0uODkuNiwyLjczLDIuNzMsMCwwLDAtLjYuOSwyLjY5LDIuNjksMCwwLDAtLjIyLDEuMDksMi43NCwyLjc0LDAsMCwwLC4yMiwxLjEsMi41OSwyLjU5LDAsMCwwLC42Ljg5LDIuNzcsMi43NywwLDAsMCwuODkuNjEsMi45MSwyLjkxLDAsMCwwLDEuMS4yMWguNTZ2MS4xM2gtLjU2YTMuNzgsMy43OCwwLDAsMS0xLjU0LS4zMSwzLjkzLDMuOTMsMCwwLDEtMi4yMy00Ljc2LDMuOCwzLjgsMCwwLDEsMS4yMi0xLjg2LDMuODgsMy44OCwwLDAsMSwxLS42MSwzLjgyLDMuODIsMCwwLDEtLjExLS45LDQsNCwwLDAsMSwuMzEtMS41M0EzLjksMy45LDAsMCwxLDE5LjczLDE3LDMuOTQsMy45NCwwLDAsMSwyMSwxNi4xOWEzLjc4LDMuNzgsMCwwLDEsMS41NC0uMzEsNCw0LDAsMCwxLDEuMS4xNSw0LDQsMCwwLDEsMS44MywxLjE3LDQuMzIsNC4zMiwwLDAsMSwuNjEuOTIsNC44OSw0Ljg5LDAsMCwxLDEuNTIuMyw1LjQsNS40LDAsMCwxLDEuMzQuNzJBNS4yMiw1LjIyLDAsMCwxLDMwLDIwLjIzLDQuNzMsNC43MywwLDAsMSwzMC42NywyMS42MVptLTEyLjE0LTVMMTYuOTQsMTVsLjgtLjgsMS41OSwxLjU5Wm0zLDRhNSw1LDAsMCwxLDMuMjgtMi4zNSwyLjc2LDIuNzYsMCwwLDAtMS0uOTFBMi42OCwyLjY4LDAsMCwwLDIyLjUxLDE3YTIuNzQsMi43NCwwLDAsMC0xLjEuMjIsMi45MywyLjkzLDAsMCwwLS44OS42MSwyLjc5LDIuNzksMCwwLDAtLjgyLDIsMi40NCwyLjQ0LDAsMCwwLC4wNy41OSwzLjkzLDMuOTMsMCwwLDEsLjQ5LDBBNC4xNSw0LjE1LDAsMCwxLDIxLjU1LDIwLjU5Wm0xLjUyLTUuODRIMjJWMTIuNWgxLjEyWm0wLDEwLjI3YTEsMSwwLDAsMSwuMzguMDcsMSwxLDAsMCwxLC41My41My45NC45NCwwLDAsMSwwLC43NiwxLDEsMCwwLDEtLjUzLjUzLDEsMSwwLDAsMS0uMzguMDcsMSwxLDAsMCwxLS4zOC0uMDcsMSwxLDAsMCwxLS4zMS0uMjIuODUuODUsMCwwLDEtLjIxLS4zMS45NC45NCwwLDAsMSwwLS43Ni44NS44NSwwLDAsMSwuMjEtLjMxLDEsMSwwLDAsMSwuMzEtLjIyQTEsMSwwLDAsMSwyMy4wNywyNVptMCwzLjM3YTEsMSwwLDAsMSwuMzguMDgsMSwxLDAsMCwxLC4zMi4yMUExLjQxLDEuNDEsMCwwLDEsMjQsMjlhMSwxLDAsMCwxLC4wOC4zOSwxLDEsMCwwLDEtLjA4LjM4LDEuNDEsMS40MSwwLDAsMS0uMjEuMzEsMSwxLDAsMCwxLS4zMi4yMSwxLDEsMCwwLDEtLjM4LjA4LDEsMSwwLDAsMS0uMzgtLjA4Ljg1Ljg1LDAsMCwxLS4zMS0uMjEsMSwxLDAsMCwxLS4yMS0uMzEuODQuODQsMCwwLDEtLjA4LS4zOC44NS44NSwwLDAsMSwuMDgtLjM5LDEsMSwwLDAsMSwuMjEtLjMxLjg1Ljg1LDAsMCwxLC4zMS0uMjFBMSwxLDAsMCwxLDIzLjA3LDI4LjM5Wm0zLjM4LTQuNWExLDEsMCwwLDEsLjM4LjA4LDEsMSwwLDAsMSwuNTIuNTIsMSwxLDAsMCwxLC4wOC4zOSwxLDEsMCwwLDEtLjA4LjM4LDEsMSwwLDAsMS0uNTIuNTIsMSwxLDAsMCwxLS4zOC4wOCwxLDEsMCwwLDEtLjM5LS4wOCwxLDEsMCwwLDEtLjUyLS41MiwxLDEsMCwwLDEtLjA4LS4zOCwxLDEsMCwwLDEsLjA4LS4zOSwxLDEsMCwwLDEsLjUyLS41MkExLDEsMCwwLDEsMjYuNDUsMjMuODlabTAsMy4zOGExLDEsMCwwLDEsLjM4LjA3LDEuMTYsMS4xNiwwLDAsMSwuMzEuMjIuODUuODUsMCwwLDEsLjIxLjMxLjk0Ljk0LDAsMCwxLDAsLjc2Ljg1Ljg1LDAsMCwxLS4yMS4zMSwxLjE2LDEuMTYsMCwwLDEtLjMxLjIyLDEsMSwwLDAsMS0uMzguMDcsMSwxLDAsMCwxLS4zOS0uMDcsMS4xNiwxLjE2LDAsMCwxLS4zMS0uMjIuODUuODUsMCwwLDEtLjIxLS4zMS45NC45NCwwLDAsMSwwLS43Ni44NS44NSwwLDAsMSwuMjEtLjMxLDEuMTYsMS4xNiwwLDAsMSwuMzEtLjIyQTEsMSwwLDAsMSwyNi40NSwyNy4yN1ptMC0xMC42NC0uOC0uOCwxLjU5LTEuNTkuOC44WiIvPjwvc3ZnPg==";

                // rain and snow
                case 29:
                    // rainsnow icon
                    return useFile ? GetImageUri("rainsnow.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTMwLjY3LDIzLjYxYTMuMzYsMy4zNiwwLDAsMSwxLC40NiwzLjMxLDMuMzEsMCwwLDEsLjguNzQsMy40MSwzLjQxLDAsMCwxLC43MSwyLjA3LDMuMjcsMy4yNywwLDAsMS0uMTcsMSwzLjM2LDMuMzYsMCwwLDEtLjQ1LjkxLDMuNzcsMy43NywwLDAsMS0uNzEuNzNBMy41NCwzLjU0LDAsMCwxLDMxLDMwVjI4LjgyYTIuMzgsMi4zOCwwLDAsMCwuODItLjgyLDIuMjgsMi4yOCwwLDAsMCwuMTItMiwyLjA4LDIuMDgsMCwwLDAtLjUtLjcxLDIuNCwyLjQsMCwwLDAtMS42Mi0uNjUsMy45MywzLjkzLDAsMCwwLS40NS0xLjM0LDQuMDksNC4wOSwwLDAsMC0uODYtMS4wNyw0LDQsMCwwLDAtMS4xOC0uNzEsMy44MiwzLjgyLDAsMCwwLTEuNC0uMjUsMy42MywzLjYzLDAsMCwwLTEuMjguMjIsNC4xOSw0LjE5LDAsMCwwLTEuMTIuNjEsMy42NSwzLjY1LDAsMCwwLS44Ni45MywzLjkyLDMuOTIsMCwwLDAtLjUzLDEuMTgsMywzLDAsMCwwLS44Ni0uNTEsMi42NSwyLjY1LDAsMCwwLTEtLjE4LDIuNzQsMi43NCwwLDAsMC0xLjEuMjIsMi45MSwyLjkxLDAsMCwwLS44OS42LDIuNzMsMi43MywwLDAsMC0uNi45LDIuNjksMi42OSwwLDAsMC0uMjIsMS4wOSwyLjc0LDIuNzQsMCwwLDAsLjIyLDEuMSwyLjU5LDIuNTksMCwwLDAsLjYuODksMi43NywyLjc3LDAsMCwwLC44OS42MSwyLjkxLDIuOTEsMCwwLDAsMS4xLjIxbC0uNTQsMS4wOGE0LDQsMCwwLDEtMS4zNS0uNDQsMy44MywzLjgzLDAsMCwxLTEuMDgtLjg3LDQsNCwwLDAsMS0uNzEtMS4xOCwzLjg3LDMuODcsMCwwLDEtLjI2LTEuNCw0LDQsMCwwLDEsLjMxLTEuNTMsNCw0LDAsMCwxLC44NS0xLjI1LDQuMTIsNC4xMiwwLDAsMSwxLjI0LS44NCwzLjc4LDMuNzgsMCwwLDEsMS41NC0uMzEsNC4xNSw0LjE1LDAsMCwxLDEuMjkuMjEsNS4xNyw1LjE3LDAsMCwxLDEuODQtMS44LDQuNTksNC41OSwwLDAsMSwxLjE5LS40OSw0Ljc0LDQuNzQsMCwwLDEsMS4zLS4xOCw0LjgzLDQuODMsMCwwLDEsMS41Ny4yNiw1LjEsNS4xLDAsMCwxLDEuMzkuNzIsNSw1LDAsMCwxLDEuODMsMi41MVptLTYuODgsN2ExLjIzLDEuMjMsMCwwLDEsMCwxLjA4LDEuMzgsMS4zOCwwLDAsMS0uNzUuNzUsMS40MSwxLjQxLDAsMCwxLS41NC4xMSwxLjQ2LDEuNDYsMCwwLDEtLjU1LS4xMSwxLjIzLDEuMjMsMCwwLDEtLjQ0LS4zLDEuNTcsMS41NywwLDAsMS0uMzEtLjQ1LDEuNDIsMS40MiwwLDAsMS0uMTEtLjU1LDEuMTIsMS4xMiwwLDAsMSwuMTMtLjUzTDIyLjUxLDI4Wm00LjY5LTIuODRhLjUuNSwwLDAsMSwuMzkuMTcuNTMuNTMsMCwwLDEsLjE2LjM5LjUyLjUyLDAsMCwxLS4yOC40OGwtLjYyLjM2LjYyLjM3QS41Mi41MiwwLDAsMSwyOSwzMGEuNTQuNTQsMCwwLDEtLjE3LjQuNTMuNTMsMCwwLDEtLjM5LjE2LjUyLjUyLDAsMCwxLS4yNC0uMDVMMjgsMzAuMzdsLS4yMy0uMTQtLjItLjEzYzAsLjExLDAsLjI0LDAsLjM5YTEuMzksMS4zOSwwLDAsMSwwLC40MS43Mi43MiwwLDAsMS0uMTYuMzQuNDkuNDksMCwwLDEtLjM4LjE0LjUuNSwwLDAsMS0uMzktLjE0LjcyLjcyLDAsMCwxLS4xNi0uMzQsMS4zOSwxLjM5LDAsMCwxLDAtLjQxYzAtLjE1LDAtLjI4LDAtLjM5bC0uMjEuMTMtLjIyLjE0LS4yNC4xMWEuNDguNDgsMCwwLDEtLjIzLjA1LjU3LjU3LDAsMCwxLS40LS4xNkEuNTQuNTQsMCwwLDEsMjUsMzBhLjUyLjUyLDAsMCwxLC4yOC0uNDhsLjYxLS4zNy0uNjEtLjM2YS41Mi41MiwwLDAsMS0uMjgtLjQ4LjUzLjUzLDAsMCwxLC4xNi0uMzkuNS41LDAsMCwxLC4zOS0uMTcuNTIuNTIsMCwwLDEsLjI0LjA1LDEuMzQsMS4zNCwwLDAsMSwuMjQuMTJsLjIzLjE0LjIuMTJjMC0uMTEsMC0uMjQsMC0uMzlhMS4zOSwxLjM5LDAsMCwxLDAtLjQxLjcyLjcyLDAsMCwxLC4xNi0uMzQuNS41LDAsMCwxLC4zOS0uMTMuNDkuNDksMCwwLDEsLjM4LjEzLjcyLjcyLDAsMCwxLC4xNi4zNCwxLjM5LDEuMzksMCwwLDEsMCwuNDFjMCwuMTUsMCwuMjgsMCwuMzlsLjItLjEyLjIzLS4xNGExLjM0LDEuMzQsMCwwLDEsLjI0LS4xMkEuNTIuNTIsMCwwLDEsMjguNDgsMjcuNzJaIi8+PC9zdmc+";

                // hot
                case 30:
                // cold
                case 31:
                    // temperature icon
                    return useFile ? GetImageUri("temperature.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PGRlZnM+PHN0eWxlPi5he2ZvbnQtc2l6ZToxNnB4O2ZvbnQtZmFtaWx5OkZ1bGxNREwyQXNzZXRzLCBGdWxsIE1ETDIgQXNzZXRzO308L3N0eWxlPjwvZGVmcz48dGl0bGU+d2VhdGhlcmljb25zPC90aXRsZT48dGV4dCBjbGFzcz0iYSIgdHJhbnNmb3JtPSJ0cmFuc2xhdGUoMTUgMzMpIj7up4o8L3RleHQ+PC9zdmc+";

                // windy
                case 32:
                    // wind icon
                    return useFile ? GetImageUri("wind.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTI4LjcsMTguMTJhMy40LDMuNCwwLDAsMS0xLDIuMzksMy40OSwzLjQ5LDAsMCwxLTEuMDguNzMsMy40LDMuNCwwLDAsMS0xLjMxLjI2SDE1LjJWMjAuMzhIMjUuMzJhMi4yLDIuMiwwLDAsMCwuODgtLjE4QTIuMjksMi4yOSwwLDAsMCwyNy40LDE5YTIuMzQsMi4zNCwwLDAsMCwwLTEuNzUsMi4yOSwyLjI5LDAsMCwwLTEuMi0xLjIsMi4xOSwyLjE5LDAsMCwwLS44OC0uMTcsMi4xMSwyLjExLDAsMCwwLS44Ny4xNywyLjM5LDIuMzksMCwwLDAtLjcyLjQ4LDIuMzMsMi4zMywwLDAsMC0uNDguNzIsMi4xMiwyLjEyLDAsMCwwLS4xOC44N0gyMmEzLjE4LDMuMTgsMCwwLDEsLjI2LTEuMywzLjQ5LDMuNDksMCwwLDEsLjczLTEuMDgsMy4yOSwzLjI5LDAsMCwxLDIuMzgtMSwzLjQsMy40LDAsMCwxLDEuMzEuMjYsMy40OSwzLjQ5LDAsMCwxLDEuMDguNzMsMy4zLDMuMywwLDAsMSwuNzIsMS4wOEEzLjE5LDMuMTksMCwwLDEsMjguNywxOC4xMlpNMzEsMjAuMzhhMi4xMSwyLjExLDAsMCwxLC44Ny4xNywyLjIxLDIuMjEsMCwwLDEsMS4yLDEuMiwyLjIyLDIuMjIsMCwwLDEsMCwxLjc1LDIuMjEsMi4yMSwwLDAsMS0xLjIsMS4yLDIuMTIsMi4xMiwwLDAsMS0uODcuMThIMjkuNTJhMi4xMywyLjEzLDAsMCwxLC4zLDEuMTIsMi4zOCwyLjM4LDAsMCwxLS4xNy44OCwyLjIsMi4yLDAsMCwxLS40OS43MSwyLDIsMCwwLDEtLjcxLjQ4LDIuMiwyLjIsMCwwLDEtLjg4LjE4LDIuMTIsMi4xMiwwLDAsMS0uODctLjE4LDIuMDcsMi4wNywwLDAsMS0uNzItLjQ4LDIuMTcsMi4xNywwLDAsMS0uNDgtLjcxLDIuMiwyLjIsMCwwLDEtLjE4LS44OGgxLjEzYTEuMjcsMS4yNywwLDAsMCwuMDguNDQsMS4wOSwxLjA5LDAsMCwwLDEsLjY4QTEsMSwwLDAsMCwyOCwyN2ExLjExLDEuMTEsMCwwLDAsLjM2LS4yNSwxLjIxLDEuMjEsMCwwLDAsLjI0LS4zNSwxLjEyLDEuMTIsMCwwLDAsMC0uODgsMS4yMSwxLjIxLDAsMCwwLS4yNC0uMzVBMS4xMSwxLjExLDAsMCwwLDI4LDI1YTEsMSwwLDAsMC0uNDQtLjA4SDE1LjJWMjMuNzVIMzFhMS4xMiwxLjEyLDAsMCwwLC40NC0uMDksMS4yMSwxLjIxLDAsMCwwLC4zNS0uMjQsMS4xMywxLjEzLDAsMCwwLC4yNC0uMzYsMSwxLDAsMCwwLC4wOS0uNDQsMSwxLDAsMCwwLS4wOS0uNDMsMS4xMywxLjEzLDAsMCwwLS4yNC0uMzYsMS4yMSwxLjIxLDAsMCwwLS4zNS0uMjRBMS4xMiwxLjEyLDAsMCwwLDMxLDIxLjVhMSwxLDAsMCwwLS40NC4wOSwxLjEsMS4xLDAsMCwwLS42LjYsMSwxLDAsMCwwLS4wOS40M0gyOC43YTIuMTEsMi4xMSwwLDAsMSwuMTctLjg3LDIuMjksMi4yOSwwLDAsMSwxLjItMS4yQTIuMTYsMi4xNiwwLDAsMSwzMSwyMC4zOFoiLz48L3N2Zz4=";

                // clear
                case 33:
                // mostly clear
                case 34:
                // partly cloudy
                case 35:
                // intermittent clouds
                case 36:
                // hazy moonlight
                case 37:
                // mostly cloudy
                case 38:
                    // cloudmoon icon
                    return useFile ? GetImageUri("cloudmoon.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTMwLjY3LDIzLjYxYTMuMzksMy4zOSwwLDAsMSwxLC40NywzLjI5LDMuMjksMCwwLDEsLjguNzMsMy4xOSwzLjE5LDAsMCwxLC41MiwxLDMuNCwzLjQsMCwwLDEsLjE5LDEuMTEsMy4xOSwzLjE5LDAsMCwxLS4yNywxLjMsMy40LDMuNCwwLDAsMS0xLjgsMS44LDMuMjMsMy4yMywwLDAsMS0xLjMxLjI3SDIwLjI2YTMuNzgsMy43OCwwLDAsMS0xLjU0LS4zMSwzLjkzLDMuOTMsMCwwLDEtMS4yNS0uODQsNC4xLDQuMSwwLDAsMS0uODQtMS4yNSwzLjc4LDMuNzgsMCwwLDEtLjMxLTEuNTRBMy44OCwzLjg4LDAsMCwxLDE2LjU0LDI1YTMuOCwzLjgsMCwwLDEsLjY1LTEuMTZBNC42LDQuNiwwLDAsMSwxNiwyMi42N2E0LjQxLDQuNDEsMCwwLDEtLjY3LTEuNTQsNC4zNCw0LjM0LDAsMCwwLDEsLjEyLDQuNDUsNC40NSwwLDAsMCwxLjc1LS4zNSw0Ljc4LDQuNzgsMCwwLDAsMS40NC0xLDQuNiw0LjYsMCwwLDAsMS0xLjQ0LDQuNDUsNC40NSwwLDAsMCwuMzUtMS43NSw0LjM0LDQuMzQsMCwwLDAtLjEyLTEsNC4yNSw0LjI1LDAsMCwxLDEuNC41OSw0LjM4LDQuMzgsMCwwLDEsMS4xMSwxLDQuNzQsNC43NCwwLDAsMSwuNzMsMS4zLDQuMzYsNC4zNiwwLDAsMSwuMjYsMS40OWMwLC4wNSwwLC4xLDAsLjE1czAsLjExLDAsLjE1aDBjLjI4LS4wOS41NS0uMTYuODItLjIyYTQuNjMsNC42MywwLDAsMSwuODUtLjA4LDQuOTQsNC45NCwwLDAsMSwxLjU4LjI2LDQuNzUsNC43NSwwLDAsMSwxLjM4LjcxLDUsNSwwLDAsMSwxLjEsMS4xQTQuOSw0LjksMCwwLDEsMzAuNjcsMjMuNjFabS04Ljc5LTYuMDVhNS4yMSw1LjIxLDAsMCwxLS41NCwxLjczLDUuNTgsNS41OCwwLDAsMS0yLjQ4LDIuNDgsNS4zNyw1LjM3LDAsMCwxLTEuNzMuNTQsMywzLDAsMCwwLC45MS43NCw0LjEsNC4xLDAsMCwxLDEuMDYtLjUsNC4xNiw0LjE2LDAsMCwxLDEuODItLjEyLDMuNDIsMy40MiwwLDAsMSwuNjMuMTYsNC4wNiw0LjA2LDAsMCwxLC42MS0uODIsMTAsMTAsMCwwLDEsLjc2LS42OSwzLjExLDMuMTEsMCwwLDAsLjE1LTEsMy4yOSwzLjI5LDAsMCwwLS4zMS0xLjQxQTMuMDgsMy4wOCwwLDAsMCwyMS44OCwxNy41NlptNy45NCwxMS41NkEyLjE5LDIuMTksMCwwLDAsMzAuNywyOWEyLjI5LDIuMjksMCwwLDAsMS4yLTEuMiwyLjI5LDIuMjksMCwwLDAsLjE3LS44NywyLjE5LDIuMTksMCwwLDAtLjE4LS45LDIsMiwwLDAsMC0uNS0uNzIsMi4yMywyLjIzLDAsMCwwLS43My0uNDcsMi40NCwyLjQ0LDAsMCwwLS45LS4xNyw0LDQsMCwwLDAtMS4zMS0yLjQxLDQsNCwwLDAsMC0xLjE4LS43MSwzLjgxLDMuODEsMCwwLDAtMS4zOS0uMjUsMy42MywzLjYzLDAsMCwwLTEuMjguMjIsMy45LDMuOSwwLDAsMC0xLjEyLjYsNCw0LDAsMCwwLS44Ni45MywzLjg3LDMuODcsMCwwLDAtLjUzLDEuMTksMywzLDAsMCwwLS44Ni0uNTEsMi42NSwyLjY1LDAsMCwwLTEtLjE4LDIuNzQsMi43NCwwLDAsMC0xLjEuMjIsMi45MSwyLjkxLDAsMCwwLS44OS42LDIuNzMsMi43MywwLDAsMC0uNi45LDIuNjksMi42OSwwLDAsMC0uMjIsMS4wOSwyLjc0LDIuNzQsMCwwLDAsLjIyLDEuMSwyLjU5LDIuNTksMCwwLDAsLjYuODksMi43NywyLjc3LDAsMCwwLC44OS42MSwyLjkxLDIuOTEsMCwwLDAsMS4xLjIxWiIvPjwvc3ZnPg==";

                // partly cloudy w showers
                case 39:
                // mostly cloudy w showers
                case 40:
                // partly cloudy w thunderstorms
                case 41:
                // mostly cloudy w thunderstorms
                case 42:
                    // moonrain icon
                    return useFile ? GetImageUri("moonrain.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTMwLjY3LDIzLjYxYTMuMzYsMy4zNiwwLDAsMSwxLC40NiwzLjMxLDMuMzEsMCwwLDEsLjguNzQsMy40MSwzLjQxLDAsMCwxLC43MSwyLjA3LDMuNCwzLjQsMCwwLDEtMSwyLjM4LDMuNDksMy40OSwwLDAsMS0xLjA4LjczLDMuNCwzLjQsMCwwLDEtMS4zMS4yNkgyNi40NWwxLjEyLTEuMTNoMi4yNUEyLjE5LDIuMTksMCwwLDAsMzAuNywyOWEyLjI5LDIuMjksMCwwLDAsMS4yLTEuMiwyLjI5LDIuMjksMCwwLDAsLjE3LS44NywyLjIzLDIuMjMsMCwwLDAtLjE4LS45LDIuMDgsMi4wOCwwLDAsMC0uNS0uNzEsMi40LDIuNCwwLDAsMC0xLjYyLS42NSwzLjkzLDMuOTMsMCwwLDAtLjQ1LTEuMzQsNC4wOSw0LjA5LDAsMCwwLS44Ni0xLjA3LDQsNCwwLDAsMC0xLjE4LS43MSwzLjgyLDMuODIsMCwwLDAtMS40LS4yNSwzLjYzLDMuNjMsMCwwLDAtMS4yOC4yMiw0LjE5LDQuMTksMCwwLDAtMS4xMi42MSwzLjY1LDMuNjUsMCwwLDAtLjg2LjkzLDMuOTIsMy45MiwwLDAsMC0uNTMsMS4xOCwzLDMsMCwwLDAtLjg2LS41MSwyLjY1LDIuNjUsMCwwLDAtMS0uMTgsMi43NCwyLjc0LDAsMCwwLTEuMS4yMiwyLjkxLDIuOTEsMCwwLDAtLjg5LjYsMi43MywyLjczLDAsMCwwLS42LjksMi42OSwyLjY5LDAsMCwwLS4yMiwxLjA5LDIuNzQsMi43NCwwLDAsMCwuMjIsMS4xLDIuNTksMi41OSwwLDAsMCwuNi44OSwyLjc3LDIuNzcsMCwwLDAsLjg5LjYxLDIuOTEsMi45MSwwLDAsMCwxLjEuMjFoMi4yNUwyMiwzMC4yNUgyMC4yNmEzLjc4LDMuNzgsMCwwLDEtMS41NC0uMzEsMy45MywzLjkzLDAsMCwxLTIuNC0zLjYzLDMuNzksMy43OSwwLDAsMSwuMjEtMS4yNiwzLjUsMy41LDAsMCwxLTEtMS4xOCwzLjIzLDMuMjMsMCwwLDEtLjM1LTEuNDksMy4xOSwzLjE5LDAsMCwxLC4yNi0xLjMxQTMuNDksMy40OSwwLDAsMSwxNi4xOSwyMGEzLjI5LDMuMjksMCwwLDEsMi4zOC0xaC4xOUwxOSwxOWEzLjc2LDMuNzYsMCwwLDEsLjYxLS45NSw0LjI4LDQuMjgsMCwwLDEsLjgzLS43MSwzLjY3LDMuNjcsMCwwLDEsMS0uNDUsMy43NywzLjc3LDAsMCwxLDEuMTEtLjE2LDMuODcsMy44NywwLDAsMSwxLjQuMjYsNC4wOSw0LjA5LDAsMCwxLDEuMTkuNzFBMy44MywzLjgzLDAsMCwxLDI2LDE4LjhhMy43LDMuNywwLDAsMSwuNDQsMS4zNSw0Ljg5LDQuODksMCwwLDEsMS40MS4zNyw1LjE5LDUuMTksMCwwLDEsMS4yMi43Myw1LjQxLDUuNDEsMCwwLDEsMSwxLjA1QTQuODYsNC44NiwwLDAsMSwzMC42NywyMy42MVptLTkuMTItMWE0Ljc5LDQuNzksMCwwLDEsLjctLjkxLDQuNjgsNC42OCwwLDAsMSwuODgtLjczLDUuNDQsNS40NCwwLDAsMSwxLS41Miw1LjIzLDUuMjMsMCwwLDEsMS4xMi0uMjcsMi44NywyLjg3LDAsMCwwLS4zNS0uOTEsMi43NiwyLjc2LDAsMCwwLS42Mi0uNzMsMi44MywyLjgzLDAsMCwwLS44My0uNDcsMi42MywyLjYzLDAsMCwwLTEtLjE3LDIuNzMsMi43MywwLDAsMC0xLC4xOSwyLjY0LDIuNjQsMCwwLDAtLjg2LjU1LDIuODIsMi44MiwwLDAsMC0uNjIuODIsMi44NiwyLjg2LDAsMCwwLS4yOCwxLDIuNywyLjcsMCwwLDAtLjU1LS4yNCwyLDIsMCwwLDAtLjYtLjA5LDIuMTIsMi4xMiwwLDAsMC0uODcuMTgsMi4zOSwyLjM5LDAsMCwwLS43Mi40OCwyLjMzLDIuMzMsMCwwLDAtLjQ4LjcyLDIuMTMsMi4xMywwLDAsMC0uMTguODgsMi4xNywyLjE3LDAsMCwwLC4xOS45LDIuMTUsMi4xNSwwLDAsMCwuNTUuNzUsMy44OCwzLjg4LDAsMCwxLDMuMi0xLjY1QTQuMTUsNC4xNSwwLDAsMSwyMS41NSwyMi41OVptMSw5LjkxTDI0Ljc2LDI4aC0ybDEuNjktMy4zOGgybC0xLjEzLDIuMjZoMi44MVoiLz48L3N2Zz4=";

                // mostly cloudy w flurries
                case 43:
                // mostly cloudy w snow
                case 44:
                    // night snow
                    return useFile ? GetImageUri("nightsnow.png") : "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PHRpdGxlPndlYXRoZXJpY29uczwvdGl0bGU+PHBhdGggZD0iTTMwLjY3LDIxLjYxYTMuMzYsMy4zNiwwLDAsMSwxLC40NiwzLjUyLDMuNTIsMCwwLDEsLjguNzQsMy40MSwzLjQxLDAsMCwxLC43MSwyLjA3QTMuMjgsMy4yOCwwLDAsMSwzMywyNi4wNWEzLjcyLDMuNzIsMCwwLDEtLjU5LDEsMy40MywzLjQzLDAsMCwxLS45Ljc1LDMuMjcsMy4yNywwLDAsMS0xLjEyLjQxVjI3LjA1YTIsMiwwLDAsMCwuNjgtLjMsMi4xNSwyLjE1LDAsMCwwLC41NC0uNSwyLjA5LDIuMDksMCwwLDAsLjM1LS42NCwyLjI0LDIuMjQsMCwwLDAsLjEyLS43MywyLjE5LDIuMTksMCwwLDAtLjE4LS45LDIsMiwwLDAsMC0uNS0uNzEsMi4yMywyLjIzLDAsMCwwLS43My0uNDcsMi4yNiwyLjI2LDAsMCwwLS45LS4xOCw0LDQsMCwwLDAtMS4zMS0yLjQxLDQsNCwwLDAsMC0xLjE4LS43MSwzLjgxLDMuODEsMCwwLDAtMS4zOS0uMjUsMy42MywzLjYzLDAsMCwwLTEuMjguMjIsNC4xOSw0LjE5LDAsMCwwLTEuMTIuNjEsMy42NSwzLjY1LDAsMCwwLS44Ni45MywzLjkyLDMuOTIsMCwwLDAtLjUzLDEuMTgsMywzLDAsMCwwLS44Ni0uNTEsMi42NSwyLjY1LDAsMCwwLTEtLjE4LDIuNzQsMi43NCwwLDAsMC0xLjEuMjIsMi45MSwyLjkxLDAsMCwwLS44OS42LDIuNzMsMi43MywwLDAsMC0uNi45LDIuNjksMi42OSwwLDAsMC0uMjIsMS4wOSwzLDMsMCwwLDAsLjExLjgxLDIuNTksMi41OSwwLDAsMCwuMzUuNzMsMywzLDAsMCwwLC41My42LDIuNjEsMi42MSwwLDAsMCwuNjkuNDR2MS4xOUEzLjc2LDMuNzYsMCwwLDEsMTgsMjcuNTNhNC4zMiw0LjMyLDAsMCwxLS44OS0uODcsNC4xLDQuMSwwLDAsMS0uNTctMS4xLDMuNzMsMy43MywwLDAsMS0uMjEtMS4yNSwzLjgzLDMuODMsMCwwLDEsLjIzLTEuMywzLjc3LDMuNzcsMCwwLDEsLjY0LTEuMTZBNC44MSw0LjgxLDAsMCwxLDE2LDIwLjY3YTQuMzMsNC4zMywwLDAsMS0uNjctMS41NCw0LjM0LDQuMzQsMCwwLDAsMSwuMTIsNC40NSw0LjQ1LDAsMCwwLDEuNzUtLjM1LDQuNzgsNC43OCwwLDAsMCwxLjQ0LTEsNC42LDQuNiwwLDAsMCwxLTEuNDQsNC40NSw0LjQ1LDAsMCwwLC4zNS0xLjc1LDQuMzQsNC4zNCwwLDAsMC0uMTItMSw0LjI1LDQuMjUsMCwwLDEsMS40LjU5LDQuMzgsNC4zOCwwLDAsMSwxLjExLDEsNC43NCw0Ljc0LDAsMCwxLC43MywxLjMsNC4zNiw0LjM2LDAsMCwxLC4yNiwxLjQ5YzAsLjA1LDAsLjEsMCwuMTVzMCwuMTEsMCwuMTVBNC43MSw0LjcxLDAsMCwxLDI1LDE4LjJhNC45MSw0LjkxLDAsMCwxLC44Ni0uMDgsNC44Myw0LjgzLDAsMCwxLDEuNTcuMjYsNS4xLDUuMSwwLDAsMSwxLjM5LjcyLDQuNzgsNC43OCwwLDAsMSwxLjEsMS4xQTQuNzIsNC43MiwwLDAsMSwzMC42NywyMS42MVptLTkuMTItMWE1LDUsMCwwLDEsMS4zNy0xLjUsMy4zLDMuMywwLDAsMC0uMTYtMi4zOCwzLjA4LDMuMDgsMCwwLDAtLjg4LTEuMTUsNS4yMSw1LjIxLDAsMCwxLS41NCwxLjczLDUuNTgsNS41OCwwLDAsMS0yLjQ4LDIuNDgsNS4zNyw1LjM3LDAsMCwxLTEuNzMuNTQsMy4xOCwzLjE4LDAsMCwwLC45MS43NSw0LjE1LDQuMTUsMCwwLDEsMS4wNi0uNTEsMy43NSwzLjc1LDAsMCwxLDEuMTYtLjE3QTQuMTUsNC4xNSwwLDAsMSwyMS41NSwyMC41OVptMi43LDhhLjUyLjUyLDAsMCwxLC4yOC40OC41NC41NCwwLDAsMS0uMTcuNC41NC41NCwwLDAsMS0uMzkuMTcuNjguNjgsMCwwLDEtLjIzLS4wNSwxLjM0LDEuMzQsMCwwLDEtLjI0LS4xMmwtLjIzLS4xNC0uMi0uMTJjMCwuMTEsMCwuMjQsMCwuMzhhMS40NCwxLjQ0LDAsMCwxLDAsLjQyLjczLjczLDAsMCwxLS4xNi4zMy40Ni40NiwwLDAsMS0uMzguMTQuNDcuNDcsMCwwLDEtLjM5LS4xNEEuNzMuNzMsMCwwLDEsMjIsMzBhMS40NCwxLjQ0LDAsMCwxLDAtLjQyYzAtLjE0LDAtLjI3LDAtLjM4bC0uMi4xMi0uMjMuMTRhMS4zNCwxLjM0LDAsMCwxLS4yNC4xMi42OC42OCwwLDAsMS0uMjMuMDUuNTYuNTYsMCwwLDEtLjQtLjE3LjU0LjU0LDAsMCwxLS4xNy0uNC40Ni40NiwwLDAsMSwuMDgtLjI4LjU4LjU4LDAsMCwxLC4yMS0uMmwuNjEtLjM2LS42MS0uMzZhLjU4LjU4LDAsMCwxLS4yMS0uMi40Ni40NiwwLDAsMS0uMDgtLjI4LjUxLjUxLDAsMCwxLC4xNy0uNC41NC41NCwwLDAsMSwuMzktLjE3LjY5LjY5LDAsMCwxLC4yNC4wNSwxLjM0LDEuMzQsMCwwLDEsLjI0LjEybC4yMy4xNC4yLjEyYzAtLjExLDAtLjI0LDAtLjM4YTEuNDQsMS40NCwwLDAsMSwwLS40Mi43My43MywwLDAsMSwuMTYtLjMzLjQ3LjQ3LDAsMCwxLC4zOS0uMTQuNDYuNDYsMCwwLDEsLjM4LjE0LjczLjczLDAsMCwxLC4xNi4zMywxLjQ0LDEuNDQsMCwwLDEsMCwuNDJjMCwuMTQsMCwuMjcsMCwuMzhsLjItLjEyTDIzLjUsMjdhMS4zNCwxLjM0LDAsMCwxLC4yNC0uMTIuNjkuNjksMCwwLDEsLjI0LS4wNS41Ni41NiwwLDAsMSwuNTUuNTcuNTIuNTIsMCwwLDEtLjI4LjQ4bC0uNjIuMzZabTQuMjMtMi44OWEuNS41LDAsMCwxLC4zOS4xNy41My41MywwLDAsMSwuMTYuMzkuNTIuNTIsMCwwLDEtLjI4LjQ4bC0uNjIuMzYuNjIuMzdBLjUyLjUyLDAsMCwxLDI5LDI4YS41NC41NCwwLDAsMS0uMTcuNC41My41MywwLDAsMS0uMzkuMTYuNS41LDAsMCwxLS4yMy0uMDUsMS4zNCwxLjM0LDAsMCwxLS4yNC0uMTJsLS4yMy0uMTQtLjItLjEyYzAsLjExLDAsLjI0LDAsLjM5YTEuMzksMS4zOSwwLDAsMSwwLC40MS43Mi43MiwwLDAsMS0uMTYuMzQuNDkuNDksMCwwLDEtLjM4LjE0LjUuNSwwLDAsMS0uMzktLjE0LjcyLjcyLDAsMCwxLS4xNi0uMzQsMS4zOSwxLjM5LDAsMCwxLDAtLjQxYzAtLjE1LDAtLjI4LDAtLjM5bC0uMi4xMi0uMjMuMTRhMS4zNCwxLjM0LDAsMCwxLS4yNC4xMi41LjUsMCwwLDEtLjIzLjA1LjUyLjUyLDAsMCwxLS40LS4xN0EuNTQuNTQsMCwwLDEsMjUsMjhhLjQ0LjQ0LDAsMCwxLC4wOC0uMjguNTguNTgsMCwwLDEsLjIxLS4ybC42MS0uMzctLjYxLS4zNmEuNTguNTgsMCwwLDEtLjIxLS4yLjQ0LjQ0LDAsMCwxLS4wOC0uMjguNTguNTgsMCwwLDEsLjU2LS41Ni41Mi41MiwwLDAsMSwuMjQuMDUsMS4zNCwxLjM0LDAsMCwxLC4yNC4xMmwuMjMuMTQuMi4xMmMwLS4xMSwwLS4yNCwwLS4zOWExLjM5LDEuMzksMCwwLDEsMC0uNDEuNzIuNzIsMCwwLDEsLjE2LS4zNC41LjUsMCwwLDEsLjM5LS4xMy40OS40OSwwLDAsMSwuMzguMTMuNzIuNzIsMCwwLDEsLjE2LjM0LDEuMzksMS4zOSwwLDAsMSwwLC40MWMwLC4xNSwwLC4yOCwwLC4zOWwuMi0uMTIuMjMtLjE0YTEuMzQsMS4zNCwwLDAsMSwuMjQtLjEyQS41Mi41MiwwLDAsMSwyOC40OCwyNS43MloiLz48L3N2Zz4=";
                default:
                    return string.Empty;
            }
        }

        private string GetImageUri(string imagePath)
        {
            var serverUrl = _httpContext.HttpContext.Request.Scheme + "://" + _httpContext.HttpContext.Request.Host.Value;
            return $"{serverUrl}/images/{imagePath}";
        }

        private Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static class DialogIds
        {
            public const string GeographyPrompt = "geographyPrompt";
            public const string GetForecastResponseDialog = "getForecastResponseDialog";
            public const string GetCurrentWeatherResponseDialog = "getCurrentWeatherResponseDialog";
        }
    }
}
