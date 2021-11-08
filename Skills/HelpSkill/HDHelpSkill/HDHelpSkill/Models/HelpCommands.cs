// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;

namespace HDHelpSkill.Models
{
    public class HelpCommands
    {
        [JsonProperty(PropertyName = "O365helpCommands")]
        public string O365helpCommands { get; set; }
        [JsonProperty(PropertyName = "PagerhelpCommands")]
        public string PagerhelpCommands { get; set; }
        [JsonProperty(PropertyName = "ServicenowhelpCommands")]
        public string ServicenowhelpCommands { get; set; }
        [JsonProperty(PropertyName = "DepartmenthelpCommands")]
        public string DepartmenthelpCommands { get; set; }

        [JsonProperty(PropertyName = "WeatherhelpCommands")]
        public string WeatherhelpCommands { get; set; }
    }
}
