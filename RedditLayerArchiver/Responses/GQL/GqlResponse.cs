using System;
using System.Collections.Generic;
using System.Text;

namespace RedditLayerArchiver.Responses.GQL
{
    public class GqlResponse
    {
        public GqlResponse_Data data;
    }

    public class GqlResponse_Data
    {
        public GqlResponse_Data_Subreddit subreddit;
    }

    public class GqlResponse_Data_Subreddit
    {
        public string id;
        public GqlResponse_Data_Subreddit_Layers layers;
    }

    public class GqlResponse_Data_Subreddit_Layers
    {
        public GqlResponse_Data_Subreddit_Layers_PageInfo pageInfo;
        public List<GqlResponse_Data_Subreddit_Layers_Edge> edges;
    }

    public class GqlResponse_Data_Subreddit_Layers_PageInfo
    {
        public bool hasNextPage;
        public bool hasPreviousPage;
        public string startCursor;
        public string endCursor;
    }

    public class GqlResponse_Data_Subreddit_Layers_Edge
    {
        public string cursor;
        public GqlResponse_Data_Subreddit_Layers_Edge_Node node;
    }

    public class GqlResponse_Data_Subreddit_Layers_Edge_Node
    {
        public string id;
        public string name;
        public string imageUrl;
        public string postUrl;
        public GqlResponse_Data_Subreddit_Layers_Edge_Node_Box box;
        public GqlResponse_Data_Subreddit_Layers_Edge_Node_PostInfo postInfo;
    }

    public class GqlResponse_Data_Subreddit_Layers_Edge_Node_Box
    {
        public GqlResponse_Data_Subreddit_Layers_Edge_Node_Box_Point startPoint;
        public GqlResponse_Data_Subreddit_Layers_Edge_Node_Box_Point endPoint;
    }

    public class GqlResponse_Data_Subreddit_Layers_Edge_Node_Box_Point
    {
        public int x;
        public int y;
    }

    public class GqlResponse_Data_Subreddit_Layers_Edge_Node_PostInfo
    {
        public GqlResponse_Data_Subreddit_Layers_Edge_Node_AuthorInfo authorInfo;
    }

    public class GqlResponse_Data_Subreddit_Layers_Edge_Node_AuthorInfo
    {
        public string id;
    }
}
