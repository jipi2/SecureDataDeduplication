using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.HttpFolder
{
    public class HttpServiceCustom
    {
        static private string _baseUrlForProxy = "https://98.66.191.100:443";
        static private string _baseUrlForApi = "https://98.66.191.100:443";
        static private HttpClient getClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;  // Bypass SSL certificate validation

            HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(30) };
            return client;
        }
        static public HttpClient GetProxyClient()
        {
            HttpClient client = getClient();
            client.BaseAddress = new Uri(_baseUrlForProxy);
            return client;
        }

        static public HttpClient GetProxyClient(string jwt)
        {
            HttpClient client = getClient();
            client.BaseAddress = new Uri(_baseUrlForProxy);
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
            return client;
        }

        static public HttpClient GetApiClient()
        {
            HttpClient client = getClient();
            client.BaseAddress = new Uri(_baseUrlForApi);
            return client;
        }

        static public HttpClient GetApiClient(string jwt)
        {
            HttpClient client = getClient();
            client.BaseAddress = new Uri(_baseUrlForApi);
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
            return client;
        }
    }
}
