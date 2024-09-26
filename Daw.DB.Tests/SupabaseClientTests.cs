using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Daw.DB.Tests
{
    [TestClass]
    public class SupabaseClientTests
    {
        private string _supabaseUrl;
        private string _supabaseKey;

        [TestInitialize]
        public void Setup()
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _supabaseUrl = configuration["Supabase:Url"];
            _supabaseKey = configuration["Supabase:Key"];
        }

        [TestMethod]
        public async Task TestSupabaseClientConnection()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_supabaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _supabaseKey);
                client.DefaultRequestHeaders.Add("apikey", _supabaseKey);

                // Make a request to the Supabase REST API
                var response = await client.GetAsync("/rest/v1/names?select=*");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonArray = JArray.Parse(content);

                Assert.IsNotNull(jsonArray);
                Assert.IsTrue(jsonArray.Count > 0, "No records returned from the table.");

                foreach (var item in jsonArray)
                {
                    var id = item["id"];
                    var name = item["name"];
                    Assert.IsNotNull(name);
                }
            }
        }
    }
}
