using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DepartmentSkill.Models.Action;

namespace DepartmentSkill.Models.Action
{
    public class ActionResult
    {
        public ActionResult()
        {

        }

        [JsonProperty("skillList")]
        public List<DeptInput> skillsList { get; set; }

        [JsonProperty("actionSuccess")]
        public bool ActionSuccess { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

    }
}
