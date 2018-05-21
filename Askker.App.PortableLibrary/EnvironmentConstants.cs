using System;
using System.Net.Http;

namespace Askker.App.iOS
{
    public static class EnvironmentConstants
    {
        //SET THIS VARIABLE TO THE ENVIRONMENT YOU WANT
        private static bool isDesenv = true;

        private static string serverUrl;
        private static string s3Url;

        public static string getServerUrl(){
            if (isDesenv)
            {
                serverUrl = "https://blinq-development.com:44325/";
            }
            else
            {
                serverUrl = "https://askker.io:44322/";
            }
            return serverUrl;            
        }

        public static string getS3Url(){
            if (isDesenv)
            {
                s3Url = "https://s3-us-west-2.amazonaws.com/workdone-desenv/";
            }
            else
            {
                s3Url = "https://s3-us-west-2.amazonaws.com/askker-prod/";
            }
            return s3Url;            
        }

        public static string getServerTimeZone()
        {
            return "America/Sao_Paulo";
        }

        public static DateTime getServerDateTime()
        {
            try
            {
                HttpResponseMessage response;
                using (var client = new HttpClient())
                {
                    response = client.GetAsync(getServerUrl() + "api/survey/GetServerDatetime").Result;
                }

                var stringDatetime = response.Content.ReadAsStringAsync().Result;

                return DateTime.ParseExact(stringDatetime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
    }
}
