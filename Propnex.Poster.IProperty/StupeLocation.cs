using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class StupeLocation
    {
        public LocationDto Location { get; set; }

        public List<int> buildingFacilityCodes { get; set; } = new List<int>();

        public object extension { get; set; } = new object();

        public int? propertyCategoryTypeCode {  get; set; }  

        public int? propertyGroupTypeCode {  get; set; } 

        public int? propertyTypeCode { get; set; }

        public int? saleableAreaMeasurementCode {  get; set; }

        public int? storeyCode { get; set; }
    }
}
