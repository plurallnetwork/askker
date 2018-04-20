namespace Askker.App.iOS
{
    public static class EnvironmentConstants
    {
        //SET THIS VARIABLE TO THE ENVIRONMENT YOU WANT
        private static bool isDesenv = false;

        private static string serverUrl;
        private static string s3Url;

        public static string getServerUrl(){
            if (isDesenv)
            {
                serverUrl = "https://blinq-development.com:44322/";
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
                s3Url = "https://s3-us-west-2.amazonaws.com/askker-desenv/";
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
    }
}
