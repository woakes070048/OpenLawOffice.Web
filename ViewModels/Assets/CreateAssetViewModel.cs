using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Assets
{
    public class CreateAssetViewModel
    {
        public AssetViewModel Asset { get; set; }

        public List<HttpPostedFileBase> Files { get; set; }

        public string[] SourceFiles { get; set; }

        public string[] Tags { get; set; }

        public List<TagViewModel> PossibleTags { get; set; }
    }
}