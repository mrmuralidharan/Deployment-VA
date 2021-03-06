// <auto-generated>
// Code generated by luis:generate:cs
// Tool github: https://github.com/microsoft/botframework-cli
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
namespace Luis
{
    public partial class WeatherSkillLuis: IRecognizerConvert
    {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("alteredText")]
        public string AlteredText;

        public enum Intent {
            ChangeTemperatureUnit,
            CheckWeatherTime,
            ForecastWeather,
            CheckWeatherValue,
            GetWeatherAdvisory,
            None,
            QueryWeather
        };
        [JsonProperty("intents")]
        public Dictionary<Intent, IntentScore> Intents;

        public class _Entities
        {
            // Simple entities
            public string[] CityCountry;
            public string[] Historic;
            public string[] SuitableFor;
            public string[] WeatherCondition;
            public string[] WeatherRange;
            public string[] geographyV2City;
            public string[] geographyV2Country;

            // Built-in entities
            public DateTimeSpec[] datetime;
            public Dimension[] dimension;
            public GeographyV2[] geographyV2;
            public Temperature[] temperature;

            // Lists
            public string[][] AdditionalWeatherCondition;
            public string[][] WindDirectionUnit;


            // Composites
            public class _InstanceGeoDetails
            {
                public InstanceData[] geographyV2City;
                public InstanceData[] geographyV2Country;
            }
            public class GeoDetailsClass
            {
                public string[] geographyV2City;
                public string[] geographyV2Country;
                [JsonProperty("$instance")]
                public _InstanceGeoDetails _instance;
            }
            public GeoDetailsClass[] geoDetails;

            // Instance
            public class _Instance
            {
                public InstanceData[] AdditionalWeatherCondition;
                public InstanceData[] CityCountry;
                public InstanceData[] Historic;
                public InstanceData[] SuitableFor;
                public InstanceData[] WeatherCondition;
                public InstanceData[] WeatherRange;
                public InstanceData[] WindDirectionUnit;
                public InstanceData[] datetime;
                public InstanceData[] dimension;
                public InstanceData[] geoDetails;
                public InstanceData[] geographyV2;
                public InstanceData[] geographyV2City;
                public InstanceData[] geographyV2Country;
                public InstanceData[] temperature;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;
        }
        [JsonProperty("entities")]
        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties {get; set; }

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<WeatherSkillLuis>(
                JsonConvert.SerializeObject(
                    result,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Error = OnError }
                )
            );
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        private static void OnError(object sender, ErrorEventArgs args)
        {
            // If needed, put your custom error logic here
            Console.WriteLine(args.ErrorContext.Error.Message);
            args.ErrorContext.Handled = true;
        }

        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}
