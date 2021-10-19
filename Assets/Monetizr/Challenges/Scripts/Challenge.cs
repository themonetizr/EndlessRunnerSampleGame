using System.Collections.Generic;

namespace Monetizr.Challenges
{
    [System.Serializable]
    public class Challenge
    {
        public string id;
        public string title;
        public string content;
        public int progress;
        public int reward;
        public List<Asset> assets = new List<Asset>();

        [System.Serializable]
        public class Asset
        {
            public string id;
            public string type;
            public string title;
            public string url;
        }
    }
}