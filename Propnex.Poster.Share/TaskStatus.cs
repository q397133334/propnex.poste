using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.Share
{
    public enum TaskStatus
    {
        /// <summary>
        /// 等待中
        /// </summary>
        Wait,
        /// <summary>
        /// 运行中
        /// </summary>
        Runing,
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 失败
        /// </summary>
        Failure,
        /// <summary>
        /// 未找到
        /// </summary>
        NotFind
    }
}
