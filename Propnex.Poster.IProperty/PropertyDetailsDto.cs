using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{



    public class PropertyDetailsDto
    {
        public int? bathroom { get; set; }

        public int? bedroomCode { get; set; }

        public List<int>? buildingFacilityCodes { get; set; }

        public int? carPark { get; set; }

        public PropertyDetailsExtension extension { get; set; }

        public int? grossArea { get; set; }

        public NullClass location { get; set; }

        public ListingSalePrice maintenanceFee { get; set; }

        public ListingSalePrice rentPrice { get; set; }

        public ListingSalePrice salePrice { get; set; }

        public int? saleableArea { get; set; }

        public int? saleableAreaMeasurementCode { get; set; }

        public int? storeRoom { get; set; }

        public int? tenureCode { get; set; }

        public List<int>? unitFeatureCodes { get; set; }

    }

    public class PropertyDetailsExtension
    {
        public AreaDimension grossAreaDimension { get; set; }

        public bool? isBumiLot { get; set; }

        public AreaDimension saleableAreaDimension { get; set; }

        public int? unexpiredLeaseYear { get; set; }
    }

    public class AreaDimension
    {
        public int? length { get; set; }

        public int? width { get; set; }
    }
}
