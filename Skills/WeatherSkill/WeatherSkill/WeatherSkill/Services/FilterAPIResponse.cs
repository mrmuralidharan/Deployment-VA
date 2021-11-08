using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeatherSkill.Models;

namespace WeatherSkill.Services
{
    public class FilterAPIResponse
    {
        public static void ExtractPostalCode(SkillState state, List<SkillServiceLibrary.Models.AzureMaps.SearchResult> res)
        {
            try
            {
                List<SkillServiceLibrary.Models.AzureMaps.SearchResult> countryQry = res.Where(x => x.Address.PostalCode != null && (x.Address.CountryCodeISO3 == "USA" || x.Address.CountryCodeISO3 == "CAN")).ToList();

                int resultIndexForCountry = countryQry.FindIndex(x => state.zipCode.Contains(x.Address.PostalCode));
                if (resultIndexForCountry >= 0)
                {
                    state.Latitude = countryQry[resultIndexForCountry].Position.Latitude;
                    state.Longitude = countryQry[resultIndexForCountry].Position.Longitude;
                }
                else
                {
                    List<SkillServiceLibrary.Models.AzureMaps.SearchResult> qryRes = res.Where(x => x.Address.PostalCode != null).ToList();
                    int resIndex = qryRes.FindIndex(x => state.zipCode.Contains(x.Address.PostalCode));
                    if (resIndex >= 0)
                    {
                        state.Latitude = res[resIndex].Position.Latitude;
                        state.Longitude = res[resIndex].Position.Longitude;
                    }
                    else
                    {
                        if (Regex.IsMatch(state.zipCode, @"[\s]"))
                        {
                            int indexRes = qryRes.FindIndex(x => state.zipCode.Contains(Regex.IsMatch(x.Address.PostalCode, @"[\s]") ? Regex.Replace(x.Address.PostalCode, @"[\s]", "") : x.Address.PostalCode));
                            if (indexRes >= 0)
                            {
                                state.Latitude = res[indexRes].Position.Latitude;
                                state.Longitude = res[indexRes].Position.Longitude;
                            }
                        }
                    }

                }

                #region MyRegion
                //List<SkillServiceLibrary.Models.AzureMaps.SearchResult> qryRes = res.Where(x => x.Address.PostalCode != null).ToList();
                //int resIndex = qryRes.FindIndex(x => x.Address.PostalCode.Contains(state.IsZipWithSpace ? Regex.Replace(state.zipCode, @"[-\s]", "") : state.zipCode));
                //if (resIndex >= 0)
                //{
                //    state.Latitude = res[resIndex].Position.Latitude;
                //    state.Longitude = res[resIndex].Position.Longitude;
                //}
                //else
                //{
                //    string[] extendedZipCode = res.Where(x => x.Address.ExtendedPostalCode != null).Select(x => x.Address.ExtendedPostalCode).ToArray();
                //    int Index = -1;
                //    string re = Array.Find(extendedZipCode, ele => ele.Contains(','));

                //    foreach (var item in extendedZipCode)
                //    {
                //        Index++;
                //        if (item.Contains(','))
                //        {
                //            string[] x = item.Split(',');
                //            string result = Array.Find(x, ele => ele.Contains(state.zipCode));
                //            if (!string.IsNullOrEmpty(result))
                //            {
                //                state.Latitude = res[Index].Position.Latitude;
                //                state.Longitude = res[Index].Position.Longitude;
                //            }
                //        }
                //    }
                //    if (double.IsNaN(state.Latitude) && double.IsNaN(state.Longitude))
                //    {
                //        string result = Array.Find(extendedZipCode, ele => ele.Contains(state.zipCode));
                //        if (!string.IsNullOrEmpty(result))
                //        {
                //            state.Latitude = res[Index].Position.Latitude;
                //            state.Longitude = res[Index].Position.Longitude;
                //        }
                //        else
                //        {
                //            //var t = Regex.Replace(state.zipCode, @"[-\s]", " ");
                //            int resultIndex = qryRes.FindIndex(x => x.Address.FreeformAddress.Contains(state.IsZipWithSpace ? Regex.Replace(state.zipCode, @"[-\s]", " ") : state.zipCode));
                //            if (resultIndex >= 0)
                //            {
                //                state.Latitude = res[resultIndex].Position.Latitude;
                //                state.Longitude = res[resultIndex].Position.Longitude;
                //            }
                //        }
                //    }
                //}
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
