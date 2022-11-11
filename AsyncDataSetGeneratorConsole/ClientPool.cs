using Flurl;
using Flurl.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace AsyncDataSetGeneratorConsole
{
    /// <summary>
    /// Static class that manages cached IFlurlClient instances
    /// </summary>
    public static class ClientPool
    {
        /// <summary>
        /// Cache for the clients
        /// </summary>
        private static readonly ConcurrentDictionary<string, IFlurlClient> Clients =
            new ConcurrentDictionary<string, IFlurlClient>();

        /// <summary>
        /// Gets a cached client for the host associated to the input URL
        /// </summary>
        /// <param name="url"><see cref="Url"/> or <see cref="string"/></param>
        /// <returns>A cached <see cref="FlurlClient"/> instance for the host</returns>
        public static IFlurlClient GetClient(Url url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            return PerHostClientFromCache(url);
        }

        /// <summary>
        /// Gets a cached client with a proxy attached to it
        /// </summary>
        /// <returns>A cached <see cref="FlurlClient"/> instance with a proxy</returns>
        public static IFlurlClient GetProxiedClient()
        {
            //string proxyUrl = ;

            return ProxiedClientFromCache(ChooseProxy());
        }
        static  ProxyHelper proxyHelper = new ProxyHelper();
        private static WebProxy ChooseProxy()
        {
            // Do something and return a proxy URL
            var p = proxyHelper.getProxy();
            Console.WriteLine($"client:{p.ip}:{p.port}");
            //     return new WebProxy( p.ip, int.Parse(p.port));
            return new WebProxy($"{p.protocols.First(x=>x.Contains("http"))}://{p.ip}:{p.port}");
        }

        private static IFlurlClient PerHostClientFromCache(Url url) => Clients.AddOrUpdate(
                key: url.ToUri().Host,
                addValueFactory: u =>
                {
                    return CreateClient();
                },
                updateValueFactory: (u, client) =>
                {
                    return client.IsDisposed ? CreateClient() : client;
                }
            );

        private static IFlurlClient ProxiedClientFromCache(WebProxy proxyUrl)
        {
            return Clients.AddOrUpdate(
                key: proxyUrl.Address.ToString(),
                addValueFactory: u => {
                    return CreateProxiedClient(proxyUrl);
                },
                updateValueFactory: (u, client) => {
                    return client.IsDisposed ? CreateProxiedClient(proxyUrl) : client;
                }
            );
        }

        private static IFlurlClient CreateProxiedClient(WebProxy proxyUrl)
        {
            HttpMessageHandler handler = new HttpClientHandler()
            {
               // Proxy = new WebProxy(proxyUrl),
               Proxy = proxyUrl,
                UseProxy = true,
                
          //      ConnaseTimeout = TimeSpan.FromMinutes(10)
            };

            HttpClient client = new HttpClient(handler);

            return new FlurlClient(client);
        }

        private static IFlurlClient CreateClient()
        {
            HttpMessageHandler handler = new HttpClientHandler()
            {
              //  PooledConnectionLifetime = TimeSpan.FromMinutes(10)
            };
           /// ServicePointManager. = TimeSpan.FromMinutes(10);

            HttpClient client = new HttpClient(handler);

            return new FlurlClient(client);
        }
    }
}
