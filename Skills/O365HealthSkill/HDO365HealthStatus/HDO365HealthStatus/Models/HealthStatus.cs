using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HDO365HealthStatus.Models
{
    public class HealthStatus
    {
        [JsonProperty("@odata.context")]
        public Uri OdataContext { get; set; }
        [JsonProperty("value")]
        public Value[] Value { get; set; }
    }

    public class Value
    {
        [JsonProperty("service")]
        public string serviceName { get; set; }

        [JsonProperty("status")]
        public string serviceStatus { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
