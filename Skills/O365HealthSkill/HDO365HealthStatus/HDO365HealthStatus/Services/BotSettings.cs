// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Solutions;

namespace HDO365HealthStatus.Services
{
    public class BotSettings : BotSettingsBase
    {
        public bool LogPersonalData { get; set; }

        public string MicrosoftLoginUrl { get; set; }

        public string TenantId { get; set; }

        public string GraphDefaultScope { get; set; }

        public string HealthStatusApiUrl { get; set; }

        public string GetIncidentApiUrl { get; set; }
    }
}