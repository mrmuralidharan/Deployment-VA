using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDPagerSkill.Models
{
    public class EscalationPolicy
    {
        public string id { get; set; }
        public string policyname { get; set; }
        public List<UserInfo> userdetails { get; set; }
    }
}
