using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NewsAPI.Models;
using System.Collections.Generic;
using System.Threading;

namespace NewsClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://127.0.0.1/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                 HttpResponseMessage response = await client.GetAsync("api/users/1");
                if (response.IsSuccessStatusCode)
                {
                    User user = await response.Content.ReadAsAsync<User>();
                    Console.WriteLine("{0}", user.Name);
                }

                var gizmo = new User() { Name = "Gizmo" };
                response = await client.PostAsJsonAsync("api/users", gizmo);
                if (response.IsSuccessStatusCode)
                {
                    Uri gizmoUrl = response.Headers.Location;
                    User user = await response.Content.ReadAsAsync<User>();
                    response = await SubscribeToFeed(client, response, user);
                    response = await ReadArticles(client, response, user);
                    // HTTP DELETE
                    response = await client.DeleteAsync(gizmoUrl);
                }
            }
        }

        private static async Task<HttpResponseMessage> SubscribeToFeed(HttpClient client, HttpResponseMessage response, User user)
        {
            Console.WriteLine("{0}", user.Name);
            // HTTP PUT
            user.Feeds.Add(new Feed() { Name = "Slashdot", Url = "http://rss.slashdot.org/Slashdot/slashdot" });
            response = await client.PutAsJsonAsync("api/users/" + user.UserId, user);
            Console.WriteLine("{0}", user.Name);
            return response;
        }

        private static async Task<HttpResponseMessage> ReadArticles(HttpClient client, HttpResponseMessage response, User user)
        {
            Thread.Sleep(10000);
            // HTTP PUT
            response = await client.GetAsync("api/users/" + user.UserId + "/articles");
            if (response.IsSuccessStatusCode)
            {
                IEnumerable<Article> articles = await response.Content.ReadAsAsync<IEnumerable<Article>>();
                foreach (Article article in articles)
                    Console.WriteLine("{0}\t{1}\t{2}", article.ArticleId, article.Title, article.PermLink);
            }
           
            return response;
        }
    }
}
