using System.Net.Http;
using System.Threading.Tasks;

namespace InstagramCommentBot
{
    internal static class TokenHandler
    {
        public static bool AppIsUpToDate(HttpClient client)
        {
            HttpResponseMessage response = client.GetAsync("https://tokeninstabot.herokuapp.com/token").Result;  // Blocking call! 
            string token = "";
            if (response.IsSuccessStatusCode)
            {
                // Get the response
                token =  response.Content.ReadAsStringAsync().Result;
            }

            if(token == "" || token != "WiBnVdGZ56ClgpeeckyT")
            {
                return false;
            }

            return true;
        }
    }
}