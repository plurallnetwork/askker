﻿using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using Foundation;
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
using UIKit;

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
                    var uri = new Uri(string.Format("https://blinq-development.com:44322/api/survey/getfeed/{0}/{1}", userId, filter));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> SaveSurvey(SurveyModel surveyModel, string authenticationToken, Stream questionImage, List<KeyValuePair<string, byte[]>> optionImages)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string boundary = "---8d0f01e6b3b5dafaaadaad";
                    var content = new MultipartFormDataContent(boundary);
                    content.Add(new StringContent(JsonConvert.SerializeObject(surveyModel), Encoding.UTF8, "application/json"), "model");
                    content.Add(new StreamContent(new MemoryStream()), "questionImg");

                    if (surveyModel.type == SurveyType.Image) {
                        foreach (var img in optionImages)
                        {
                            content.Add(CreateFileContent(img.Value, img.Key));
                        }
                    }

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                
                    return await client.PostAsync("https://blinq-development.com:44322/api/survey", content);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private ByteArrayContent CreateFileContent(byte[] stream, string fileName)
        {
            var fileContent = new ByteArrayContent(stream.ToArray());
            fileContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse("form-data");
            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("name", "optionImage-"+Path.GetFileNameWithoutExtension(fileName)));

            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("fileName", "optionImage-" + Path.GetFileNameWithoutExtension(fileName)));

            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/"+Path.GetExtension(fileName).Replace(".",""));
            return fileContent;
        }


        
    }
}
