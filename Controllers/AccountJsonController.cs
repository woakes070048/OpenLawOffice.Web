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
            Common.Net.AuthPackage authPackage;
            Common.Net.Response<Guid> response = new Common.Net.Response<Guid>();

            response.RequestReceived = DateTime.Now;

            authPackage = Request.InputStream.JsonDeserialize<Common.Net.AuthPackage>();

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    dynamic profile;
                    Common.Models.Account.Users user = Data.Account.Users.Get(trans, authPackage.Username);
                    profile = ProfileBase.Create(user.Username);

                    // decrypt password
                    Common.Encryption enc = new Common.Encryption();
                    Common.Encryption.Package package;
                    enc.IV = authPackage.IV;
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
                        Input = authPackage.Password
                    });
                    if (string.IsNullOrEmpty(package.Output))
                    {
                        response.Successful = false;
                        response.Package = Guid.Empty;
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    authPackage.Password = package.Output;

                    string hashFromDb = Security.ClientHashPassword(user.Password);
                    string hashFromWeb = Security.ClientHashPassword(authPackage.Password);

                    if (MembershipService.ValidateUser(authPackage.Username, authPackage.Password))
                    {
                        Common.Models.External.ExternalSession session =
                            Data.External.ExternalSession.Get(trans, authPackage.AppName, authPackage.MachineId, authPackage.Username);
                        user = Data.Account.Users.Get(trans, authPackage.Username);

                        if (session == null)
                        { // create
                            session = Data.External.ExternalSession.Create(trans, new Common.Models.External.ExternalSession()
                            {
                                MachineId = authPackage.MachineId,
                                User = user,
                                AppName = authPackage.AppName
                            });
                        }
                        else
                        { // update
                            session = Data.External.ExternalSession.Update(trans, new Common.Models.External.ExternalSession()
                            {
                                Id = session.Id,
                                MachineId = authPackage.MachineId,
                                User = user,
                                AppName = authPackage.AppName
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

        [HttpPost]
        [JsonAuthorize]
        public ActionResult CloseSession()
        {
            Guid? token;
            Common.Net.Request<Common.Net.AuthPackage> request;
            Common.Models.External.ExternalSession session;
            Common.Net.Response<bool> response = new Common.Net.Response<bool>();

            request = Request.InputStream.JsonDeserialize<Common.Net.Request<Common.Net.AuthPackage>>();

            response.RequestReceived = DateTime.Now;

            token = Request.GetToken();
            if (token == null || !token.HasValue)
            {
                response.Successful = false;
                response.Error = "Invalid Auth Token";
                response.Package = false;
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    // Close the session here
                    session = Data.External.ExternalSession.Get(trans, request.Package.AppName, request.Package.MachineId, request.Package.Username);
                    session = Data.External.ExternalSession.Delete(trans, session);

                    trans.Commit();

                    response.Successful = true;
                    response.Package = true;
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = false;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}