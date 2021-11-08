using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DepartmentSkill.Models.Action
{
    public class DeptInput
    {
        [JsonProperty("DeptDetail")]
        public string DeptDetail { get; set; }
    }
}
