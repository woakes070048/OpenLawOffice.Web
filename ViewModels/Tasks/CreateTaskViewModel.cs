using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Tasks
{
    public class CreateTaskViewModel
    {
        public TaskViewModel Task { get; set; }

        public Tasks.TaskAssignedContactViewModel TaskContact { get; set; }

        public List<TaskTemplateViewModel> TaskTemplates { get; set; }
    }
}