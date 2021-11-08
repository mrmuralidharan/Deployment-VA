using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Solutions.Responses;

namespace HDO365HealthStatus.Models
{
    public class HealthStatusCard : ICardData
    {
        public Value[] Value { get; set; }
    }
}
