using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BabybusSSApi.DatabaseModel;
using BabyBusSSApi.ServiceModel.Enumeration;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Web;

namespace Foundation.ServiceInterface.Utilities
{
    public class ReliableCredentialsAuthProvider : CredentialsAuthProvider
    {
        public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
        {
            return base.TryAuthenticate(authService, userName, password);
        }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
        {
            var response = base.Authenticate(authService, session, request);
            return response;
        }

        public override IHttpResult OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens,
            Dictionary<string, string> authInfo)
        {
            //            using (var cache = authService.TryResolve<ICacheClient>())
            //            {
            //                var sessionPattern = IdUtils.CreateUrn<IAuthSession>("");
            //                var sessionKeys = cache.GetKeysStartingWith(sessionPattern).ToList();
            //                var allSessions = cache.GetAll<IAuthSession>(sessionKeys);
            //                var existingSessions = allSessions.Where(s => s.Value.UserName == session.UserName);
            //                cache.RemoveAll(existingSessions.Select(s => s.Key));
            //            }
            var authRepo = (OrmLiteAuthRepository)authService.TryResolve<IUserAuthRepository>();

            var reliableUserSession = session as ReliableUserSession;
            //Fill the IAuthSession with data which you want to retrieve in the app eg:
            Debug.Assert(reliableUserSession != null, "babybusUserSession != null");
            var auth = authRepo.GetUserAuth(reliableUserSession.UserAuthId);
            if (auth != null && auth.RefId != null)
            {
                reliableUserSession.UserId = auth.RefId.Value;

                using (var db = authService.TryResolve<IDbConnectionFactory>().Open())
                {
                    var user = db.SingleById<User>(reliableUserSession.UserId);
                    reliableUserSession.RoleType = (RoleType)user.RoleType;
                    reliableUserSession.ClassId = user.ClassId;
                    reliableUserSession.KindergartenId = user.KindergartenId;
                    reliableUserSession.LoginName = user.LoginName;
                    reliableUserSession.RealName = user.RealName;
                    reliableUserSession.HeadImage = user.ImageName;
                    reliableUserSession.OpenId = user.LoginName;
                    reliableUserSession.GroupId = user.GroupId;
                    reliableUserSession.UserGender = user.Gender??1;
                    if (reliableUserSession.UserId != 0)
                    {
                        var child = db.Where<UV_Child>("UserId", reliableUserSession.UserId)
                               .OrderByDescending(c => c.IsSelected)
                               .FirstOrDefault();
                        if (child != null)
                        {
                            reliableUserSession.ClassType = child.ClassType;
                        }
                    }

                    if (reliableUserSession.RoleType == RoleType.Parent)
                    {
                        if (reliableUserSession.UserId != 0)
                        {
                            var child = db.Where<UV_Child>("UserId", reliableUserSession.UserId)
                                .OrderByDescending(c => c.IsSelected)
                                .FirstOrDefault();
                            if (child != null)
                            {
                                reliableUserSession.ChildId = child.ChildId;
                                reliableUserSession.ChildName = child.ChildName;
                                reliableUserSession.ClassId = child.ClassId;
                                reliableUserSession.KindergartenId = child.KindergartenId;
                                reliableUserSession.ClassName = child.ClassName;
                                reliableUserSession.KindergartenName = child.KindergartenName;
                                reliableUserSession.IsTestMember = child.IsTestMember ?? false;
                                reliableUserSession.MemberType = child.MemberType;

                                reliableUserSession.Birthday = child.Birthday;
                                reliableUserSession.Gender = child.Gender;
                                reliableUserSession.HeadImage = child.ImageName;
                            }
                        }
                    }
                    else if (reliableUserSession.RoleType == RoleType.Teacher)
                    {
                        if (reliableUserSession.ClassId != 0)
                        {
                            var classTable = db.Single<Class>(c => c.Id == reliableUserSession.ClassId);
                            reliableUserSession.ClassName = classTable.ClassName;
                            reliableUserSession.ClassId = classTable.Id;

                            var kindergarten =
                                db.Single<Kindergarten>(c => c.KindergartenId == reliableUserSession.KindergartenId);
                            reliableUserSession.KindergartenName = kindergarten.KindergartenName;
                            reliableUserSession.KindergartenId = kindergarten.KindergartenId;
                        }
                    }
                    else if (reliableUserSession.RoleType == RoleType.HeadMaster)
                    {
                        if (reliableUserSession.KindergartenId != 0)
                        {
                            var kindergarten =
                            db.Single<Kindergarten>(c => c.KindergartenId == reliableUserSession.KindergartenId);
                            reliableUserSession.KindergartenName = kindergarten.KindergartenName;
                            //                            babybusUserSession.KindergartenId = kindergarten.KindergartenId;
                            reliableUserSession.ServiceType = kindergarten.ServiceType;
                            //                            babybusUserSession.KindergartenRegionCode = kindergarten.RegionCode;
                        }
                        reliableUserSession.ServiceUnitId = user.CooperatedId;
                    }
                    else if (reliableUserSession.RoleType == RoleType.SuperHeadMaster)
                    {
                        if (reliableUserSession.UserId != 0)
                        {
                            var groupInformation = db.Single<Group>(g => g.Id == reliableUserSession.GroupId);
                            reliableUserSession.GroupName = groupInformation.GroupName;
                        }
                    }
                    else if (reliableUserSession.RoleType == RoleType.Agency || reliableUserSession.RoleType == RoleType.Agency2)
                    {
                        if (reliableUserSession.UserId != 0)
                        {
                            reliableUserSession.AgencyId = user.CooperatedId;
                        }
                        if (reliableUserSession.RoleType == RoleType.Agency2)
                        {
                            var unitInfo = db.SingleById<DB_ServiceUnit>(user.CooperatedId);
                            reliableUserSession.UnitType = unitInfo.UnitType;
                        }
                    }
                    else if (reliableUserSession.RoleType == RoleType.Alliance)
                    {
                        if (reliableUserSession.UserId != 0)
                        {
                            reliableUserSession.AllianceId = user.CooperatedId;
                        }
                    }
                }
            }

            //Important: You need to save the session!
            authService.SaveSession(reliableUserSession, SessionExpiry);

            return base.OnAuthenticated(authService, session, tokens, authInfo);
        }
    }
}
