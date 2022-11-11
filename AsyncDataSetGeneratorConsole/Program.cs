using AngleSharp.Html.Parser;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDataSetGeneratorConsole
{
    internal class Program
    {
        private const string ROOT_URL = "https://fakecaptcha.com";
        static string FILE_LOC = Environment.CurrentDirectory + @"\data\";
      //  static CookieSession cookieSession = new CookieSession(ROOT_URL);
        static async Task GenerateCap(string word)
        {
            // try
            //{
            var client = ClientPool.GetProxiedClient();
              
                var generateResponse = await (await client.Request(ROOT_URL+"/generate.php")
                    /*  .WithHeaders(new
                    //  {
                    //     Connection = "keep-alive",
                    //     User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36",

                      })*/
                    .AllowAnyHttpStatus()
                    .WithTimeout(60)
                    .PostUrlEncodedAsync($"words={word}&force=0&color=red")).GetStringAsync();
            Console.WriteLine($"-------------------generate word:{word}-------------------");
            if (generateResponse.Contains("Please wait while we build your Captch")) //lazy here ,should not be check list that
                {
                    
                    Console.WriteLine($"request generate.php");
                    var pareser = await new HtmlParser().ParseDocumentAsync(generateResponse);
                    var wordToken = pareser.QuerySelector("#submit_form > input[type=hidden]").GetAttribute("value");
                    Console.WriteLine($"word token {wordToken}");
                    var resultResponse = await (await client.Request(ROOT_URL+"/result.php").AllowAnyHttpStatus().PostUrlEncodedAsync($"words={wordToken}")).GetStringAsync();
                    pareser = await new HtmlParser().ParseDocumentAsync(resultResponse);
                    var imageBase64 = pareser.QuerySelector("#words").GetAttribute("src").Replace("data:image/jpg;base64,", "");
                    Console.WriteLine($"result.php base64 {imageBase64}");
                    byte[] imageBtyes = Convert.FromBase64String(imageBase64);
                    var filePath = Environment.CurrentDirectory + @"\data\" + word + ".jpg";
                  
                    using (FileStream sourceStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                           duplicated.Add(word);
                          Console.WriteLine($"saving image:{filePath}");
                        await sourceStream.WriteAsync(imageBtyes, 0, imageBtyes.Length);
                        Console.WriteLine("--------------------------------------------------------------");
                    }
                }
            //}
            //catch
            //{
            //    Console.WriteLine("rate limited");
            //    //cookieSession = new CookieSession(ROOT_URL);
            //}
        }
        private static Random random = new Random();
        static  string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower();
        static List<string> duplicated = new List<string>();
        public static string RandomString(int length = 4)
        {
            again:
            var st = new string(Enumerable.Repeat(CHARS, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            if (duplicated.Contains(st)) goto again;
         
            return st;
        }
        static async Task MainAsync()
        {
            while(true)
            {
             
                    await Task.WhenAny(Enumerable.Range(0, 500).Select(_ => GenerateCap(RandomString())));
               
                   // Console.WriteLine("rate limited");
                    //cookieSession = new CookieSession(ROOT_URL);
                  //  await Task.Delay(30000);
               

                //not to ddos the website
                await Task.Delay(random.Next(1000 * 60 , 1000 * 60 * 2) );
            }
            Console.Read();
        }
        static void Main(string[] args)
        {
            if (!Directory.Exists(FILE_LOC))
            {
                Directory.CreateDirectory(FILE_LOC);
            }
            duplicated = Directory.EnumerateFiles(FILE_LOC, "*.jpg").Select(p=>  Path.GetFileNameWithoutExtension(p) ).ToList();
            MainAsync().GetAwaiter().GetResult();
        }
    }
}
