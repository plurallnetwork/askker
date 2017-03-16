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
        public async Task<HttpResponseMessage> Vote(VoteModel voteModel, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    List<VoteModel> votes = new List<VoteModel>();
                    votes.Add(voteModel);

                    var formContent = new StringContent(JsonConvert.SerializeObject(votes), Encoding.UTF8, "application/json");

                    return await client.PostAsync("https://blinq-development.com:44322/api/survey/vote", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
