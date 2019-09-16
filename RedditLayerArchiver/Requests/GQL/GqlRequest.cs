using System;
using System.Collections.Generic;
using System.Text;

namespace RedditLayerArchiver.Requests.GQL
{
    public class GqlRequest
    {
        public string id;
        public GqlRequest_Vars variables;
    }

    public class GqlRequest_Vars
    {
        public string subredditId;
        public int first;
        public string after;
    }
}
