using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillServiceLibrary.Models.AzureMaps
{
    public class DataSources
    {
        [JsonProperty(PropertyName = "geometry")]
        public GeoID Geometry { get; set; }
    }
}
