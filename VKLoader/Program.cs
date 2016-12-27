#region usingDirectives
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;


#endregion usingDirectives

/*
    Copyright (c) 2016 Kenneth Luján R.

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
    to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
    and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


    For further advice contact me at : klujanrosas@gmail.com
*/

namespace VKLoader
{
    class Program
    {
        static List<VKPhoto> photos = new List<VKPhoto>();
        static List<string> downloadQueue = new List<string>();
        static WebClient wc = new WebClient();
        static string version = "v1.0";
        static string albumUrl;
        static int offset = 0;
        static int offsetSize = 40;
        static int albumPhotoCount = 0;
        static NameValueCollection offsetBasedRequestData = new NameValueCollection
            {
                { "al","1" },
                { "al_ad", "0" },
                { "offset",offset.ToString() }, // 40 - 80 - 120 - ... - 520
                { "part","1" },
                { "rev","" }
            };

        
        
        static void Main(string[] args)
        {
            Console.Title = $"VKLoader {version}";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"VKLoader {version} by hexc0der");
            Console.WriteLine("Download all images from an album hosted on VK in one go!");
            Console.WriteLine("If you liked this program or have any doubts contact me at https://github.com/hexc0der");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            do
            {
                Console.WriteLine("Enter a valid VK Album url i.e http://vk.com/album-22382023_240601069 : ");
                albumUrl = Console.ReadLine();
            } while (!IsValidUrl(albumUrl));


            var albumIDFromUrl = albumUrl.Substring(albumUrl.LastIndexOf('/')+1);

            var tempRequestData = new NameValueCollection
            {
                { "act", "show" },
                { "al", "1" },
                { "al_ad", "0" },
                { "list", albumIDFromUrl}, 
                { "module", "photos"},
                { "photo", ""}, //encontramos cuantas fotos hay en el album 7u7
            };

            albumPhotoCount = GetAlbumPhotoCount(wc, tempRequestData);


            for (offset=0; offset <= albumPhotoCount; offset+=offsetSize)
            {
                Console.WriteLine($"Getting images in offset {offset}");
                offsetBasedRequestData = new NameValueCollection
                {
                    { "al","1" },
                    { "al_ad", "0" },
                    { "offset",offset.ToString() }, // 40 - 80 - 120 - ... - 520
                    { "part","1" },
                    { "rev","" }
                };
                GetAlbumInfo(offsetBasedRequestData);
            }

            //

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{downloadQueue.Count} images have been found in this album.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;


            //After getting images for all offsets we start getting info for each img and available res

            foreach (var item in downloadQueue)
            {

                VKPhoto asd = GetVKPhoto(wc, albumIDFromUrl, item);
                photos.Add(asd);
            }

            //Once we finally have all info, we start downloading uwu

            Console.WriteLine("Downloading images now");
            using (var progress = new ProgressBar())
            {
                for(int i = 0;i<photos.Count;i++)
                {
                
                    DownloadFromUrl(wc, photos[i].GetHighestResUrl(), photos[i].id);
                    progress.Report((double)i / 100);
                    
                }
            }

            
                exit("Download Complete.");
        }

        public static int GetAlbumPhotoCount(WebClient wc, NameValueCollection tempRequestData)
        {
            byte[] response = wc.UploadValues("http://vk.com/al_photos.php", "POST", tempRequestData);
            string decodedResponse = Encoding.UTF8.GetString(response);

            

            int indexFrom = decodedResponse.IndexOf("<!int>") + "<!int>".Length;
            int indexTo = decodedResponse.NthIndexOf("<!>", 7);

            return int.Parse(decodedResponse.Substring(indexFrom, indexTo - indexFrom));
        }
        
        private static void exit(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void GetAlbumInfo(NameValueCollection requestData)
        {
            byte[] response = wc.UploadValues(albumUrl, "POST", requestData);
            string plainHtml = wc.DownloadString(albumUrl);
            string decodedResponse = Encoding.UTF8.GetString(response);

            MatchCollection matches = Regex.Matches(decodedResponse, @"(showPhoto)\(([^\)]+)\)");
            foreach (Match match in matches)
            {
                foreach (Capture capture in match.Captures)
                {


                    int indexFrom = capture.Value.IndexOf("'-") + "'-".Length;
                    int indexTo = capture.Value.IndexOf("',");

                    string actualValue = "-" + capture.Value.Substring(indexFrom, indexTo - indexFrom);
                    //Console.WriteLine("Index={0}, Value={1}", capture.Index, actualValue);
                    //Console.WriteLine("Index={0}, Value={1}", capture.Index, capture.Value);
                    downloadQueue.Add(actualValue);
                }
            }
        }
        public static void DownloadFromUrl(WebClient wc, string url, string imgID)
        {
            using (wc)
            {

                byte[] data = wc.DownloadData(url);

                using (MemoryStream mem = new MemoryStream(data))
                {
                    using (var yourImage = Image.FromStream(mem))
                    {

                        yourImage.Save($"{imgID.Replace('-',' ')}.jpg", ImageFormat.Jpeg);
                        //Console.WriteLine($"Se guardo { imgID.Replace('-', ' ')}.jpg");
                    }
                }

            }
        }

        public static bool IsValidUrl(string url)
        {
            return Regex.IsMatch(url, @"(http|https)(:\/\/vk.com\/album-)\d{8,9}_\d{1,9}");
        }

        public static VKPhoto GetVKPhoto(WebClient wc, string albumID, string photoID)
        {
            var requestData = new NameValueCollection
            {
                { "act", "show" },
                { "al", "1" },
                { "al_ad", "0" },
                { "list", albumID}, //unico para un album
                { "module", "photos"},
                { "photo", photoID}, //cada foto 
            };




            byte[] response = wc.UploadValues("http://vk.com/al_photos.php", "POST", requestData);
            string decodedResponse = Encoding.UTF8.GetString(response);

            var idx = decodedResponse.IndexOf("<!json>");

            int indexFrom = decodedResponse.IndexOf("<!json>") + "<!json>".Length;
            int indexTo = decodedResponse.NthIndexOf("<!>", 9);

            string jsonData = decodedResponse.Substring(indexFrom, indexTo - indexFrom);

            dynamic dyn = JsonConvert.DeserializeObject(jsonData);

            VKPhoto photo = new VKPhoto();

            foreach (var item in dyn)
            {
                if (item.id.ToString() == photoID)
                {
                    photo.id = item.id;
                    photo.baseUrl = item.author_href.ToString();


                    //ikr ._. but it works :)

                    if (item.x_ != null && item.x_src != null)
                    {
                        photo.addResolution(int.Parse(item.x_[1].ToString()), int.Parse(item.x_[2].ToString()), item.x_src.ToString());
                    }
                    if (item.y_ != null && item.y_src != null)
                    {
                        photo.addResolution(int.Parse(item.y_[1].ToString()), int.Parse(item.y_[2].ToString()), item.y_src.ToString());
                    }
                    if (item.z_ != null && item.z_src != null)
                    {
                        photo.addResolution(int.Parse(item.z_[1].ToString()), int.Parse(item.z_[2].ToString()), item.z_src.ToString());
                    }
                    if (item.w_ != null && item.w_src != null)
                    {
                        photo.addResolution(int.Parse(item.w_[1].ToString()), int.Parse(item.w_[2].ToString()), item.w_src.ToString());
                    }
                    if (item.o_ != null && item.o_src != null)
                    {
                        photo.addResolution(int.Parse(item.o_[1].ToString()), int.Parse(item.o_[2].ToString()), item.o_src.ToString());
                    }
                    if (item.p_ != null && item.p_src != null)
                    {
                        photo.addResolution(int.Parse(item.p_[1].ToString()), int.Parse(item.p_[2].ToString()), item.p_src.ToString());
                    }
                    if (item.q_ != null && item.q_src != null)
                    {
                        photo.addResolution(int.Parse(item.q_[1].ToString()), int.Parse(item.q_[2].ToString()), item.q_src.ToString());
                    }
                    if (item.r_ != null && item.r_src != null)
                    {
                        photo.addResolution(int.Parse(item.r_[1].ToString()), int.Parse(item.r_[2].ToString()), item.r_src.ToString());

                        //Console.WriteLine(item.z_src.ToString()); Console.WriteLine(item.z_[0].ToString());
                    }

                }
            }

            

            return photo;
        }
        
    }

    public static class StringExtender
    {
        public static int NthIndexOf(this string target, string value, int n)
        {
            Match m = Regex.Match(target, "((" + Regex.Escape(value) + ").*?){" + n + "}");

            if (m.Success)
                return m.Groups[2].Captures[n - 1].Index;
            else
                return -1;
        }
    }
}
