using Askker.App.PortableLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class CrashReportManager
    {
        public CrashReportService crashReportService { get; set; }

        public CrashReportManager()
        {
            crashReportService = new CrashReportService();
        }

        public void PostCrashReport(string authenticationToken, string report)
        {
            var response = crashReportService.PostCrashReport(authenticationToken, report);
            var json = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Unauthorized");
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
        }
    }
}
