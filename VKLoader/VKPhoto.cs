using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class VKPhoto
    {
        public string id { get; set; }
        public string baseUrl { get; set; }
        public List<VKPhotoResolution> availableResolutions { get; set; }

        public VKPhoto()
        {
            availableResolutions = new List<VKPhotoResolution>();
        }

        public VKPhoto(string id, string baseUrl)
        {
            availableResolutions = new List<VKPhotoResolution>();
        }
        public void addResolution(int w, int h, string url)
        {
            availableResolutions.Add(new VKPhotoResolution(w, h, url));
        }

        public string GetHighestResUrl()
        {
            VKPhotoResolution vkpr = null;
            string url = null;
            int max = 0;
            foreach (var res in availableResolutions)
            {
                max = res.width > max ? res.width : max;
            }

            if (max != 0)
            {
                vkpr = availableResolutions.Where(x => x.width == max).First();
                url = vkpr.url;
                return url;
            }

            return "ERROR";            
        }

    }
}
