using System.Net;
using System.Net.Http;

namespace OurGroceriesTizen.Api
{
    public class CookieWebClient : HttpClient
    {
        public HttpClientHandler clientHandler { get; }
        public CookieContainer cookieContainer { get; }

        public CookieWebClient() : this(new CookieContainer(), new HttpClientHandler())
        {
        }

        public CookieWebClient(CookieContainer cookieContainer) : this(cookieContainer, new HttpClientHandler())
        {
        }

        public CookieWebClient(CookieContainer cookieContainer, HttpClientHandler clientHandler) : base(clientHandler)
        {
            this.cookieContainer = cookieContainer;
            this.clientHandler = clientHandler;
            this.clientHandler.CookieContainer = cookieContainer;
        }
    }
}
