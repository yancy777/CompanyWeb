using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabybusSSApi.DatabaseModel;
using BabyBusSSApi.ServiceInterface.Services;
using BabyBusSSApi.ServiceInterface.Utilities;
using BabyBusSSApi.ServiceModel.DTO.Create;
using BabyBusSSApi.ServiceModel.DTO.Get;
using BabyBusSSApi.ServiceModel.Enumeration;
using Foundation.ServiceInterface.Utilities;
using Foundation.ServiceModel.Kindergartens;
using ServiceStack;
using ServiceStack.OrmLite;

namespace Foundation.ServiceInterface.Services
{
    public class KindergartenServices : Service
    {
        public IAutoQuery AutoQuery { get; set; }

        public object Get(QueryKindergartens request)
        {
            var ReliableUserSession = this.GetSession() as ReliableUserSession;
            Debug.Assert(ReliableUserSession != null, "ReliableUserSession != null");

            request.Cancel = false;
            if (ReliableUserSession.RoleType != RoleType.Admin && ReliableUserSession.RoleType != RoleType.Parent && ReliableUserSession.RoleType != RoleType.SuperHeadMaster)
            {
                request.KindergartenId = ReliableUserSession.KindergartenId;
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());

            return AutoQuery.Execute(request, q);
        }

        public object Post(CreateKindergarten request)
        {
            var babybusSession = this.GetSession() as ReliableUserSession;
            var kindergarten = request.ConvertTo<Kindergarten>();
            Db.Save(kindergarten);

            var kindergartenAreaCode = request.RegionCode.Substring(0, 4);

            //Init Kindergarten Code
            var initCodeService = new InitCodeService();
            initCodeService.InitGroupCode();
            initCodeService.InitKindergartenCode(kindergartenAreaCode);

            //Create Default User
            var userService = new UsersService();
            var kg = Db.SingleById<Kindergarten>(kindergarten.KindergartenId);
            if (kg != null && !string.IsNullOrEmpty(kg.Code))
            {
                var kgUser = new CreateUser
                {
                    KindergartenId = kg.KindergartenId,
                    GroupId = kg.GroupId,
                    LoginName = kg.Code,
                    RoleType = RoleType.HeadMaster,
                    Roles = new List<string> { "HeadMaster" },
                    Password = "123456",
                    RealName = kg.KindergartenName.Trim()
                };
                userService.Post(kgUser);
            }
            if (request.AgencyId != null)
            {
                Db.Insert(new PA_AgencyAndKgRelation { CreateTime = System.DateTime.Now, AgencyId = request.AgencyId.Value, KindergartenId = kindergarten.KindergartenId });
            }
            var content = babybusSession.LoginName + "创建幼儿园，ID:" + kindergarten.KindergartenId;
            Db.DBExpand(1, 4, content, babybusSession.UserId, babybusSession.LoginName);
            return kindergarten.KindergartenId;
        }


        public bool Get(CheckKindergartenName request)
        {
            var information = 0;
            if (request.KindergartenId != null)
            {
                information =
                    Db.Select<Kindergarten>(
                        Db.From<Kindergarten>()
                            .Where(
                                s =>
                                    s.RegionCode == request.RegionCode 
                                    && s.KindergartenName == request.KindergartenName 
                                    && s.KindergartenId != request.KindergartenId
                                    && !s.Cancel))
                        .Count;
            }
            else
            {
                information =
                    Db.Select<Kindergarten>(
                        Db.From<Kindergarten>()
                            .Where(
                                s =>
                                    s.RegionCode == request.RegionCode 
                                    && s.KindergartenName == request.KindergartenName
                                    && !s.Cancel))
                        .Count;
            }
            if (information == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Put(UpdateKindergarten request)
        {
            var babybusSession = this.GetSession() as ReliableUserSession;
            var kindergarten = Db.SingleById<Kindergarten>(request.KindergartenId);

            kindergarten.Address = request.Address ?? kindergarten.Address;
            kindergarten.City = request.City ?? kindergarten.City;
            kindergarten.Description = request.Description ?? kindergarten.Description;
            kindergarten.KindergartenName = request.KindergartenName ?? kindergarten.KindergartenName;
            kindergarten.RegionCode = request.RegionCode ?? kindergarten.RegionCode;
            kindergarten.RegionName = request.RegionName ?? kindergarten.RegionName;
            Db.Update(kindergarten);
            Db.Delete<PA_AgencyAndKgRelation>(q => q.KindergartenId == request.KindergartenId);
            Db.Insert(new PA_AgencyAndKgRelation { CreateTime = System.DateTime.Now,AgencyId = request.AgencyId.Value, KindergartenId = (int)request.KindergartenId});
            Db.ExecuteNonQuery("EXEC dbo.UP_PA_CalcAgencyServiceInfo");
            var content = babybusSession.LoginName + "修改幼儿园信息，ID:" + request.KindergartenId;
            Db.DBExpand(2, 4, content, babybusSession.UserId, babybusSession.LoginName);
        }

        public void Delete(DeleteKindergarten request)
        {
            var kindergarten = request.ConvertTo<Kindergarten>();
            kindergarten.Cancel = true;
            Db.UpdateOnly(kindergarten,
               onlyFields: p => new { p.Cancel },
               where: p => p.KindergartenId == request.Id);
            var babybusSession = this.GetSession() as ReliableUserSession;
            var content = babybusSession.LoginName + "删除【" + kindergarten.KindergartenName + "】,ID:" +
                          kindergarten.KindergartenId;
            Db.DBExpand(3, 4, content, babybusSession.UserId, babybusSession.LoginName);
        }

        //Not Used
#if false
        public object Post(UpdateGroupKindergarten request)
        {
            var babybusSession = this.GetSession() as ReliableUserSession;
            var info = request.ConvertTo<Kindergarten>();
            info.GroupId = request.GroupId;
            for (var i = 0; i < (request.KindergartenId).Length; i++)
            {
                Db.UpdateOnly(info,
               onlyFields: p => new { p.GroupId },
               where: p => p.KindergartenId == (request.KindergartenId)[i]);
            }
            var content = babybusSession.LoginName + "更新幼儿园所属集团，ID:" + request.KindergartenId;
            Db.DBExpand(2, 4, content, babybusSession.UserId, babybusSession.LoginName);
            return true;
        }
#endif
    }
}
