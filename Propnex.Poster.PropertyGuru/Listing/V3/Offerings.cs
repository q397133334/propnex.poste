using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class Offerings
    {
        /// <summary>
        /// 产品列表
        /// </summary>
        [JsonProperty("products")]
        public List<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        /// <summary>
        /// 产品唯一标识
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// 列表ID
        /// </summary>
        [JsonProperty("listingId")]
        public long ListingId { get; set; }

        /// <summary>
        /// 列表状态码
        /// </summary>
        [JsonProperty("listingStatusCode")]
        public string ListingStatusCode { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        [JsonProperty("brand")]
        public string Brand { get; set; }

        /// <summary>
        /// 组类型代码
        /// </summary>
        [JsonProperty("groupTypeCode")]
        public string GroupTypeCode { get; set; }

        /// <summary>
        /// 类型代码
        /// </summary>
        [JsonProperty("typeCode")]
        public string TypeCode { get; set; }

        /// <summary>
        /// 货币类型代码
        /// </summary>
        [JsonProperty("currencyTypeCode")]
        public string CurrencyTypeCode { get; set; }

        /// <summary>
        /// 产品持续时间
        /// </summary>
        [JsonProperty("productDuration")]
        public int ProductDuration { get; set; }

        /// <summary>
        /// 产品周期（如：day）
        /// </summary>
        [JsonProperty("productPeriod")]
        public string ProductPeriod { get; set; }

        /// <summary>
        /// 成本
        /// </summary>
        [JsonProperty("cost")]
        public int Cost { get; set; }

        /// <summary>
        /// 剩余数量
        /// </summary>
        [JsonProperty("remaining")]
        public int Remaining { get; set; }

        /// <summary>
        /// 列表配额剩余
        /// </summary>
        [JsonProperty("listingQuotaRemaining")]
        public int ListingQuotaRemaining { get; set; }

        /// <summary>
        /// 是否可消费
        /// </summary>
        [JsonProperty("canConsume")]
        public bool CanConsume { get; set; }
    }
}
