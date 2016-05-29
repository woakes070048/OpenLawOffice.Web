using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Assets
{
    public class CreateVersionViewModel
    {
        public bool IsFinal { get; set; }
        public bool IsCourtFiled { get; set; }

        public VersionViewModel Version { get; set; }
        
        public List<HttpPostedFileBase> Files { get; set; }

        public string[] SourceFiles { get; set; }
    }
}