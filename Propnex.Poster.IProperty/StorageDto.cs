using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{

    public class ResponseImageDto
    {
        public StorageDto storage { get; set; }

        public ImageDto image { get; set; }
    }


    public class StorageDto
    {
        public string Bucket { get; set; }

        public string ETag { get; set; }

        public string Key { get; set; }

        public string Location { get; set; }

        public string ServerSideEncryption { get; set; }

    }

    public class ImageDto
    {
        public string format { get; set; }

        public int width { get; set; }

        public int height { get; set; }  
    
        public bool premultiplied { get; set; }

        public int size { get; set; }
    }
}
