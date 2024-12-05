using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty.V1
{
    public class ResponseDataV1<T>
    {
        public T Data { get; set; }
    }

    public class RequestDataV1<T>
    {
        public string OperationName { get; set; }

        public T variables { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string query { get; set; } = null;

        public ExtensionsV1 extensions { get; set; } = null;

    }

    public class ExtensionsV1
    {
        public PersistedQueryV1 persistedQuery { get; set; }
    }

    public class PersistedQueryV1
    {
        public int Version { get; set; }

        public string Sha256Hash { get; set; }
    }
}
