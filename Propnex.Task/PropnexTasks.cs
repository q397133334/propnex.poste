using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Propnex
{

    public class PropnexTasks
    {
        public PropnexTasks()
        {
            Tasks = new List<PropnexTask>();
        }

        public PropnexTasks(string content)
        {
            var lenght = content.IndexOf("Xpressor-Listing-File===");
            var taskContent = content.Substring(0, lenght == -1 ? content.Length : lenght);
            Tasks = new List<PropnexTask>();
            var document = XDocument.Parse(taskContent);
            if (document != null)
            {
                Tasks = new List<PropnexTask>();
                var root = document.Root;
                var taskDocument = root.Elements("Task");
                foreach (var element in taskDocument)
                {
                    var task = new PropnexTask()
                    {
                        Id = element.ElementString("id"),
                        TaskType = element.ElementString("task_type"),
                        MaxDeay = element.ElementString("max_delay"),
                        EndTime = element.ElementString("end_time"),
                        AccountId = element.ElementString("account_id"),
                        Account = element.ElementString("account"),
                        Password = element.ElementString("password"),
                        TargetPortal = element.ElementString("target_portal"),
                        AccountNotes = element.ElementString("account_notes"),
                        UserName = element.ElementString("username"),
                        UserGroup = element.ElementString("user_group"),
                        ListingFile = element.ElementString("listing_file"),
                        ListingCategory = element.ElementString("listing_category"),
                        Country = element.ElementString("country"),
                        Status = element.ElementString("status"),
                    };
                    content = content.Replace("cef_", "");
                    // find listingfile start index
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
                    Tasks.Add(task);
                }
            }
        }

        public List<PropnexTask> Tasks { get; set; }

    }

    public class PropnexTask
    {
        public PropnexTask()
        {
            Listings = new PropnexListings();
        }

        public string Id { get; set; }

        public string TaskType { get; set; }

        public string MaxDeay { get; set; }

        public string EndTime { get; set; }

        public string AccountId { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public string TargetPortal { get; set; }

        public string AccountNotes { get; set; }

        public string UserName { get; set; }

        public string UserGroup { get; set; }

        public string ListingFile { get; set; }

        public string ListingCategory { get; set; }

        public string Country { get; set; }

        public string Status { get; set; }

        public PropnexListings Listings { get; set; }
    }
}
