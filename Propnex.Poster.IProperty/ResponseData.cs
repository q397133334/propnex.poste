using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class ResponseData<T>
    {
        public T Data { get; set; }
    }

    public class RequestData<T>
    {
        public string OperationName { get; set; }

        public T variables { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Extensions extensions { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? shouldExtendsFields { get; set; } = null;
    }

    public class Extensions
    {
        public PersistedQuery persistedQuery { get; set; }
    }

    public class PersistedQuery
    {
        public int Version { get; set; }

        public string Sha256Hash { get; set; }
    }
}
