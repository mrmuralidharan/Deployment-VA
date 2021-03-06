// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace SkillServiceLibrary.Models.AzureMaps
{
    /// <summary>
    /// An address.
    /// </summary>
    public class SearchAddress
    {
        /// <summary>
        /// Gets or sets the street number of an address.
        /// </summary>
        /// <value>
        /// The street number of an address.
        /// </value>
        [JsonProperty(PropertyName = "streetNumber")]
        public string StreetNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the street.
        /// </summary>
        /// <value>
        /// The name of the street.
        /// </value>
        [JsonProperty(PropertyName = "streetName")]
        public string StreetName { get; set; }

        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }

        [JsonProperty(PropertyName = "streetNameAndNumber")]
        public string StreetNameAndNumber { get; set; }

        /// <summary>
        /// Gets or sets a string specifying the subdivision name in the country or region for an address.
        /// This element is typically treated as the first order administrative subdivision,
        /// but in some cases it is the second, third, or fourth order subdivision in a country, dependency, or region.
        /// </summary>
        /// <value>
        /// A string specifying the subdivision name in the country or region for an address.
        /// This element is typically treated as the first order administrative subdivision,
        /// but in some cases it is the second, third, or fourth order subdivision in a country, dependency, or region.
        /// </value>
        /// 

        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "countrySubdivision")]
        public string CountrySubdivision { get; set; }

        [JsonProperty(PropertyName = "countrySubdivisionName")]
        public string CountrySubdivisionName { get; set; }

        [JsonProperty(PropertyName = "municipality")]
        public string Municipality { get; set; }

        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "postalName")]
        public string PostalName { get; set; }

        [JsonProperty(PropertyName = "extendedPostalCode")]
        public string ExtendedPostalCode { get; set; }

        /// <summary>
        /// Gets or sets a string specifying the subdivision name in the country or region for an address.
        /// This element is used when there is another level of subdivision information for a location, such as the county.
        /// </summary>
        /// <value>
        /// A string specifying the subdivision name in the country or region for an address.
        /// This element is used when there is another level of subdivision information for a location, such as the county.
        /// </value>
        [JsonProperty(PropertyName = "countrySecondarySubdivision")]
        public string CountrySecondarySubdivision { get; set; }

        /// <summary>
        /// Gets or sets a string specifying the country or region name of an address.
        /// </summary>
        /// <value>
        /// A string specifying the country or region name of an address.
        /// </value>
        [JsonProperty(PropertyName = "countryCodeISO3")]
        public string CountryCodeISO3 { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets a string specifying the complete address. This address may not include the country or region.
        /// </summary>
        /// <value>
        /// A string specifying the complete address. This address may not include the country or region.
        /// </value>
        [JsonProperty(PropertyName = "freeformAddress")]
        public string FreeformAddress { get; set; }

        /// <summary>
        /// Gets or sets a string specifying the populated place for the address.
        /// This typically refers to a city, but may refer to a suburb or a neighborhood in certain countries or regions.
        /// </summary>
        /// <value>
        /// A string specifying the populated place for the address.
        /// This typically refers to a city, but may refer to a suburb or a neighborhood in certain countries or regions.
        /// </value>
        [JsonProperty(PropertyName = "municipalitySubdivision")]
        public string MunicipalitySubdivision { get; set; }

        /// <summary>
        /// Gets or sets a string specifying the post code, postal code, or ZIP Code of an address.
        /// </summary>
        /// <value>
        /// A string specifying the post code, postal code, or ZIP Code of an address.
        /// </value>
        /// <returns></returns>
        internal Address ToBingAddress()
        {
            string addressLine = string.Empty;

            if (!string.IsNullOrEmpty(StreetNumber) && !string.IsNullOrEmpty(StreetName))
            {
                addressLine = StreetNumber + " " + StreetName;
            }
            else if (!string.IsNullOrEmpty(StreetName))
            {
                addressLine = StreetName;
            }

            string locality = MunicipalitySubdivision;

            if (!string.IsNullOrEmpty(locality) && locality.Contains(","))
            {
                locality = locality.Split(',')[0];
            }

            string postCode = PostalCode;

            if (!string.IsNullOrEmpty(postCode) && postCode.Contains(","))
            {
                postCode = postCode.Split(',')[0];
            }

            string extendedPostalCode = ExtendedPostalCode;

            return new Address()
            {
                AddressLine = addressLine,
                AdminDistrict = CountrySubdivisionName,
                AdminDistrict2 = CountrySecondarySubdivision,
                FormattedAddress = FreeformAddress,
                CountryRegion = CountryCodeISO3,
                Locality = locality,
                PostalCode = postCode,
                ExtendedPostalCode= extendedPostalCode
            };
        }
    }
}