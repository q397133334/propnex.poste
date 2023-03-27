using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Propnex.Task
{
    public class DefaultPropnexTaskProvider : IPropnexTaskProvider
    {
        private PropnexTasks Tasks { get; set; }

        public DefaultPropnexTaskProvider()
        {
            Tasks = new PropnexTasks();
        }


        public PropnexTasks GetTasks(string content)
        {

            return Tasks;
        }

        private void initListings(string content)
        {
            foreach(var task in Tasks.Tasks)
            {
                var listingFileIndex = content.IndexOf($"Xpressor-Listing-File==={task.ListingFile}");
                // find listingfile end index
                var listingFileEndIndex = content.IndexOf("Xpressor-Listing-File===", listingFileIndex + 1);

                if (task.TaskType.IndexOf("Retrieve") > -1)
                {
                    continue;
                }

                if (listingFileEndIndex == -1)
                {
                    listingFileEndIndex = content.Length - 1;
                }
                //get listingfile context
                var listsContext = content.Substring(listingFileIndex, listingFileEndIndex - listingFileIndex);
                //remove Xpressor-Listing-File===
                listsContext = listsContext.Replace($"Xpressor-Listing-File==={task.ListingFile}", "");
                listsContext = listsContext.Trim('\r');
                listsContext = listsContext.Trim('\n');
                listsContext = listsContext.Trim('\r');
                if (listsContext.IndexOf("Xpressor-Listing-File") > -1)
                {
                    listsContext = listsContext.Replace($"Xpressor-Listing-File==={task.ListingFile}\r\n", "");
                }

                task.Listings = new PropnexListings(listsContext);
            }
        }

        private void initTasks(string taskContent)
        {
            
        }

    }
}
