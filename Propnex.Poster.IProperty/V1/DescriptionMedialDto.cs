using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty.V1
{
    public class DescriptionMedialDto
    {
        public ListingMultiLangTextV1 description { get; set; }

        public dynamic extension { get; set; } = new { isLiveTourAvailable = false };

        public List<DescriptionMedialPhotoDto> floorPlans { get; set; }

        public string id { get; set; }

        public List<DescriptionMedialPhotoDto> images { get; set; }

        public NullClass location { get; set; }

        public List<DescriptionMedialPhotoDto> photo360s { get; set; }

        public int? saleableAreaMeasurementCode { get; set; }

        public ListingMultiLangTextV1 title { get; set; }

        public List<dynamic> videos { get; set; }
    }

    public class DescriptionMedialPhotoDto
    {
        public NullClass caption { get; set; }

        public string fullPath { get; set; }

        public int height { get; set; }

        public string id { get; set; }

        public string path { get; set; }

        public int width { get; set; }
    }
}
