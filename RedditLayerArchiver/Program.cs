using Newtonsoft.Json;
using RedditLayerArchiver.Responses.GQL;
using RedditLayerArchiver.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace RedditLayerArchiver
{
    class Program
    {
        public const string OUTPUT_IMGS = @"E:\Reddit Layer\img\";
        public const string OUTPUT_IMGS_CORRUPT = @"E:\Reddit Layer\img_corrupt\";
        public const string OUTPUT_POSTS = @"E:\Reddit Layer\posts\";
        public const string OUTPUT_CANVAS = @"E:\Reddit Layer\canvas_date\";
        public const string OUTPUT_INDEX = @"E:\Reddit Layer\index.json";
        public const string OUTPUT_LOG = @"E:\Reddit Layer\error_log.log";

        public static HttpClient client;
        public static List<string> log;

        static void Main(string[] args)
        {
            //Init
            client = new HttpClient();
            log = new List<string>();

            //Load data
            var edges = JsonConvert.DeserializeObject<List<GqlResponse_Data_Subreddit_Layers_Edge>>(File.ReadAllText(OUTPUT_INDEX));
            edges.Reverse();

            //Download images
            //ImageDownloader.DownloadAll(edges).GetAwaiter().GetResult();
            CanvasGenerator.GenerateAll(edges);

            //Save log
            File.WriteAllLines(OUTPUT_LOG, log.ToArray());
        }

        static void SeekPosts()
        {
            //Set up
            client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("rp-archiver/0.1");

            //Obtain token
            WebTools.ObtainToken().GetAwaiter().GetResult();

            //Go
            var edges = Tools.DataSeekerTool.SeekAll().GetAwaiter().GetResult();
            File.WriteAllText(OUTPUT_INDEX, JsonConvert.SerializeObject(edges));
        }
    }
}
