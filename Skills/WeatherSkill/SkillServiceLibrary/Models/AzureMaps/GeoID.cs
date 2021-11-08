using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillServiceLibrary.Models.AzureMaps
{
    public class GeoID
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
