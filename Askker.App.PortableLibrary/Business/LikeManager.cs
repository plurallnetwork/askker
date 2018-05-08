using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class LikeManager
    {
        public async Task Like(SurveyLikeModel surveyLikeModel, string authenticationToken)
        {
            try
            {
                LikeService likeService = new LikeService();

                var response = await likeService.Like(surveyLikeModel, authenticationToken).ConfigureAwait(false);

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

        public async Task Unlike(string surveyId, string userId, string authenticationToken)
        {
            try
            {
                LikeService likeService = new LikeService();

                var response = await likeService.Unlike(surveyId, userId, authenticationToken);

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
