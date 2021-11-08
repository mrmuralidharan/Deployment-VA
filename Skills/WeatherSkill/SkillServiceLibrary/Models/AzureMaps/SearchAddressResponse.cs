using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SkillServiceLibrary.Models.AzureMaps
{
    internal class SearchAddressResponse
    {
        public class Rootobject
        {
            [JsonProperty(PropertyName = "summary")]
            public Summary summary { get; set; }

            [JsonProperty(PropertyName = "addresses")]

            public Address[] addresses { get; set; }
        }

        public class Summary
        {
            [JsonProperty(PropertyName = "queryTime")]
            public int queryTime { get; set; }

            [JsonProperty(PropertyName = "numResults")]
            public int numResults { get; set; }
        }

        public class Address
        {
            [JsonProperty(PropertyName = "address")]
            public Address1 address { get; set; }

            [JsonProperty(PropertyName = "position")]
            public string position { get; set; }
        }

        public class Address1
        {
            [JsonProperty(PropertyName = "buildingNumber")]
            public string buildingNumber { get; set; }

            [JsonProperty(PropertyName = "streetNumber")]
            public string streetNumber { get; set; }

            [JsonProperty(PropertyName = "routeNumbers")]
            public object[] routeNumbers { get; set; }

            [JsonProperty(PropertyName = "street")]
            public string street { get; set; }

            [JsonProperty(PropertyName = "streetName")]
            public string streetName { get; set; }

            [JsonProperty(PropertyName = "streetNameAndNumber")]
            public string streetNameAndNumber { get; set; }

            [JsonProperty(PropertyName = "countryCode")]
            public string countryCode { get; set; }

            [JsonProperty(PropertyName = "countrySubdivision")]
            public string countrySubdivision { get; set; }

            [JsonProperty(PropertyName = "countrySecondarySubdivision")]
            public string countrySecondarySubdivision { get; set; }

            [JsonProperty(PropertyName = "municipality")]
            public string municipality { get; set; }

            [JsonProperty(PropertyName = "postalCode")]
            public string postalCode { get; set; }

            [JsonProperty(PropertyName = "municipalitySubdivision")]
            public string municipalitySubdivision { get; set; }

            [JsonProperty(PropertyName = "country")]
            public string country { get; set; }

            [JsonProperty(PropertyName = "countryCodeISO3")]
            public string countryCodeISO3 { get; set; }

            [JsonProperty(PropertyName = "freeformAddress")]
            public string freeformAddress { get; set; }

            [JsonProperty(PropertyName = "boundingBox")]
            public Boundingbox boundingBox { get; set; }

            [JsonProperty(PropertyName = "extendedPostalCode")]
            public string extendedPostalCode { get; set; }

            [JsonProperty(PropertyName = "countrySubdivisionName")]
            public string countrySubdivisionName { get; set; }

            [JsonProperty(PropertyName = "localName")]
            public string localName { get; set; }
        }

        public class Boundingbox
        {
            [JsonProperty(PropertyName = "northEast")]
            public string northEast { get; set; }

            [JsonProperty(PropertyName = "southWest")]
            public string southWest { get; set; }

            [JsonProperty(PropertyName = "entity")]
            public string entity { get; set; }
        }

    }
}
