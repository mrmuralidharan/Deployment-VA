using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillServiceLibrary.Models.AzureMaps
{
    public class BoundingBox
    {
        [JsonProperty(PropertyName = "topLeftPoint")]
        public LatLng TopLeftPoint { get; set; }

        [JsonProperty(PropertyName = "btmRightPoint")]
        public LatLng BtmRightPoint { get; set; }
    }
}
