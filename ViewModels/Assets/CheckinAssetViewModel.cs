using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Assets
{
    public class CheckinAssetViewModel
    {
        public AssetViewModel Asset { get; set; }
        public VersionViewModel Version { get; set; }

        public List<HttpPostedFileBase> Files { get; set; }

        public string[] SourceFiles { get; set; }

        public List<TagViewModel> Tags { get; set; }
    }
}