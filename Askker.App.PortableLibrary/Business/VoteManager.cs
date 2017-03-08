using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class VoteManager
    {
        public async Task Vote(VoteModel voteModel, string authenticationToken)
        {
            try
            {
                VoteService voteService = new VoteService();

                voteModel.voteDate = "";
                voteModel.active = 1;

                var response = await voteService.Vote(voteModel, authenticationToken).ConfigureAwait(false);

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
    }
}
