using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class PropnexTasks
    {
        public List<PropnexTask> Tasks { get; set; }
    }

    public class PropnexTask
    {
        public PropnexTask() { }

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

    }
}
