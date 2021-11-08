using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcronymLookup.Models
{
    public class AcronymModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Acronym")]
        public string Acronym { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Modified")]
        public string Modified { get; set; }

        [JsonProperty(PropertyName = "LastModified")]
        public string LastModified { get; set; }
    }
}
