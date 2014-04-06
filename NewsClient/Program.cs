using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NewsAPI.Models;

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
                client.BaseAddress = new Uri("http://localhost:26118/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                 HttpResponseMessage response = await client.GetAsync("api/users/27");
                if (response.IsSuccessStatusCode)
                {
                    User user = await response.Content.ReadAsAsync<User>();
                    Console.WriteLine("{0}", user.Name);
                }
            }
        }
    }
}
