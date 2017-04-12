using Askker.App.PortableLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class LoginService
    {
        public async Task<HttpResponseMessage> GetAuthorizationToken(UserLoginModel userLoginModel)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", userLoginModel.Username),
                        new KeyValuePair<string, string>("password", userLoginModel.Password)
                    });

                    return await client.PostAsync("https://blinq-development.com:44322/Token", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetUserById(string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.GetAsync("https://blinq-development.com:44322/api/Account/GetUserById");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetUserById(string authenticationToken, string userId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format("https://blinq-development.com:44322/api/Account/GetUserById/{0}", userId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> SearchUsersByName(string authenticationToken, string name)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format("https://blinq-development.com:44322/api/Account/SearchUsersByName/{0}", name));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> Update(string authenticationToken, UserModel userModel, byte[] profileImage, string profileImageName)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string boundary = "---8d0f01e6b3b5dafaaadaad";
                    var content = new MultipartFormDataContent(boundary);
                    content.Add(new StringContent(JsonConvert.SerializeObject(userModel), Encoding.UTF8, "application/json"), "model");
                    if (!string.IsNullOrEmpty(profileImageName))
                    {
                        content.Add(CreateFileContent(profileImage, profileImageName));
                    }                    

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.PostAsync("https://blinq-development.com:44322/api/Account/Update", content);
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
            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("name", Path.GetFileNameWithoutExtension(fileName)));

            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("fileName", Path.GetFileNameWithoutExtension(fileName)));

            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/" + Path.GetExtension(fileName).Replace(".", ""));
            return fileContent;
        }
    }
}
