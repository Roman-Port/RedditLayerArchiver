using RedditLayerArchiver.Responses.GQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedditLayerArchiver.Tools
{
    public class ImageDownloader
    {
        public static HttpClient wc;

        public static async Task DownloadAll(List<GqlResponse_Data_Subreddit_Layers_Edge> edges)
        {
            //Start all
            wc = new HttpClient();
            Console.WriteLine("[Download] Starting now...");
            DateTime startTime = DateTime.UtcNow;
            int numDownloaded = 0;
            int numOk = 0;
            int numCached = 0; //Wrong name, but ones already downloaded

            //Start worker thread for updating
            Thread t = new Thread(() =>
            {
                while(true)
                {
                    UpdateMsg(startTime, numDownloaded, numOk, edges.Count);
                    Thread.Sleep(100);
                }
            });
            t.IsBackground = true;
            t.Start();

            //Run
            Parallel.For(0, edges.Count, (int i) =>
            {
                bool ok = DownloadEdge(edges[i]).GetAwaiter().GetResult();
                numDownloaded++;
                if (ok)
                    numOk++;
            });

            //Kill thread
            t.Abort();

            //Wait
            Thread.Sleep(300);

            //Update
            Console.Write("\r[Download] Done!");
            Console.ReadLine();
        }

        private static void UpdateMsg(DateTime startTime, int numDownloaded, int numOk, int count)
        {
            //Update message
            //Protect against div by 0
            if(numDownloaded == 0 || numOk == 0)
            {
                Console.Write("\r[Download] Pending first download, please wait...");
                return;
            }
            TimeSpan timePerPost = (DateTime.UtcNow - startTime).Divide(numDownloaded);
            TimeSpan remainingTime = timePerPost.Multiply(count - numDownloaded);
            float okPercent = ((float)numOk) / ((float)numDownloaded);
            Console.Write($"\r[Download] Downloaded {numDownloaded}/{count} edges. Time remaining: {remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s. {numOk}/{numDownloaded} ({okPercent * 100}%) OK.         ");
        }

        private static async Task<bool> DownloadEdge(GqlResponse_Data_Subreddit_Layers_Edge e)
        {
            //Get the ID
            string id = e.node.id;

            //Start downloads
            bool ok = true;
            bool cached = false;

            //Download the image
            try
            {
                if (!File.Exists(Program.OUTPUT_IMGS + id + ".png"))
                {
                    using (FileStream fs = new FileStream(Program.OUTPUT_IMGS + id + ".png", FileMode.Create))
                    using (Stream img = await wc.GetStreamAsync(e.node.imageUrl))
                        await img.CopyToAsync(fs);
                }
                else
                {
                    cached = true;
                }
            }
            catch
            {
                ok = false;
                Console.Write($"\r[Download] Failed: {e.node.imageUrl} with index {id}.\n");
                Program.log.Add($"[Download] Failed: {e.node.imageUrl} with index {id}.");
            }

            //Download the post
            try
            {
                if (!File.Exists(Program.OUTPUT_POSTS + id + ".json"))
                {
                    using (FileStream fs = new FileStream(Program.OUTPUT_POSTS + id + ".json", FileMode.Create))
                    using (Stream post = await wc.GetStreamAsync(e.node.postUrl + ".json"))
                        await post.CopyToAsync(fs);
                }
                else
                {
                    cached = true;
                }
            }
            catch
            {
                ok = false;
                Console.Write($"\r[Download] Failed: {e.node.postUrl} with index {id}.\n");
                Program.log.Add($"[Download] Failed: {e.node.postUrl} with index {id}.");
            }

            return ok;
        }
    }
}
