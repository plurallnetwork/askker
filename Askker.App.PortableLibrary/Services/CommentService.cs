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

        public async Task<HttpResponseMessage> GetSurveyComments(string surveyId, string userId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/getsurveycomments/{0}/{1}", surveyId, userId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> ReportComment(string userId, string commentId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/ReportComment/{0}/{1}", userId, commentId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.PostAsync(uri, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
