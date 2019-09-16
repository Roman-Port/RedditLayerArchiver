using RedditLayerArchiver.Requests.GQL;
using RedditLayerArchiver.Responses.GQL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedditLayerArchiver.Tools
{
    public static class DataSeekerTool
    {
        public static async Task<List<GqlResponse_Data_Subreddit_Layers_Edge>> SeekAll()
        {
            Console.WriteLine("[Data Seeker] Starting seek...");
            
            //Loop through until we get no results
            string after = null;
            List<GqlResponse_Data_Subreddit_Layers_Edge> output = new List<GqlResponse_Data_Subreddit_Layers_Edge>();
            while (true)
            {
                //Build a request
                GqlRequest request = new GqlRequest
                {
                    id = "363cd3f2e49f",
                    variables = new GqlRequest_Vars
                    {
                        after = after,
                        first = 500,
                        subredditId = "t5_32b7p"
                    }
                };

                //Get data
                GqlResponse response;
                try
                {
                    response = await WebTools.PostJsonAsync<GqlResponse>("https://gql.reddit.com/", request);
                } catch (Exception ex)
                {
                    Console.WriteLine("FAILED. Going to output files now. " + ex.Message + ex.StackTrace);
                    break;
                }
                var edges = response.data.subreddit.layers.edges;
                Console.WriteLine($"[Data Seeker] Received {edges.Count} edges with range {response.data.subreddit.layers.pageInfo.startCursor}-{response.data.subreddit.layers.pageInfo.endCursor} with target begin {after}. Waiting 10 seconds...");
                if (edges.Count == 0)
                    break;
                after = response.data.subreddit.layers.pageInfo.endCursor;

                //Add to list
                output.AddRange(edges);

                //Wait to avoid Too Many Requests error
                Thread.Sleep(1000 * 10);
            }

            Console.WriteLine($"[Data Seeker] Found {output.Count} edges.");
            return output;
        }
    }
}
