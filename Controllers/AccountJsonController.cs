using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Profile;
using System.Web.Routing;

namespace OpenLawOffice.Web.Controllers
{
    public class AccountJsonController : Controller
    {
        public AccountMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        [HttpPost]
        public ActionResult Authenticate()
        {
            Common.Net.Request<Common.Net.AuthPackage> request;
            Common.Net.Response<Guid> response = new Common.Net.Response<Guid>();

            response.RequestReceived = DateTime.Now;
            
            request = Request.InputStream.JsonDeserialize<Common.Net.Request<Common.Net.AuthPackage>>();

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    dynamic profile;
                    Common.Models.Account.Users user = Data.Account.Users.Get(trans, request.Package.Username);
                    profile = ProfileBase.Create(user.Username);

                    // decrypt password
                    Common.Encryption enc = new Common.Encryption();
                    Common.Encryption.Package package;
                    enc.IV = request.Package.IV;
                    if (profile != null && profile.ExternalAppKey != null
                        && !string.IsNullOrEmpty(profile.ExternalAppKey))
                        enc.Key = profile.ExternalAppKey;
                    else
                    {
                        response.Successful = false;
                        response.Package = Guid.Empty;
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    package = enc.Decrypt(new Common.Encryption.Package()
                    {
                        Input = request.Package.Password
                    });
                    if (string.IsNullOrEmpty(package.Output))
                    {
                        response.Successful = false;
                        response.Package = Guid.Empty;
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    request.Package.Password = package.Output;

                    string hashFromDb = Security.ClientHashPassword(user.Password);
                    string hashFromWeb = Security.ClientHashPassword(request.Package.Password);

                    if (MembershipService.ValidateUser(request.Package.Username, request.Package.Password))
                    {
                        Common.Models.External.ExternalSession session =
                            Data.External.ExternalSession.Get(trans, request.Package.AppName, request.Package.MachineId, request.Package.Username);
                        user = Data.Account.Users.Get(trans, request.Package.Username);

                        if (session == null)
                        { // create
                            session = Data.External.ExternalSession.Create(trans, new Common.Models.External.ExternalSession()
                            {
                                MachineId = request.Package.MachineId,
                                User = user,
                                AppName = request.Package.AppName
                            });
                        }
                        else
                        { // update
                            session = Data.External.ExternalSession.Update(trans, new Common.Models.External.ExternalSession()
                            {
                                Id = session.Id,
                                MachineId = request.Package.MachineId,
                                User = user,
                                AppName = request.Package.AppName
                            });
                        }

                        response.Successful = true;
                        response.Package = session.Id.Value;
                        trans.Commit();
                    }
                    else
                    {
                        response.Successful = false;
                        response.Package = Guid.Empty;
                        response.Error = "Invalid security credentials.";
                    }
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = Guid.Empty;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [JsonAuthorize]
        public ActionResult Index()
        {
            return null;
        }
    }
}