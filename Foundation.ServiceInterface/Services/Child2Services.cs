using System;
using System.Linq;
using BabybusSSApi.DatabaseModel;
using BabyBusSSApi.ServiceInterface.Utilities;
using BabyBusUtilities.Helper;
using Foundation.ServiceInterface.Utilities;
using Foundation.ServiceModel.Children;
using ServiceStack;
using ServiceStack.OrmLite;

namespace Foundation.ServiceInterface.Services
{
    public class Child2Services : Service
    {
        public IAutoQuery AutoQuery { get; set; }

        public bool Post(CreateChild request)
        {
            request.Birthday = request.Birthday.Value.ToLocalTime();
            var child = new DB_Child
            {
                Birthday = request.Birthday,
                CreateTime = DateTime.Now,
                Cancel = request.Cancel,
                ClassId = request.ClassId,
                Gender = request.Gender,
                ChildName = request.ChildName,
                ServiceUnitId = request.ServiceUnitId,
                ImageName = request.ImageName,
                Code = InitCodeHelper.GenerateChild2Code(Db,request.ServiceUnitId),
                Phone = request.Phone,
                ResponsibleTeacher = request.ResponsibleTeacher,
                GUID = Guid.NewGuid().ToString()
            };
            Db.Save(child);
            if(request.ClassId != 0 )
            {
                var relationClass = Db.SingleById<DB_Class>(child.ClassId);
                var relationService = Db.SingleById<DB_ServiceUnit>(child.ServiceUnitId);
                relationClass.Count += 1;
                relationService.Count += 1;
                Db.UpdateOnly(new DB_ServiceUnit {Count = relationService.Count}, onlyFields: u => u.Count,
                    where: u => u.Id == relationService.Id);
                Db.UpdateOnly(new DB_Class {Count = relationClass.Count}, onlyFields: c => c.Count,
                    where: c => c.Id == relationClass.Id);
            }
            return true;
        }

        public bool Put(UpdateChild request)
        {
            Db.UpdateOnly(
                new DB_Child
                {
                    ChildName = request.ChildName,
                    Gender = request.Gender,
                    Birthday = request.Birthday.Value.ToLocalTime(),
                    Phone = request.Phone,
                    ClassId = request.ClassId,
                    ResponsibleTeacher = request.ResponsibleTeacher
                }, onlyFields: c => new {c.ChildName, c.Gender, c.Birthday,c.Phone,c.ClassId,c.ResponsibleTeacher },
                where: c => c.Id == request.Id);
            if (request.OldClassId != null && request.OldClassId != request.ClassId)
            {
                var newClassInfo = Db.SingleById<DB_Class>(request.ClassId);
                var oldClassInfo = Db.SingleById<DB_Class>(request.OldClassId);
                Db.UpdateOnly(new DB_Class {Count = newClassInfo.Count + 1}, onlyFields: c => c.Count,
                    where: c => c.Id == request.ClassId);
                Db.UpdateOnly(new DB_Class { Count = oldClassInfo.Count - 1 }, onlyFields: c => c.Count,
                    where: c => c.Id == request.OldClassId);
            }
            return true;
        }

        public bool Put(CancelChild request)
        {
            var childInfo = Db.SingleById<DB_Child>(request.Id);
            Db.UpdateOnly( new DB_Child {Cancel = true},onlyFields: p => p.Cancel,  where: c => c.Id == request.Id);
            var classInfo = Db.SingleById<DB_Class>(childInfo.ClassId);
            var serviceInfo = Db.SingleById<DB_ServiceUnit>(childInfo.ServiceUnitId);
            if (classInfo.Count > 0)
            {
                Db.UpdateOnly(new DB_Class { Count = classInfo.Count -1}, onlyFields: c => c.Count, where: c => c.Id == classInfo.Id);
            }
            if (serviceInfo.Count > 0)
            {
                Db.UpdateOnly(new DB_ServiceUnit { Count = serviceInfo.Count - 1 }, onlyFields: c => c.Count, where: c => c.Id == serviceInfo.Id);
            }
            return true;
        }

        public object Get(SearchChild request)
        {
            var childInfo =
                Db.Select(
                    Db.From<DB_Child>()
                        .Where(c => c.ChildName.Contains(request.ChildName) && c.ServiceUnitId == request.ServiceUnitId));
            return childInfo;
        }

        public object Any(BindChild request)
        {
//            var ReliableUserSession = this.GetSession() as ReliableUserSession;
//            var userId = ReliableUserSession.UserId;
            var userId = request.UserId;
            if (!string.IsNullOrEmpty(request.ChildCode))
            {
                var child = Db.Select<DB_Child>(p => p.Code == request.ChildCode).FirstOrDefault();
                if (child != null)
                {
                    request.ChildId = child.Id;
                }
                else
                {
                    return 2;
                }
            }
         

            var isExist = false;
            var pcrList = Db.Select<ParentChildRelation>(p => p.UserId == userId);

            foreach (var pcr in pcrList)
            {
                if (pcr.ChildId == request.ChildId)
                {
                    isExist = true;
                    pcr.IsSelected = true;
                }
                else
                {
                    pcr.IsSelected = false;
                }
                Db.Save(pcr);
            }
            if (!isExist)
            {
                var pcr = new ParentChildRelation();
                pcr.UserId = (int)userId;
                pcr.ChildId = (int)request.ChildId;
                pcr.IsSelected = true;
                Db.Save(pcr);
                return pcr.Id;
            }
//            var content = ReliableUserSession.LoginName + "绑定宝贝，用户ID:" + ReliableUserSession.UserId;
//            Db.DBExpand(2, 1, content, ReliableUserSession.UserId, ReliableUserSession.LoginName);
            return 1;
        }

        public bool Post(ServiceUnitUpdateChild request)
        {
            Db.UpdateOnly(
                new DB_Child {Birthday = request.Birthday, Gender = request.Gender, ChildName = request.ChildName},
                onlyFields: c => new {c.Birthday, c.Gender, c.ChildName}, where: c => c.Id == request.Id);
            return true;
        }

        public bool Post(UnbindChild request)
        {
            var pcr = Db.Single<ParentChildRelation>(p =>
                p.UserId == request.UserId && p.ChildId == request.ChildId);
            if (pcr != null)
            {
                Db.Delete(pcr);
                var parentRelation = Db.Select(Db.From<ParentChildRelation>().Where( p => p.UserId == request.UserId));
                if (parentRelation.Count > 0)
                {
                    Db.UpdateOnly(new ParentChildRelation {IsSelected = true},onlyFields: p => p.IsSelected,where: p => p.Id == parentRelation[0].Id);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
