using Askker.App.iOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class CrashReportService
    {
        public HttpResponseMessage PostCrashReport(string authenticationToken, string report)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string boundary = "---8d0f01e6b3b5dafaaadaad";
                    var content = new MultipartFormDataContent(boundary);
                    content.Add(new StringContent(report, Encoding.UTF8, "text/plain"), "report");
                    
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return client.PostAsync(EnvironmentConstants.getServerUrl() + "api/Account/CrashReport", content).Result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
