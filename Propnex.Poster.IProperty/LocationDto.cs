using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class LocationDto
    {


        public string? Address { get; set; }

        public bool? hasNoTownship { get; set; } = false;

        public bool? HideUnitFloor { get; set; } = true;

        public double latitude { get; set; }

        public double longitude { get; set; }

        public Level? level1 { get; set; }
        public Level? level2 { get; set; }
        public Level3? level3 { get; set; }
        public Level? level4 { get; set; }
        public Level? level5 { get; set; }

        public string postalCode { get; set; } = "";

    }
}
