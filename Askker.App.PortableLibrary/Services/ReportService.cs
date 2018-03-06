using Askker.App.iOS;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class ReportService
    {
        public async Task<HttpResponseMessage> GetOverallResults(string userId, string creationDate, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/GetOverallResults/{0}/{1}", userId, creationDate));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetResultsByAge(string userId, string creationDate, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/GetResultsByAge/{0}/{1}", userId, creationDate));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetResultsByGender(string userId, string creationDate, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/GetResultsByGender/{0}/{1}", userId, creationDate));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
