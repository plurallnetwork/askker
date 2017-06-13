using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class VoteManager
    {
        public async Task Vote(SurveyVoteModel surveyVoteModel, string authenticationToken)
        {
            try
            {
                VoteService voteService = new VoteService();

                surveyVoteModel.voteDate = "";
                surveyVoteModel.active = 1;

                var response = await voteService.Vote(surveyVoteModel, authenticationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteVote(string surveyId, string userId, string authenticationToken)
        {
            try
            {
                VoteService voteService = new VoteService();

                var response = await voteService.DeleteVote(surveyId, userId, authenticationToken);

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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
