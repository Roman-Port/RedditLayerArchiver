using Newtonsoft.Json;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace RedditLayerArchiver.Tools
{
    public static class WebTools
    {
        private static string cachedToken;
        public static DateTime cachedTokenExpire;

        /// <summary>
        /// Unfortunately, we cannot obtain a token acceptable by Reddit under normal means. We have to use this janky workaround.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> ObtainToken()
        {
            //Check if we even need to refresh
            if (DateTime.UtcNow.AddMinutes(10) < cachedTokenExpire && cachedToken != null)
                return cachedToken;

            //We need to request the Reddit homepage
            string homepage = await Program.client.GetStringAsync("https://reddit.com/");

            //Now, parse out the page
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(homepage);

            //Get the element with the data
            var ele = doc.GetElementbyId("data");
            string payload = ele.InnerHtml.Substring("window.___r = ".Length);
            var data = JsonConvert.DeserializeObject<RedditPage>(payload, new JsonSerializerSettings
            {
                CheckAdditionalContent = false
            }); //This skips the data after our payload data

            //Now, get the token and expire time
            cachedToken = data.user.session.accessToken;
            cachedTokenExpire = data.user.session.expires;

            //Return token
            Console.WriteLine("[Token Update] Now using token " + cachedToken);
            return cachedToken;
        }

        public static void DevalidateToken()
        {
            cachedToken = null;
        }

        public static async Task<T> GetJsonAsync<T>(string url)
        {
            Program.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await ObtainToken());
            var response = await Program.client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Got invalid status code {response.StatusCode.ToString()} to GET {url}.");
            var stream = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stream);
        }

        public static async Task<T> PostJsonAsync<T>(string url, object request)
        {
            Program.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await ObtainToken());
            string content = JsonConvert.SerializeObject(request);
            var scontent = new StringContent(content);
            scontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await Program.client.PostAsync(url, scontent);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Got invalid status code {response.StatusCode.ToString()} to POST {url}. Content: {content}.");
            var stream = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stream);
        }

        class RedditToken
        {
            public string accessToken;
            public DateTime expires;
            public int expiresIn;
            public bool unsafeLoggedOut;
            public bool safe;
        }

        class RedditUser
        {
            public RedditToken session;
        }

        class RedditPage
        {
            public RedditUser user;
        }
    }
}
