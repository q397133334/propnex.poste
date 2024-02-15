using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class BuildingQueryVariablesDto
    {
        public bool includeBuildingFacilityCodes { get; set; } = true;

        public string keyword { get; set; } = "";

        public string q { get; set; } = "level5";

        public bool shouldExtendsFields { get; set; } = true;
    }
}
