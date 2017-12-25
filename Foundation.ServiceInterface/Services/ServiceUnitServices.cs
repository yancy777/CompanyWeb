using System;
using System.Collections.Generic;
using System.Linq;
using BabybusSSApi.DatabaseModel;
using BabyBusSSApi.ServiceInterface.Utilities;
using BabyBusSSApi.ServiceModel.DTO.Create;
using Foundation.ServiceModel.ServiceUnits;
using ServiceStack;
using ServiceStack.OrmLite;
using BabyBusSSApi.ServiceModel.Enumeration;
using ServiceStack.Auth;

namespace Foundation.ServiceInterface.Services
{
    public class ServiceUnitServices : Service
    {
        public IAutoQuery AutoQuery { get; set; }

        readonly CreateUserHelper _createUserHelper;
        public ServiceUnitServices()
        {
            _createUserHelper = new CreateUserHelper();
        }
        public bool Post(UpdateClass request)
        {
            Db.UpdateOnly(new DB_Class {Name = request.Name}, onlyFields: p => p.Name, where: p => p.Id == request.Id);
            return true;
        }

        public object Get(ChildList request)
        {
            var childList =
                Db.Select(
                    Db.From<UV_DB_Child_Class>()
                        .Where(c => !c.Cancel && c.ServiceUnitId == request.ServiceUnitId && c.ClassId != -1)
                        .OrderByDescending(c => c.Id));
            return childList;
        }

        public object Get(ClassList request)
        {
            request.Cancel = false;
            var classList = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, classList);
        }

        public bool Post(DeleteClass request)
        {
            var childCount = Db.Select(Db.From<DB_Child>().Where(c => c.ClassId == request.Id && c.Cancel == false));
            if (childCount.Count <= 0)
            {
                Db.UpdateOnly(new DB_Class {Cancel = true}, onlyFields: c => c.Cancel, where: c => c.Id == request.Id);
                return true;
            }
            return false;
        }

        public bool Post(CreateTeacher request)
        {
            Db.Insert(new DB_Teacher { CreateTime = System.DateTime.Now,ServiceUnitId = request.ServiceUnitId,ClassId = 0,TeacherName = request.TeacherName,PhoneNumber = request.PhoneNumber,Cancel = false});
            return true;
        }

        public bool Get(CheckCreatePhoneNumber request)
        {
            var result = true;
            int teacherCount;
            if (request.Id != null)
            {
                teacherCount =
                    Db.Select(
                        Db.From<DB_Teacher>()
                            .Where(t => t.PhoneNumber == request.PhoneNumber && t.Id != request.Id.Value && t.Cancel == false)).Count;
                if (teacherCount > 0)
                {
                    result = false;
                }
            }
            else
            {
                teacherCount =
                   Db.Select(Db.From<DB_Teacher>().Where(t => t.PhoneNumber == request.PhoneNumber && !t.Cancel)).Count;
                if (teacherCount > 0)
                {
                    result = false;
                }
            }
            return result;
        }

        public bool Post(UpdateTeacher request)
        {
            Db.UpdateOnly(new DB_Teacher {PhoneNumber = request.PhoneNumber, TeacherName = request.TeacherName},
                onlyFields: t =>new
                {
                    t.PhoneNumber,
                    t.TeacherName
                }, where: t => t.Id == request.Id);
            return true;
        }

        public bool Post(Delete request)
        {
            Db.UpdateOnly(new DB_Teacher {Cancel = true,ClassId = 0}, onlyFields: t => new { t.Cancel,t.ClassId}, where: t => t.Id == request.Id);
            return true;
        }

        /*B产品创建机构*/
        public string Post(Create request)
        {
            using (var dbTrans = Db.OpenTransaction())
            {
                var code = request.RegionCode.Substring(0, 4);
                var serviceUnitArr = Db.Select(Db.From<DB_ServiceUnit>().Where(s => s.Code.Contains(code) ).OrderByDescending(s => s.Id)).FirstOrDefault();
                var upCode = "0001";
                var rank = new Random();
                if (serviceUnitArr != null)
                {
                    upCode = (int.Parse(serviceUnitArr.Code.Substring(6, 4)) + 1).ToString("0000");
                }

                var serviceUnit = new DB_ServiceUnit
                {
                    Name = request.Name,
                    CreateTime = System.DateTime.Now,
                    City = request.City,
                    Cancel = false,
                    UnitType = request.UnitType,
                    Code = "su" + code + upCode + rank.Next(0, 99).ToString("00"),
                    Count = 0,
                    Description = request.Description,
                    RegionCode = request.RegionCode,
                    Address = request.Address
                };
                Db.Save(serviceUnit);
                var roles = new List<string>();
                roles.Add("Agency2");
                var user = new CreateUser
                {
                    LoginName = serviceUnit.Code,
                    Password = "123456",
                    RealName = serviceUnit.Name,
                    Roles = roles,
                    RoleType = RoleType.Agency2,
                    CooperatedId = (int)serviceUnit.Id
                };
                var authRepo = TryResolve<IUserAuthRepository>();
                var iUserAuth = _createUserHelper.CreateUser(user, authRepo, Db);
                var tolerantClass = new DB_Class
                {
                    Name = "未分班",
                    ServiceUnitId = serviceUnit.Id,
                    CreateTime = System.DateTime.Now,
                    ClassType = 1,
                    Cancel = false,
                    Count = 0
                };
                var tolerantProgram = new RT_Program
                {
                    ServiceUnitId = serviceUnit.Id,
                    Title = "营销计划",
                    Type = 0,
                    CreateTeacher = request.Name,
                    Status = false,
                    Cancel = false,
                    Count = 0,
                    CreateTime = System.DateTime.Now
                };
                Db.Save(tolerantProgram);
                Db.Save(tolerantClass);
                dbTrans.Commit();
                return serviceUnit.Code;
            }
        }
       
    }
}
