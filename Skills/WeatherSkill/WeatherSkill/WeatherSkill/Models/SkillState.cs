// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Luis;

namespace WeatherSkill.Models
{
    public class SkillState
    {
        public SkillState()
        {
            Clear();
        }

        public string Token { get; internal set; }

        public WeatherSkillLuis LuisResult { get; internal set; }

        public string zipCode { get; set; }
        public string Geography { get; set; }

        public string Country { get; set; }

        public double Latitude { get; set; } = double.NaN;

        public double Longitude { get; set; } = double.NaN;

        public bool IsAction { get; set; }

        public bool IsZipWithSpace { get; set; }

        public void Clear()
        {
            Geography = string.Empty;
            Country = string.Empty;
            Latitude = double.NaN;
            Longitude = double.NaN;
            zipCode = string.Empty;
        }
    }
}
