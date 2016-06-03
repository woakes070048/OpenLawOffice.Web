using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OpenLawOffice.Web.Controllers
{
    public class MattersJsonController : Controller
    {
        [HttpGet]
        [JsonAuthorize]
        public ActionResult List(string contactFilter, string titleFilter, string caseNumberFilter,
            int? courtTypeFilter, int? courtGeographicalJurisdictionFilter, bool activeFilter = true)
        {
            Guid? token;
            Common.Net.Response<List<Common.Models.Matters.Matter>> response
                = new Common.Net.Response<List<Common.Models.Matters.Matter>>();

            response.RequestReceived = DateTime.Now;

            token = Request.GetToken();
            if (token == null || !token.HasValue)
            {
                response.Successful = false;
                response.Error = "Invalid Auth Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    response.Package = Data.Matters.Matter.List(trans, activeFilter, contactFilter, titleFilter,
                        caseNumberFilter, courtTypeFilter, courtGeographicalJurisdictionFilter);
                    response.Successful = true;
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}