using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace League_Sandbox_Auto_Setup.Util
{
    public class Download
    {
        public static void File(String link, String location, Action<int> downloadProgress, Action finishedDownload)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                wc.DownloadProgressChanged += (_, progress) =>
                {
                    downloadProgress((int)progress.ProgressPercentage);
                };
                wc.DownloadFileCompleted += (_, _2) =>
                {
                    wc.Dispose();
                    finishedDownload();
                };
                wc.DownloadFileAsync(new System.Uri(link), location);
            }
        }
    }
}
