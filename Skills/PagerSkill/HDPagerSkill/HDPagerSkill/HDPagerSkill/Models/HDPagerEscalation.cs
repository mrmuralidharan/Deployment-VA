using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDPagerSkill.Models
{
    public class HDPagerEscalation
    {
        public string PolicyID { get; set; }
        public string PolicyName { get; set; }
        public string[] users { get; set; }
        public string user { get; set; }
        public string Desc { get; set; }

        public string Teamname { get; set; }
    }
}
