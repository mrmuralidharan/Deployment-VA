// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Solutions;

namespace AcronymLookup.Services
{
    public class BotSettings : BotSettingsBase
    {
        public bool LogPersonalData { get; set; }

        public string CosmosDB_Id { get; set; }

        public string CosmosDB_ContainerId { get; set; }

        public string BlobConnectionString { get; set; }

        public string BlobContainerName { get; set; }

        public string NewAcronymCardName { get; set; }

        public string EditAcronymCardName { get; set; }
        public string ViewAcronymCardName { get; set; }
        public string GeneralAcronymCardName { get; set; }
    }
}