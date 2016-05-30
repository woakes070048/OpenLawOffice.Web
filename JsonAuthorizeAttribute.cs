using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenLawOffice.Web
{
    public class JsonAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");

            Guid? token = filterContext.HttpContext.Request.GetToken();

            if (token == null || !token.HasValue)
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Security security = new Security();

                if (!security.VerifyToken(conn, token.Value, true))
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                    return;
                }
            }
        }
    }
}