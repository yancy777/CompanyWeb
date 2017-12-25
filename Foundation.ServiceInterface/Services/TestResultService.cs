using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabybusSSApi.DatabaseModel;
using Foundation.ServiceModel.TestResult;
using Funq;
using ServiceStack;
using ServiceStack.OrmLite;

namespace Foundation.ServiceInterface.Services
{
    public class TestResultService: Service
    {
        public IAutoQuery AutoQuery { get; set; }
        public TestResultCheckResponse Get(TestResult request)
        {
            var plan =
                Db.Select(Db.From<RT_TestPlan>().Where(p => p.KindergartenId == request.KindergartenId))
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault();
            var childList =
                Db.Select(Db.From<UV_Child_Class_Kdg>().Where(c => c.Cancel == false && c.KindergartenId == request.KindergartenId));
            var testList =
                Db.Select(
                    Db.From<UV_RT_TestResult>()
                        .Where(p => p.KindergartenId == request.KindergartenId && p.PlanId == plan.Id));
            var result = new TestResultCheckResponse();
            var childIdArr = new ArrayOfInt();
            foreach (var item in testList)
            {
                childIdArr.Add(item.ChildId);
            }
            result.ChildNoTest = new List<UV_Child_Class_Kdg>();
            foreach (var itemChild in childList)
            {
                if (childIdArr.Contains(itemChild.ChildId))
                {
                    continue;
                }
                result.ChildNoTest.Add(itemChild);
            }
            result.TestResult = testList;
            return result;
        }
    }
}
