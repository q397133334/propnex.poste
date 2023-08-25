using Propnex.Poster.PropertyGuru.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml.Linq;

namespace Propnex.Poster.PropertyGuru.Tasks
{
    public class GuruTasks
    {
        public GuruTasks() { }


        public GuruTaskListings GuruTaskListings { get; set; }



        public GuruTasks(string context, string taskcontext)
        {
            var document = XDocument.Parse(taskcontext);
            if (document != null)
            {
                Tasks = new List<GuruTask>();
                var root = document.Root;
                var taskDocument = root.Elements("Task");
                foreach (var element in taskDocument)
                {
                    var task = new GuruTask()
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
                        Timing = element.ElementString("timing"),
                        DropToPage = element.ElementString("drop_to_page"),
                        Source = element.ElementString("source", "")
                    };
                    Tasks.Add(task);
                }

                //var listContext = context.Substring(context.IndexOf("Xpressor-Listing-File==="), context.Length - 1);
                foreach (var task in Tasks)
                {
                    // find listingfile start index
                    var listingFileIndex = context.IndexOf($"Xpressor-Listing-File==={task.ListingFile}");
                    // find listingfile end index
                    var listingFileEndIndex = context.IndexOf("Xpressor-Listing-File===", listingFileIndex + 1);

                    if (task.TaskType.IndexOf("Retrieve") > -1)
                    {
                        continue;
                    }

                    if (listingFileEndIndex == -1)
                    {
                        listingFileEndIndex = context.Length - 1;
                    }
                    //get listingfile context
                    var listsContext = context.Substring(listingFileIndex, listingFileEndIndex - listingFileIndex);
                    //remove Xpressor-Listing-File===
                    listsContext = listsContext.Replace($"Xpressor-Listing-File==={task.ListingFile}", "");
                    listsContext = listsContext.Trim('\r');
                    listsContext = listsContext.Trim('\n');
                    listsContext = listsContext.Trim('\r');
                    if (listsContext.IndexOf("Xpressor-Listing-File") > -1)
                    {
                        listsContext = listsContext.Replace($"Xpressor-Listing-File==={task.ListingFile}\r\n", "");
                    }
                    System.IO.File.WriteAllText("E:\\222.txt", listsContext);
                    task.Listings = new GuruTaskListings(listsContext);
                }
            }
        }

        public List<GuruTask> Tasks { get; set; }
    }
}
