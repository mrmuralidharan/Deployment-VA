// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace SkillServiceLibrary.Models.AzureMaps
{
    public class SearchResult
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "info")]
        public string Info { get; set; }

        /// <summary>
        /// Gets or sets ResultType string: POI, Street, Geography, Point Address, Address Range, and Cross Street.
        /// </summary>
        /// <value>
        /// The result type.
        /// </value>
        [JsonProperty(PropertyName = "type")]
        public string ResultType { get; set; }

        /// <summary>
        /// Gets or sets EntityType string.
        /// See https://docs.microsoft.com/en-us/rest/api/maps/search/getsearchaddressreverse#entitytype for more information.
        /// </summary>
        /// <value>
        /// The result type.
        /// </value>
        [JsonProperty(PropertyName = "entityType")]
        public string EntityType { get; set; }

        [JsonProperty(PropertyName = "entryPoints")]
        public EntryPoint[] EntryPoints { get; set; }

        [JsonProperty(PropertyName = "address")]
        public SearchAddress Address { get; set; }

        [JsonProperty(PropertyName = "position")]
        public LatLng Position { get; set; }

        [JsonProperty(PropertyName = "poi")]
        public PoiInfo Poi { get; set; }

        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }

        [JsonProperty(PropertyName = "viewport")]
        public Viewport Viewport { get; set; }

        [JsonProperty(PropertyName = "dist")]
        public double Distance { get; set; }

        [JsonProperty(PropertyName = "boundingBox")]
        public BoundingBox BoundingBox { get; set; }

        [JsonProperty(PropertyName = "dataSources")]
        public DataSources DataSources { get; set; }
    }
}