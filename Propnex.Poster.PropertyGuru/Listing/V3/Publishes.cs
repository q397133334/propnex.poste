using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class Publishe
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }
    }

    public class PublishRequest
    {
        [JsonProperty("publishes")]
        public List<Publishe> Publishes { get; set; } = new List<Publishe>();
    }

    public class ResponsePublisheResult
    {
        public List<Result> Results { get; set; } = new List<Result>();
}

    public class Result
    {
        public int ListingId { get; set; }

        public int listerId { get; set; }

        public string Brand { get; set; }

        public bool IsSuccess { get; set; }
    }
}
