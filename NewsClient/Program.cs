using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NewsAPI.Models;
using System.Collections.Generic;
using System.Threading;
using System.Collections;

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
                client.BaseAddress = new Uri("http://newsapi.cloudapp.net/");
                //client.BaseAddress = new Uri("http://127.0.0.1/");
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
                    await SubscribeToFeed(client, user);
                    
                    response = await ReadArticles(client, user);
                    // HTTP DELETE
                    foreach (Feed feed in user.Feeds)
                    {
                        response = await client.DeleteAsync("api/feeds/" + feed.FeedId);
                    }
                    response = await client.DeleteAsync("api/users/" + user.UserId);
                }
            }
        }

        private static async Task<HttpResponseMessage> SubscribeToFeed(HttpClient client, User user)
        {
            Console.WriteLine("{0}", user.Name);
            // HTTP PUT
            user.Feeds.Add(new Feed() { Name = "Slashdot", Url = "http://rss.slashdot.org/Slashdot/slashdot" });
            HttpResponseMessage response = await client.PutAsJsonAsync("api/users/" + user.UserId, user);
            User modUser = await response.Content.ReadAsAsync<User>();
            Console.WriteLine("{0}", user.Name);
            user.Feeds.Clear();
            foreach (Feed feed in modUser.Feeds)
            {
                user.Feeds.Add(feed);
            }
            
            return response;
        }

        private static async Task<HttpResponseMessage> ReadArticles(HttpClient client, User user)
        {
            Thread.Sleep(10000);
            // HTTP PUT
            HttpResponseMessage response = await client.GetAsync("api/users/" + user.UserId + "/articles");
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
