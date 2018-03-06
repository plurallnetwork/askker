using Askker.App.iOS;
using Askker.App.PortableLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class CommentService
    {
        public async Task<HttpResponseMessage> CreateSurveyComment(SurveyCommentModel surveyCommentModel, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new StringContent(JsonConvert.SerializeObject(surveyCommentModel, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.PostAsync(EnvironmentConstants.getServerUrl() + "api/survey/createsurveycomment", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> DeleteSurveyComment(SurveyCommentModel surveyCommentModel, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new StringContent(JsonConvert.SerializeObject(surveyCommentModel, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.PostAsync(EnvironmentConstants.getServerUrl() + "api/survey/deletesurveycomment", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetSurveyComments(string surveyId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/getsurveycomments/{0}", surveyId));

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
