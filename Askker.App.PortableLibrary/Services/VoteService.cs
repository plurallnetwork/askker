using Askker.App.PortableLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Askker.App.PortableLibrary.Services
{
    public class VoteService
    {
        public async Task<HttpResponseMessage> Vote(SurveyVoteModel surveyVoteModel, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    List<SurveyVoteModel> surveyVotes = new List<SurveyVoteModel>();
                    surveyVotes.Add(surveyVoteModel);

                    var formContent = new StringContent(JsonConvert.SerializeObject(surveyVotes, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

                    return await client.PostAsync("https://askker.io:44322/api/survey/vote", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> DeleteVote(string surveyId, string userId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format("https://askker.io:44322/api/survey/deletevote/{0}/{1}", surveyId, userId));

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
