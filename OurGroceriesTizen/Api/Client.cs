using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OurGroceries.Api;
using OurGroceries.Api.Entities;
using Tizen;
using Tizen.Applications;
using Tizen.Network.Connection;

namespace OurGroceriesTizen.Api
{
    public class Client : IDisposable
    {
        private const string BaseUrl = "https://www.ourgroceries.com";
        private readonly string _cookieFile = Path.Combine(Application.Current.DirectoryInfo.Cache, "cookies.dat");

        public Client()
        {}

        ~Client()
        {            
            webClient = null;
        }

        public CookieWebClient webClient { get; private set; }

        public void OnCreate()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            try
            {
                webClient = new CookieWebClient(ReadCookiesFromDisk(_cookieFile));

                try
                {
                    var proxy = new WebProxy(ConnectionManager.GetProxy(AddressFamily.IPv4));
                    webClient.clientHandler.Proxy = proxy;
                }
                catch (Exception e)
                {
                    Log.Error("OurGroceries", $"Error creating proxy: {e.Message}");
                }

                webClient.DefaultRequestHeaders.Add("Host", "www.ourgroceries.com");
                webClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                    {
                        CharSet = Encoding.UTF8.WebName
                    });
                webClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
                webClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                webClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
                webClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                webClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type",
                    "application/x-www-form-urlencoded");
            }
            catch (Exception e)
            {
                Log.Error("OurGroceries", e.Message + " " + e.StackTrace);
            }
        }

        public bool IsSignedIn()
        {
            var cookies = webClient.cookieContainer.GetCookies(new Uri(BaseUrl));

            return !(cookies.Count == 0 || cookies[0].Expired);
        }

        public async Task<string> SignIn(string emailAddress, string password)
        {
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("emailAddress", emailAddress),
                new KeyValuePair<string, string>("action", "sign-me-in"),
                new KeyValuePair<string, string>("password", password)
            };

            var url = $"{BaseUrl}/sign-in";

            var result = await webClient.PostAsync(url, new FormUrlEncodedContent(values));

            if (!IsSignedIn())
            {
                throw new NotSignedInException();
            }

            var resultContent = await result.Content.ReadAsStringAsync();

            var shoppingTeam = CreateShoppingTeam(resultContent);

            var shoppingTeamJson = JsonConvert.SerializeObject(shoppingTeam);

            return shoppingTeamJson;
        }

        public async Task<string> LoadLists()
        {
            var url = $"{BaseUrl}/your-lists";

            var result = await webClient.GetAsync(url);

            var resultContent = await result.Content.ReadAsStringAsync();

            var shoppingTeam = CreateShoppingTeam(resultContent);

            var shoppingTeamJson = JsonConvert.SerializeObject(shoppingTeam);

            return shoppingTeamJson;
        }

        public async Task<string> SetItemCrossedOff(string listId, string teamId, string itemId, bool crossedOff)
        {
            var message = new OurGroceriesMessageUpdate
            {
                command = "setItemCrossedOff",
                listId = listId,
                teamId = teamId,
                itemId = itemId,
                versionId = string.Empty,
                crossedOff = crossedOff
            };


            var result = await PostMessage2OurGroceries(listId, message);
            
            return result;
        }

        public async Task<ListItems> GetList(string listId, string teamId)
        {
            var message = new OurGroceriesMessage
            {
                command = "getList", 
                listId = listId, 
                teamId = teamId, 
                versionId = string.Empty
            };

            var result = await PostMessage2OurGroceries(listId, message);
            
            var listItemsJson = JObject.Parse(result)["list"]?.ToString();
            var listItems =
                JsonConvert.DeserializeObject<ListItems>(listItemsJson ?? string.Empty);

            return listItems;
        }

        public async Task<bool> isCrossedOff(string listId, string teamId, string itemId)
        {
            var list = await GetList(listId, teamId);
            var item = list.items.ToList().Find(o => o.id == itemId);

            return item != null && item.crossedOff;
        }
        
        private async Task<string> PostMessage2OurGroceries<T>(string listId, T message)
        {
            var serializedMessage = JsonConvert.SerializeObject(message);
            
            var url = $"{BaseUrl}/your-lists/list/{listId}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(serializedMessage)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = Encoding.UTF8.WebName
            };

            var result = await webClient.SendAsync(request);

            return await result.Content.ReadAsStringAsync();
        }

        private static ShoppingTeam CreateShoppingTeam(string content2Parse)
        {
            var shoppingTeam = new ShoppingTeam();

            if (string.IsNullOrEmpty(content2Parse)) return shoppingTeam;
            //parse out teamId
            var matchedTeamId = Regex.Match(content2Parse, @"var\sg_teamId\s=\s""(?<teamId>[a-zA-Z0-9]+)""");

            if (matchedTeamId.Success)
            {
                shoppingTeam.teamId = matchedTeamId.Groups["teamId"].Value;
            }

            //parse out ShoppingList
            var matchedShoppingLists = Regex.Matches(content2Parse, @"id:\s""(?<id>\w+)"",\sname:\s""(?<name>[\s\\'a-zA-Z0-9]+)""");
            var shoppingList = new List<ShoppingList>();

            if(matchedShoppingLists.Count > 0)
            {
                foreach(Match matchedShoppingList in matchedShoppingLists)
                {
                    if(matchedShoppingList.Success)
                    {
                        var id = matchedShoppingList.Groups["id"].Value;
                        var name = matchedShoppingList.Groups["name"].Value;

                        shoppingList.Add(new ShoppingList() { id = id, name = name });
                    }
                }
            }

            shoppingTeam.shoppingLists = shoppingList;

            return shoppingTeam;
        }

        public void Dispose()
        {
            webClient.Dispose();
        }

        public void OnSleep()
        {
            WriteCookiesToDisk(_cookieFile, webClient.cookieContainer);
        }

        private static void WriteCookiesToDisk(string file, CookieContainer cookieJar)
        {
            using (Stream stream = File.Create(file))
            {
                try
                {
                    Console.Out.Write("Writing cookies to disk... ");
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cookieJar);
                    Console.Out.WriteLine("Done.");
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine("Problem writing cookies to disk: " + e.GetType());
                }
            }
        }

        private static CookieContainer ReadCookiesFromDisk(string file)
        {

            try
            {
                using (Stream stream = File.Open(file, FileMode.Open))
                {
                    Console.Out.Write("Reading cookies from disk... ");
                    BinaryFormatter formatter = new BinaryFormatter();
                    Console.Out.WriteLine("Done.");
                    return (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Problem reading cookies from disk: " + e.GetType());
                return new CookieContainer();
            }
        }

        public class NotSignedInException : Exception
        {
        }
    }
}
