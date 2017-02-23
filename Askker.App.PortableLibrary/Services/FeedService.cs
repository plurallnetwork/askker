using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class FeedService
    {
        public async Task<string> GetSurveys(string userId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44321/api/survey/getsurveys/{0}", userId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    var postResponse = await client.GetAsync(uri);

                    return await postResponse.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
