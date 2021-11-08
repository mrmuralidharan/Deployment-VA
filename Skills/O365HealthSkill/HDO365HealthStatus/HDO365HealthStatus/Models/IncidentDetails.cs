using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HDO365HealthStatus.Models
{
    public class IncidentDetails
    {
        [JsonProperty("id")]
        public string id { get; set; }
    }
}
