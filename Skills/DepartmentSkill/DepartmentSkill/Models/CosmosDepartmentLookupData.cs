using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DepartmentSkill.Models
{
    public class CosmosDepartmentLookupData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "deptNumber")]
        public string deptNumber { get; set; }

        public string deptName { get; set; }
        public string type { get; set; }
    }
}
