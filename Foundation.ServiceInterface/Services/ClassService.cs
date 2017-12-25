using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabybusSSApi.DatabaseModel;
using Foundation.ServiceModel.Classes;
using ServiceStack;
using ServiceStack.OrmLite;

namespace Foundation.ServiceInterface.Services
{
    public class ClassService:Service
    {

        public bool Put(CreateClass request)
        {
            Db.Insert(new DB_Class { CreateTime = System.DateTime.Now, ServiceUnitId = request.ServiceUnitId, Name = request.Name, ClassType = 0, Count = 0, Cancel = false, GUID = Guid.NewGuid().ToString()});
            return true;
        }

        public bool Post(CreateUnitClass request)
        {
            var classInfo = new DB_Class
            {
                CreateTime = System.DateTime.Now,
                ServiceUnitId = request.ServiceUnitId,
                Name =  request.Name,
                ClassType = request.ClassType,
                Count = 0,
                Description = request.Description,
                Cancel = false
            };
            Db.Save(classInfo);
            if (request.TeacherId != null)
            {
                Db.UpdateOnly(new DB_Teacher { ClassId = (int)classInfo.Id }, onlyFields: u => u.ClassId,
                where: u => u.Id == request.TeacherId);
            }
            
            return true;
        }

        public bool Put(UpdateUnitClass request)
        {
            if (request.TeacherId != null )
            {
                
                Db.UpdateOnly(new DB_Teacher { ClassId = request.Id }, onlyFields: u => u.ClassId,
                where: u => u.Id == request.TeacherId);
            }
            if (request.OldTeacherId != null)
            {
                Db.UpdateOnly(new DB_Teacher { ClassId = 0 }, onlyFields: u => u.ClassId,
                where: u => u.Id == request.OldTeacherId);
            }
           
            Db.UpdateOnly(
                new DB_Class {Name = request.Name, ClassType = request.ClassType, Description = request.Description},
                onlyFields: c => new {c.Name,c.ClassType,c.Description}, where: c => c.Id == request.Id);
            return true;
        }

        public bool Put(DeleteUnitClass request)
        {
            var deleteClassInfo = Db.SingleById<DB_Class>(request.Id);
            Db.UpdateOnly(new DB_Class {Cancel = true}, onlyFields: c => c.Cancel, where: c => c.Id == request.Id);
            var serviceUnit = Db.SingleById<DB_ServiceUnit>(deleteClassInfo.ServiceUnitId);
            serviceUnit.Count -= deleteClassInfo.Count;
            Db.Save(serviceUnit);
            return true;
        }

        public object Get(ClassList request)
        {
            var classList =
                Db.Select(Db.From<Class>().Where(c => c.KindergartenId == request.KindergartenId && c.Cancel == false));

            var childList =
                Db.Select(Db.From<Child>().Where(c => c.KindergartenId == request.KindergartenId && c.Cancel == false));
            var result = from l in classList select new
            {
                ClassId = l.Id,
                ClassName = l.ClassName,
                ClassType = l.ClassType,
                ClassCount = (from c in childList where c.ClassId == l.Id select c).Count()
            };
            return result;
        }
        //public object Get(ClassList request)
        //{
        //    var classList =
        //        Db.Select(Db.From<Class>().Where(c => c.KindergartenId == request.KindergartenId && c.Cancel == false));

        //    var childList =
        //        Db.Select(Db.From<Child>().Where(c => c.KindergartenId == request.KindergartenId && c.Cancel == false));
        //    var result = from l in classList
        //                 select new
        //                 {
        //                     ClassId = l.Id,
        //                     ClassName = l.ClassName,
        //                     ClassType = l.ClassType,
        //                     ClassCount = (from c in childList where c.ClassId == l.Id select c).Count()
        //                 };
        //    return result;
        //}
    }
}
