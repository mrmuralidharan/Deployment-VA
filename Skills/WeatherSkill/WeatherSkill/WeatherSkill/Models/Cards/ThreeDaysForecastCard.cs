using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Solutions.Responses;

namespace WeatherSkill.Models
{
    public class ThreeDaysForecastCard :ICardData
    {
        public string Location { get; set; }

        public string Date { get; set; }

        public int MinimumTemperature { get; set; }

        public int MaximumTemperature { get; set; }

        public string TempUnit { get; set; }

        public string ShortPhrase { get; set; }

        public string WindDescription { get; set; }

        public string DayIcon { get; set; }

        public string Speak { get; set; }

        public string Day1 { get; set; }

        public string IconDay1 { get; set; }

        public int MinimumTemperatureDay1 { get; set; }

        public int MaximumTemperatureDay1 { get; set; }

        public string TempUnit1 { get; set; }

        public string Day2 { get; set; }

        public string IconDay2 { get; set; }

        public int MinimumTemperatureDay2 { get; set; }

        public int MaximumTemperatureDay2 { get; set; }

        public string TempUnit2 { get; set; }

        public string Day3 { get; set; }

        public string IconDay3 { get; set; }

        public int MinimumTemperatureDay3 { get; set; }

        public int MaximumTemperatureDay3 { get; set; }

        public string TempUnit3 { get; set; }
    }
}