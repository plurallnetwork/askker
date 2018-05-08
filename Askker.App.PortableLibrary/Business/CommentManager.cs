using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class CommentManager
    {
        public async Task<SurveyCommentModel> CreateSurveyComment(SurveyCommentModel surveyCommentModel, string authenticationToken)
        {
            try
            {
                CommentService commentService = new CommentService();

                var response = await commentService.CreateSurveyComment(surveyCommentModel, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SurveyCommentModel>(json);
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteSurveyComment(SurveyCommentModel surveyCommentModel, string authenticationToken)
        {
            try
            {
                CommentService commentService = new CommentService();

                var response = await commentService.DeleteSurveyComment(surveyCommentModel, authenticationToken);

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

        public async Task<List<SurveyCommentModel>> GetSurveyComments(string surveyId, string userId, string authenticationToken)
        {
            try
            {
                CommentService commentService = new CommentService();

                var response = await commentService.GetSurveyComments(surveyId, userId, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<SurveyCommentModel>>(json);
                }
                else
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
