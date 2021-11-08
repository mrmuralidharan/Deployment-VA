using HDPagerSkill.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HDPagerSkill.Services
{
    public class HDPagerService
    {
       
        public async Task<string> GetEscalationPoliciesMembersforPolicyName(IConfiguration configuration,string policyidtext)
        {
            var client = new HttpClient();

            string body;
            string ApiUrl= configuration.GetSection("pagerDutyUrl").Value;
            string strtoken = configuration.GetSection("pagerDutyToken").Value;
            
            List<EscalationPolicy> policydets = new List<EscalationPolicy>();

            EscalationPolicies escnew = new EscalationPolicies();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(ApiUrl),
                Headers =
                {


        { "accept", "application/vnd.pagerduty+json;version=2" },
        { "authorization", strtoken },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();

                JObject data = JObject.Parse(body);
                var policiescount = data.SelectToken("escalation_policies").Count();
                if (policiescount > 0)
                {
                    var policies = data.SelectToken("escalation_policies");
                    foreach (var policydata in policies)
                    {
                        var policyid = policydata.SelectToken("id").ToString().ToLower();
                        policyidtext = policyidtext.ToLower();
                        if (policyidtext == policyid)
                        {
                            EscalationPolicy policydetails = new EscalationPolicy();
                            List<UserInfo> userdets = new List<UserInfo>();

                            policydetails.id = policydata.SelectToken("id").ToString();
                            policydetails.policyname = policydata.SelectToken("summary").ToString();

                            var userlength = policydata.SelectToken("escalation_rules")[0].SelectToken("targets").Count();
                            var userlist = policydata.SelectToken("escalation_rules")[0].SelectToken("targets");
                            if (userlength > 0)
                            {
                                foreach (var userdet in userlist)
                                {
                                    UserInfo userinfo = new UserInfo();
                                    userinfo.users = userdet.SelectToken("summary").ToString();
                                    userdets.Add(userinfo);

                                }


                            }
                            policydetails.userdetails = userdets;
                            policydets.Add(policydetails);

                            Console.WriteLine(body);
                            Console.WriteLine(data);
                        }
                    }
                }
            }

            escnew.Policies = policydets;
            var jsonnew = JsonConvert.SerializeObject(escnew);
            return jsonnew;
        }


        public async  Task<string> GetEscalationPoliciesMembers(IConfiguration configuration,string policynametext)
        {
            var client = new HttpClient();

            string body;
            List<EscalationPolicy> policydets = new List<EscalationPolicy>();
         
            EscalationPolicies escnew = new EscalationPolicies();
            string ApiUrl = configuration.GetSection("pagerDutyUrl").Value;
            string strtoken = configuration.GetSection("pagerDutyToken").Value;


            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(ApiUrl),
                Headers =
                {
           
                    
        { "accept", "application/vnd.pagerduty+json;version=2" },
        { "authorization", strtoken },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
           
                JObject data = JObject.Parse(body);
                var policiescount = data.SelectToken("escalation_policies").Count();
                if (policiescount > 0)
                {
                    var policies = data.SelectToken("escalation_policies");
                    foreach (var policydata in policies)
                    {
                        var policyname = policydata.SelectToken("summary").ToString().ToLower();
                        policynametext = policynametext.ToLower();
                        if (policynametext == policyname)
                        { 
                        EscalationPolicy policydetails = new EscalationPolicy();
                        List<UserInfo> userdets = new List<UserInfo>();
           
                        policydetails.id = policydata.SelectToken("id").ToString();
                        policydetails.policyname = policydata.SelectToken("summary").ToString();

                        var userlength = policydata.SelectToken("escalation_rules")[0].SelectToken("targets").Count();
                        var userlist = policydata.SelectToken("escalation_rules")[0].SelectToken("targets");
                        if (userlength > 0)
                        {
                            foreach (var userdet in userlist)
                            {
                                UserInfo userinfo = new UserInfo();
                                userinfo.users = userdet.SelectToken("summary").ToString();
                                userdets.Add(userinfo);

                            }


                        }
                        policydetails.userdetails = userdets;
                        policydets.Add(policydetails);

                        Console.WriteLine(body);
                        Console.WriteLine(data);
                    }
                    }
                }
            }

            escnew.Policies = policydets;
            var jsonnew = JsonConvert.SerializeObject(escnew);
            return jsonnew;
        }
     

        public async Task<string> ListEscalationPolicies(IConfiguration configuration)
        {
            var client = new HttpClient();
            
            string body;
            List<EscalationPolicy> policydets = new List<EscalationPolicy>();
           
            EscalationPolicies escnew = new EscalationPolicies();
            string ApiUrl = configuration.GetSection("pagerDutyUrl").Value;
            string strtoken = configuration.GetSection("pagerDutyToken").Value;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(ApiUrl),
                Headers =
                {
                  
                    
        { "accept", "application/vnd.pagerduty+json;version=2" },
        { "authorization",strtoken },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
               
                JObject data = JObject.Parse(body);
                var policiescount = data.SelectToken("escalation_policies").Count();
                if (policiescount > 0)
                {
                    var policies = data.SelectToken("escalation_policies");
                    foreach (var policydata in policies)
                    {
                        EscalationPolicy policydetails = new EscalationPolicy();
                        List<UserInfo> userdets = new List<UserInfo>();
                        
                        policydetails.id = policydata.SelectToken("id").ToString();
                        policydetails.policyname = policydata.SelectToken("summary").ToString();

                        var userlength = policydata.SelectToken("escalation_rules")[0].SelectToken("targets").Count();
                        var userlist = policydata.SelectToken("escalation_rules")[0].SelectToken("targets");
                        if (userlength > 0)
                        {
                            foreach (var userdet in userlist)
                            {
                                UserInfo userinfo = new UserInfo();
                                userinfo.users = userdet.SelectToken("summary").ToString();
                                userdets.Add(userinfo);
                               
                            }
                            

                        }
                        policydetails.userdetails = userdets;
                        policydets.Add(policydetails);
                        
                        Console.WriteLine(body);
                        Console.WriteLine(data);
                    }
                }
            }

            escnew.Policies = policydets;
            var jsonnew= JsonConvert.SerializeObject(escnew);
            return jsonnew;
        }
        public async Task<string> createincidents(IConfiguration configuration,string title,string policynumber,string desc)
        {
            try
            {
                var strincidentname = title;
                var strpolicynumber = policynumber;
                var incidentnumber = "";
                var strserviceid = configuration.GetSection("pagerDutyService").Value; 
                var client = new HttpClient();
                var strfrom = configuration.GetSection("pagerDutyFrom").Value; 
      
                string strtoken = configuration.GetSection("pagerDutyToken").Value;
                var jsoncontent = "{\"incident\":{\"type\":\"incident\",\"title\":\""+ strincidentname + "\",\"service\":{\"id\":\""+strserviceid+"\",\"summary\":null,\"type\":\"service_reference\",\"self\":null,\"html_url\":null},\"urgency\":\"high\",\"body\":{\"type\":\"incident_body\",\"details\":\""+desc+"\"},\"escalation_policy\":{\"id\":\""+strpolicynumber+"\",\"summary\":null,\"type\":\"escalation_policy_reference\",\"self\":null,\"html_url\":null}}}";
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://api.pagerduty.com/incidents"),
                    Headers =
    {
        { "accept", "application/vnd.pagerduty+json;version=2" },
        { "from", strfrom },
        { "authorization",strtoken },
    },
                    Content = new StringContent(jsoncontent)
                    {
                        Headers =
        {
            ContentType = new MediaTypeHeaderValue("application/json")
        }
                    }

                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(body);
                    incidentnumber = data.SelectToken("incident").SelectToken("incident_number").ToString();
                    Console.WriteLine(body);
                }
                return incidentnumber;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    
    
        public async Task<EscalationPolicy> IsValidEscalationPolicy(IConfiguration configuration,string policynametext)
        {
            var client = new HttpClient();
            EscalationPolicy policydetails = new EscalationPolicy();
            string body;
            string ApiUrl = configuration.GetSection("pagerDutyUrl").Value;
            string strtoken = configuration.GetSection("pagerDutyToken").Value;
 var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(ApiUrl),
                Headers =
                {


        { "accept", "application/vnd.pagerduty+json;version=2" },
        { "authorization", strtoken },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();

                JObject data = JObject.Parse(body);
                var policiescount = data.SelectToken("escalation_policies").Count();
                if (policiescount > 0)
                {
                    var policies = data.SelectToken("escalation_policies");
                    foreach (var policydata in policies)
                    {
                        var policyname = policydata.SelectToken("summary").ToString().ToLower();
                        policynametext = policynametext.ToLower();
                        if (policynametext == policyname)
                        {
                           
                            List<UserInfo> userdets = new List<UserInfo>();

                            policydetails.id = policydata.SelectToken("id").ToString();
                            policydetails.policyname = policydata.SelectToken("summary").ToString();

                            var userlength = policydata.SelectToken("escalation_rules")[0].SelectToken("targets").Count();
                            var userlist = policydata.SelectToken("escalation_rules")[0].SelectToken("targets");
                            if (userlength > 0)
                            {
                                foreach (var userdet in userlist)
                                {
                                    UserInfo userinfo = new UserInfo();
                                    userinfo.users = userdet.SelectToken("summary").ToString();
                                    userdets.Add(userinfo);

                                }


                            }
                            policydetails.userdetails = userdets;
                            

                            
                        }
                    }
                }
            }

            return policydetails;
        }
    
    }

}
