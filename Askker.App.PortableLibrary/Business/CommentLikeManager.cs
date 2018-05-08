using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class CommentLikeManager
    {
        public async Task LikeComment(SurveyCommentLikeModel surveyCommentLikeModel, string authenticationToken)
        {
            try
            {
                CommentLikeService commentLikeService = new CommentLikeService();

                var response = await commentLikeService.LikeComment(surveyCommentLikeModel, authenticationToken).ConfigureAwait(false);

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

        public async Task UnlikeComment(string commentId, string userId, string authenticationToken)
        {
            try
            {
                CommentLikeService commentLikeService = new CommentLikeService();

                var response = await commentLikeService.UnlikeComment(commentId, userId, authenticationToken);

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
