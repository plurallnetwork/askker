using Askker.App.PortableLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class FeedService
    {
        public async Task<HttpResponseMessage> GetFeed(string userId, string filter, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44322/api/survey/getfeed/{0}/{1}", userId, filter));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> SaveSurvey(SurveyModel surveyModel, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(JsonConvert.SerializeObject(surveyModel), Encoding.UTF8, "application/json"), "model");
                    content.Add(new StreamContent(new MemoryStream()), "questionImg");
                    content.Add(new StreamContent(new MemoryStream()), "optionImgs");

                    string teste = JsonConvert.SerializeObject(surveyModel);
                    var formContent = new StringContent(JsonConvert.SerializeObject(surveyModel), Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.PostAsync("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44322/api/survey", content);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
