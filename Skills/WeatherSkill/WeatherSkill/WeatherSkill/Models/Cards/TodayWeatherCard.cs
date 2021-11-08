// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Solutions.Responses;

namespace WeatherSkill.Models
{
    public class TodayWeatherCard : ICardData
    {
        public string Location { get; set; }

        public string Date { get; set; }

        public int MinimumTemperature { get; set; }

        public int MaximumTemperature { get; set; }

        public string ShortPhrase { get; set; }

        public string WindDescription { get; set; }

        public string DayIcon { get; set; }

        public string Speak { get; set; }

        public string TempUnit { get; set; }
    }
}
