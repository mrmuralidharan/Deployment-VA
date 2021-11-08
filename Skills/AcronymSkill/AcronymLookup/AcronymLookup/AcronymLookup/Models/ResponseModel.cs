using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcronymLookup.Models
{
    public class ResponseModel
    {
        [JsonProperty(PropertyName = "responseMessage")]
        public string ResponseMessage { get; set; }
    }
}
