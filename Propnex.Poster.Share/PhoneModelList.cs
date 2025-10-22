using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.Share
{
    public class PhoneModelList
    {
        static List<string> PhoneModels = new List<string>()
        {
            "23127PNOCC",
            "BRA-AL00",
            "MNA-AL00",
            "VER-AN10",
            "2211133C",
            "23113RKC6C",
            "23078RKD5C",
            "M481Q",
            "PJJ110",
            "PHZ110",
            "V2329A",
            "SM-N9810",
            "PJD110",
            "PMX3888"
        };
        static Random random = new Random(Guid.NewGuid().GetHashCode());

        public static string GetPhoneModel()
        {
            return PhoneModels[random.Next(PhoneModels.Count)];
        }
    }
}
