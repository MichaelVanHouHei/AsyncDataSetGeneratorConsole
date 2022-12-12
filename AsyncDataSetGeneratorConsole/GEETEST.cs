using Flurl.Http;
using Hazdryx.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Geetest_UMAC
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += async (o, e) =>
             {
                 await Refresh();
             };
        }
        

        async Task Refresh()
        {
            //var geetest = new GeetestRequester();
            //var gStruct = await geetest.getRegAsync();
            //await geetest.getImagesLinks(gStruct);
            GeetestImage gImage = new GeetestImage();
            await  gImage.CalDistance();
        }
    }
    public class GeetestRequester
    {
        CookieSession cookieSession = new CookieSession();
        /* 
         * https://www.geetest.com/demo/gt/register-slide?t=1668540138432 -> gt , challenge
         */
        public async Task<GResponse> getRegAsync()
        {
            // since t is timestamp , i dont care what number is it , as long as it gives me different gt ,challenge for this
            var response =   await cookieSession.Request($"https://www.geetest.com/demo/gt/register-slide?t={new Random().Next(0,int.MaxValue)}").GetJsonAsync<GResponse>();
            return response;
        }
        public async Task getImagesLinks(GResponse gStruct)
        {
            // without w parameter must return false json 
             var a = await   cookieSession.Request($"https://api.geetest.com/get.php?is_next=true&type=slide3&gt={gStruct.gt}&challenge={gStruct.challenge}&lang=zh-cn&https=true&protocol=https%3A%2F%2F&offline=false&product=embed&api_server=api.geetest.com&isPC=true&autoReset=true&width=100%25").GetStringAsync();
            var b = 0;
        }
    }
    public class GeetestImage
    {
          List<int> LOCATION = new List<int>(){39, 38, 48, 49, 41, 40, 46, 47, 35, 34, 50, 51, 33, 32, 28, 29, 27, 26, 36, 37, 31, 30, 44, 45, 43,
                 42, 12, 13, 23, 22, 14, 15, 21, 20, 8, 9, 25, 24, 6, 7, 3, 2, 0, 1, 11, 10, 4, 5, 19, 18, 16, 17};
         // 52 location
        const int R = 260;
        const int N = 160;
        const int S = N / 2;
        const int U = 10;
        public async Task<int> CalDistance()
        {
            //should change two string parameters to class 
            string bg_url = "https://static.geetest.com/pictures/gt/7bfaaa72b/bg/b51986bee.jpg";
            string full_img_url = "https://static.geetest.com/pictures/gt/7bfaaa72b/7bfaaa72b.jpg";
            var bg = await retoreImageAsync(await bg_url.GetBytesAsync());
            var full_img = await retoreImageAsync(await full_img_url.GetBytesAsync());
            using(FastBitmap fbg = new FastBitmap(bg))
            {
                using(FastBitmap fimg = new FastBitmap(full_img))
                {
                    for(int x = 0; x < bg.Width; x++)
                    {
                        for(int y = 0;y< bg.Height; y++)
                        {
                            System.Drawing.Color c1 = fbg.Get(x, y);
                            System.Drawing.Color c2 = fimg.Get(x, y);
                            var diff_r = Math.Abs(c1.R - c2.R) > 50;
                            var diff_g = Math.Abs(c1.G - c2.G) > 50;
                            var diff_b = Math.Abs(c1.B - c2.B) > 50;
                            if(diff_r && diff_g && diff_b)
                            {
                                return x-3;
                            }
                        }
                    }
                  
                }
            }
            return -1;
        }
        public async Task<Bitmap> retoreImageAsync(byte[] imgBytes)
        {
            Bitmap timg = await Task.Run(() => {
            Bitmap img , newBitmap = new Bitmap(R,N);
            using (var ms = new MemoryStream(imgBytes))
            {
               img = new Bitmap(ms);
            }

            //foreach(var loc in LOCATION)
            //{
            for (int c = 0; c <52; c++)
            {
                var loc = LOCATION[c];
                var f = loc % 26 * 12 + 1;
                var k = loc > 25 ? S : 0;
                var sourceRegion = new System.Drawing.Rectangle(f, k, U, S);
                var destRegion = new System.Drawing.Rectangle(c % 26 * 10, c > 25 ? S:0  , U, S);
                CopyRegionIntoImage(img, sourceRegion, ref newBitmap, destRegion);
            }
                return newBitmap;
            //  newBitmap.Save("test1.jpg");
            });
            return timg;
            //}

        }
        public   void CopyRegionIntoImage(Bitmap srcBitmap, System.Drawing.Rectangle srcRegion, ref Bitmap destBitmap, System.Drawing.Rectangle destRegion)
        {
            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
            }
        }
    }
    public class GResponse
    {
        public int success { get; set; }
        public string challenge { get; set; }
        public string gt { get; set; }
        public bool new_captcha { get; set; }
    }
}
