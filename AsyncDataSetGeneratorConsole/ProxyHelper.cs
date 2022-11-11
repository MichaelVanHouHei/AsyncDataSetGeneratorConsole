using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDataSetGeneratorConsole
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Proxy
    {
        public string _id { get; set; }
        public string ip { get; set; }
        public string anonymityLevel { get; set; }
        public string asn { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public DateTime created_at { get; set; }
        public bool google { get; set; }
        public string isp { get; set; }
        public int lastChecked { get; set; }
        public double latency { get; set; }
        public string org { get; set; }
        public string port { get; set; }
        public List<string> protocols { get; set; }
        public object region { get; set; }
        public int responseTime { get; set; }
        public int speed { get; set; }
        public DateTime updated_at { get; set; }
        public object workingPercent { get; set; }
        public double upTime { get; set; }
        public int upTimeSuccessCount { get; set; }
        public int upTimeTryCount { get; set; }
    }

    public class ProxyResponse
    {
        public List<Proxy> data { get; set; }
        public int total { get; set; }
        public int page { get; set; }
        public int limit { get; set; }
    }


    public class ProxyHelper
    {
        List<Proxy> proxies = new List<Proxy>();
        int COUNTER = -1;
        public ProxyHelper()
        {
            getProxyList().GetAwaiter().GetResult();
        }
        private  async Task getProxyList()
        {
            var proxyJson = await "https://proxylist.geonode.com/api/proxy-list?limit=500&page=1&sort_by=lastChecked&sort_type=desc&protocols=http%2Chttps"
                .GetJsonAsync<ProxyResponse>();
            proxies= proxyJson.data;
        }
        public Proxy getProxy()
        {
            Interlocked.Increment(ref COUNTER);
            if (COUNTER >= proxies.Count) COUNTER = 0;
            return proxies[COUNTER];
        }
    }
}
