using System.Collections.Generic;

namespace Propnex.Poster.PropertyGuru
{
    /// <summary>
    /// Represents a single "key -> localized value" meta entry,
    /// e.g. { "key": "slug", "value": { "en": "sg" } }
    /// </summary>
    public class MetaEntry
    {
        public string Key { get; set; }
        public Dictionary<string, string> Value { get; set; }

        public MetaEntry(string key, Dictionary<string, string> value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Generic geo node: used for country, region and district levels.
    /// </summary>
    public class GeoNode
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public string LevelName { get; set; }
        public string Id { get; set; }
        public GeoNode Parent { get; set; }
        public List<GeoNode> Children { get; set; }
        public List<MetaEntry> Meta { get; set; }

        public GeoNode(
            int level,
            string name,
            string levelName,
            string id,
            GeoNode parent,
            List<MetaEntry> meta,
            List<GeoNode> children = null)
        {
            Level = level;
            Name = name;
            LevelName = levelName;
            Id = id;
            Parent = parent;
            Meta = meta;
            Children = children ?? new List<GeoNode>();
        }

        private static List<MetaEntry> SlugMeta(string slug) => new List<MetaEntry>
        {
            new MetaEntry("slug", new Dictionary<string, string> { { "en", slug } }),
            new MetaEntry("legacySlug", new Dictionary<string, string>())
        };

        // ---------------------------------------------------------------
        // Country
        // ---------------------------------------------------------------
        public static readonly GeoNode Singapore = new GeoNode(
            level: 100,
            name: "Singapore",
            levelName: "country",
            id: "fh3ti",
            parent: null,
            meta: SlugMeta("sg"));

        // ---------------------------------------------------------------
        // Regions (level 200)
        // ---------------------------------------------------------------
        public static readonly GeoNode RegionNorth = new GeoNode(200, "North (D25-28)", "region", "I", Singapore, SlugMeta("north-d25-28"));
        public static readonly GeoNode RegionWest = new GeoNode(200, "West (D22-24)", "region", "H", Singapore, SlugMeta("west-d22-24"));
        public static readonly GeoNode RegionNewtonBtTimah = new GeoNode(200, "Newton / Bt. Timah (D11, 21)", "region", "C", Singapore, SlugMeta("newton-bt-timah-d11-21"));
        public static readonly GeoNode RegionSerangoonThomson = new GeoNode(200, "Serangoon / Thomson (D19-20)", "region", "G", Singapore, SlugMeta("serangoon-thomson-d19-20"));
        public static readonly GeoNode RegionChangiPasirRis = new GeoNode(200, "Changi / Pasir Ris (D17-18)", "region", "F", Singapore, SlugMeta("changi-pasir-ris-d17-18"));
        public static readonly GeoNode RegionEastCoast = new GeoNode(200, "East Coast (D15-16)", "region", "E", Singapore, SlugMeta("east-coast-d15-16"));
        public static readonly GeoNode RegionBalestierGeylang = new GeoNode(200, "Balestier / Geylang (D12-14)", "region", "D", Singapore, SlugMeta("balestier-geylang-d12-14"));
        public static readonly GeoNode RegionOrchardHolland = new GeoNode(200, "Orchard / Holland (D09-10)", "region", "B", Singapore, SlugMeta("orchard-holland-d09-10"));
        public static readonly GeoNode RegionCitySouthWest = new GeoNode(200, "City & South West (D01-08)", "region", "A", Singapore, SlugMeta("city-south-west-d01-08"));

        // ---------------------------------------------------------------
        // Districts (level 500)
        // ---------------------------------------------------------------
        public static readonly GeoNode D28 = new GeoNode(500, "Seletar / Yio Chu Kang", "district", "D28", RegionNorth, SlugMeta("seletar-yio-chu-kang"));
        public static readonly GeoNode D27 = new GeoNode(500, "Sembawang / Yishun", "district", "D27", RegionNorth, SlugMeta("sembawang-yishun"));
        public static readonly GeoNode D26 = new GeoNode(500, "Mandai / Upper Thomson", "district", "D26", RegionNorth, SlugMeta("mandai-upper-thomson"));
        public static readonly GeoNode D25 = new GeoNode(500, "Admiralty / Woodlands", "district", "D25", RegionNorth, SlugMeta("admiralty-woodlands"));

        public static readonly GeoNode D24 = new GeoNode(500, "Lim Chu Kang / Tengah", "district", "D24", RegionWest, SlugMeta("choa-chu-kang-tengah"));
        public static readonly GeoNode D23 = new GeoNode(500, "Dairy Farm / Bukit Panjang / Choa Chu Kang", "district", "D23", RegionWest, SlugMeta("bukit-batok-bukit-panjang"));
        public static readonly GeoNode D22 = new GeoNode(500, "Boon Lay / Jurong / Tuas", "district", "D22", RegionWest, SlugMeta("boon-lay-jurong-tuas"));

        public static readonly GeoNode D21 = new GeoNode(500, "Clementi Park / Upper Bukit Timah", "district", "D21", RegionNewtonBtTimah, SlugMeta("clementi-park-upper-bukit-timah"));
        public static readonly GeoNode D11 = new GeoNode(500, "Newton / Novena", "district", "D11", RegionNewtonBtTimah, SlugMeta("newton-novena"));

        public static readonly GeoNode D20 = new GeoNode(500, "Ang Mo Kio / Bishan / Thomson", "district", "D20", RegionSerangoonThomson, SlugMeta("ang-mo-kio-bishan-thomson"));
        public static readonly GeoNode D19 = new GeoNode(500, "Hougang / Punggol / Sengkang", "district", "D19", RegionSerangoonThomson, SlugMeta("hougang-punggol-sengkang"));

        public static readonly GeoNode D18 = new GeoNode(500, "Pasir Ris / Tampines", "district", "D18", RegionChangiPasirRis, SlugMeta("pasir-ris-tampines"));
        public static readonly GeoNode D17 = new GeoNode(500, "Changi Airport / Changi Village", "district", "D17", RegionChangiPasirRis, SlugMeta("changi-airport-changi-village"));

        public static readonly GeoNode D16 = new GeoNode(500, "Bedok / Upper East Coast", "district", "D16", RegionEastCoast, SlugMeta("bedok-upper-east-coast"));
        public static readonly GeoNode D15 = new GeoNode(500, "East Coast / Marine Parade", "district", "D15", RegionEastCoast, SlugMeta("east-coast-marine-parade"));

        public static readonly GeoNode D14 = new GeoNode(500, "Eunos / Geylang / Paya Lebar", "district", "D14", RegionBalestierGeylang, SlugMeta("eunos-geylang-paya-lebar"));
        public static readonly GeoNode D13 = new GeoNode(500, "Macpherson / Potong Pasir", "district", "D13", RegionBalestierGeylang, SlugMeta("macpherson-potong-pasir"));
        public static readonly GeoNode D12 = new GeoNode(500, "Balestier / Toa Payoh", "district", "D12", RegionBalestierGeylang, SlugMeta("balestier-toa-payoh"));

        public static readonly GeoNode D10 = new GeoNode(500, "Tanglin / Holland / Bukit Timah", "district", "D10", RegionOrchardHolland, SlugMeta("tanglin-holland"));
        public static readonly GeoNode D09 = new GeoNode(500, "Orchard / River Valley", "district", "D09", RegionOrchardHolland, SlugMeta("orchard-river-valley"));

        public static readonly GeoNode D08 = new GeoNode(500, "Farrer Park / Serangoon Rd", "district", "D08", RegionCitySouthWest, SlugMeta("farrer-park-serangoon-rd"));
        public static readonly GeoNode D07 = new GeoNode(500, "Beach Road / Bugis / Rochor", "district", "D07", RegionCitySouthWest, SlugMeta("beach-road-bugis-rochor"));
        public static readonly GeoNode D06 = new GeoNode(500, "City Hall / Clarke Quay", "district", "D06", RegionCitySouthWest, SlugMeta("city-hall-clarke-quay"));
        public static readonly GeoNode D05 = new GeoNode(500, "Buona Vista / West Coast / Clementi New Town", "district", "D05", RegionCitySouthWest, SlugMeta("buona-vista-west-coast-clementi-new-town"));
        public static readonly GeoNode D04 = new GeoNode(500, "Harbourfront / Telok Blangah", "district", "D04", RegionCitySouthWest, SlugMeta("harbourfront-telok-blangah"));
        public static readonly GeoNode D03 = new GeoNode(500, "Alexandra / Commonwealth", "district", "D03", RegionCitySouthWest, SlugMeta("alexandra-commonwealth"));
        public static readonly GeoNode D02 = new GeoNode(500, "Chinatown / Tanjong Pagar", "district", "D02", RegionCitySouthWest, SlugMeta("chinatown-tanjong-pagar"));
        public static readonly GeoNode D01 = new GeoNode(500, "Boat Quay / Raffles Place / Marina", "district", "D01", RegionCitySouthWest, SlugMeta("boat-quay-raffles-place-marina"));

        // ---------------------------------------------------------------
        // Flat list of all districts, in the same order as the source JSON
        // ---------------------------------------------------------------
        public static readonly List<GeoNode> Districts = new List<GeoNode>
        {
            D28, D27, D26, D25,
            D24, D23, D22,
            D21,
            D20, D19,
            D18, D17,
            D16, D15,
            D14, D13, D12,
            D11,
            D10, D09,
            D08, D07, D06, D05, D04, D03, D02, D01
        };
    }
}
