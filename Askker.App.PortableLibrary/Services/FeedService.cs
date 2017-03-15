using Askker.App.PortableLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class FeedService
    {
        public async Task<HttpResponseMessage> GetSurveys(string userId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44322/api/survey/getsurveys/{0}", userId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> SaveSurvey(SurveyModel surveyModel, string authenticationToken, Stream questionImage, List<KeyValuePair<string, MemoryStream>> optionImages)
        {
            try
            {
                HttpClient client = new HttpClient();
                //using (var client = new HttpClient(handler, false))
                //{
                    string boundary = "---8d0f01e6b3b5dafaaadaad";
                    var content = new MultipartFormDataContent(boundary);
                    content.Add(new StringContent(JsonConvert.SerializeObject(surveyModel), Encoding.UTF8, "application/json"), "model");
                    content.Add(new StreamContent(new MemoryStream()), "questionImg");
                    //content.Add(new StringContent(System.Text.Encoding.UTF8.GetString(optionImages.ElementAt(0).Value.ToArray(), 0 , optionImages.ElementAt(0).Value.ToArray().Length), Encoding.UTF8, "application/octet-stream"), "0");
                    content.Add(CreateFileContent(optionImages.ElementAt(0).Value, optionImages.ElementAt(0).Key));



                    //foreach (var img in optionImages)
                    //{
                    //    content.Add(CreateFileContent(img.Value, img.Key));
                    //    //content.Add(new StreamContent(img.Value), img.Key);
                    //}

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.PostAsync("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44322/api/survey", content);
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private ByteArrayContent CreateFileContent(MemoryStream stream, string fileName)
        {
            //var fileContent = new StreamContent(stream);
            var fileContent = new ByteArrayContent(stream.ToArray());
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"" + fileName + "\"",
                FileName = "\"" + fileName + "\""
            }; // the extra quotes are key here
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            return fileContent;
        }
    }
}
