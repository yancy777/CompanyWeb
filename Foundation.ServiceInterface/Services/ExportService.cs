using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation.ServiceModel.Export;
using ServiceStack;
using ServiceStack.OrmLite;
using BabybusSSApi.DatabaseModel;
using BabyBusSSApi.ServiceModel;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using ServiceStack.Text;

namespace Foundation.ServiceInterface.Services
{
    public class ExportService:Service
    {
        public  IAutoQuery AutoQuery { get; set; }

        public string Get(ServiceUnitFamily request)
        {
            var physicalResult =
                Db.Select(
                    Db.From<RT2_Result>()
                        .Where(t => t.ServiceUnitId == request.ServiceUnitId && t.ChildId == request.ChildId)
                        .OrderByDescending(t => t.Id)).FirstOrDefault();
            if (physicalResult == null)
            {
                return "没有孩子体测信息";
            }else if (physicalResult.Height == null || physicalResult.Weight == null || physicalResult.BroadJump == null ||
                      physicalResult.DoubleJump == null || physicalResult.SitReach == null ||
                      physicalResult.ShuttleRun == null || physicalResult.Balance == null || physicalResult.Throw == null)
            {
                return "测试未完成，不能导出报告";
            }
            else
            {
                string[] regionArr = { "110000", "120000", "500000", "310000" };

                var serviceUnitInfo =
                    Db.Select<DB_ServiceUnit>(
                        Db.From<DB_ServiceUnit>()
                            .Where(ak => ak.Id == request.ServiceUnitId)
                            .OrderByDescending(ag => ag.CreateTime)).FirstNonDefault();

                //                var kindergarProvince = kindergartenInfo.RegionCode.Substring(0, 6);
                var unitProvince = serviceUnitInfo.RegionCode.Substring(serviceUnitInfo.RegionCode.Length - 6, 2) + "0000";
                var unitArea = "";
                if (regionArr.Contains(unitProvince))
                {
                    //                    kindergartenArea = kindergartenInfo.RegionCode.Substring(12, 6);
                    unitArea = serviceUnitInfo.RegionCode.Substring(serviceUnitInfo.RegionCode.Length - 6, 2) + "0000";
                }
                else
                {
                    //                    kindergartenArea = kindergartenInfo.RegionCode.Substring(6, 6);
                    unitArea =
                        serviceUnitInfo.RegionCode.Substring(serviceUnitInfo.RegionCode.Length - 6, 4) + "00";
                }
                var provinceCode = Db.SingleById<Region>(unitProvince);
                var areaCode = Db.SingleById<Region>(unitArea);
                serviceUnitInfo.City = provinceCode.RegionName + areaCode.RegionName;

                var weightStand = Db.Select<BMIStandard>(Db.From<BMIStandard>());
                var sevenObjStand = Db.Select<IndividualIndexDistribution>(Db.From<IndividualIndexDistribution>());
                Dictionary<string, object> resultInfo = new Dictionary<string, object>();
                resultInfo.Add("PhyResultInfo", physicalResult);
                resultInfo.Add("WeightStand", weightStand);
                resultInfo.Add("SevenObjStand", sevenObjStand);
                resultInfo.Add("ServiceUnitInfo", serviceUnitInfo);
                var json = JsonSerializer.SerializeToString(resultInfo);
                var path = AppConfig.RootDirectory + "ExportMIReport/";
                var demolPath = AppConfig.DemolDirectory + "ExportAgencyFamily.html";
                var fileContent = File.ReadAllText(demolPath);
                var textVal = fileContent.Contains("$information$")
                    ? fileContent.Replace("$information$", json)
                    : fileContent;
                var reportPath = path + physicalResult.ChildName + "体测报告.html";
                File.WriteAllText(@reportPath, textVal);
            }
         
            return "导出成功";
        }

        //public string Get(ServiceUnitFamily2 request)
        //{
        //    var masterResult = new List<RT_ResultMaster>();
        //    var oldProgramId = request.ProgramId;
        //    if (request.ProgramId == null)
        //    {
        //        if (request.ChildIdArr != null)
        //        {
        //            var masterList =
        //                Db.Select(
        //                    Db.From<RT_ResultMaster>()
        //                        .Where(m => m.ChildId == request.ChildIdArr && m.TestStatus == true)
        //                        .OrderByDescending(m => m.CreateTime)
        //                        .Take(1)).FirstOrDefault();
        //            var programInfo =
        //                Db.Select(Db.From<RT_Program>().Where(p => p.Id == masterList.ProgramId)).FirstOrDefault();
        //            if (programInfo != null && programInfo.Type != 0)
        //            {
        //                masterResult =
        //                    Db.Select(
        //                        Db.From<RT_ResultMaster>()
        //                            .Where(m => m.ChildId == request.ChildIdArr && m.TestStatus == true).OrderByDescending(m => m.CreateTime).Take(1));
        //            }
        //        }
        //        if (request.Id != null)
        //        {
        //            masterResult = Db.Select(Db.From<RT_ResultMaster>().Where(m => m.Id == request.Id && m.TestStatus == true));
        //        }
        //        if (masterResult.Count > 0)
        //        {
        //            request.ProgramId = masterResult[0].ProgramId;
        //        }
        //    }
        //    else
        //    {
        //        if (request.ChildIdArr != null)
        //        {
        //            masterResult =
        //                Db.Select(
        //                    Db.From<RT_ResultMaster>()
        //                        .Where(m => m.ProgramId == request.ProgramId && m.ChildId == request.ChildIdArr && m.TestStatus == true));
        //        }
        //        else
        //        {
        //            masterResult =
        //               Db.Select(
        //                   Db.From<RT_ResultMaster>()
        //                       .Where(m => m.ProgramId == request.ProgramId && m.TestStatus == true));
        //        }
        //    }
        //    if (masterResult.Count > 0)
        //    {
        //        var programInfo = Db.Select(Db.From<RT_Program>().Where(p => p.Id == request.ProgramId));
        //        var serviceUnitInfo =
        //            Db.Select(Db.From<DB_ServiceUnit>().Where(s => s.Id == masterResult[0].ServiceUnitId)).FirstOrDefault();
        //        var result = from r in masterResult
        //            select new
        //            {
        //                master = r,
        //                detail = Detail(r.Id),
        //                childCode = r.ChildId ==0?null: ChildInfo(r.ChildId)
        //            };
        //        var standard = Db.Select(Db.From<RT_Standard>());
        //        var testResult = new Dictionary<object,object>();
        //        testResult.Add("Program",programInfo);
        //        testResult.Add("Standard",standard);
        //        testResult.Add("TestResult",result);
        //        testResult.Add("ServiceUnit",serviceUnitInfo);
        //        var json = JsonSerializer.SerializeToString(testResult);
        //        var path = AppConfig.RootDirectory + "ExportMIReport/";
        //        var demolPath = AppConfig.DemolDirectory + "BFamilyReportDemo.html";
        //        var fileContent = File.ReadAllText(demolPath);
        //        var textVal = fileContent.Contains("$information$")
        //            ? fileContent.Replace("$information$", json)
        //            : fileContent;
        //        var fileName = oldProgramId != null ? (request.ChildIdArr != null? ChildInfo(request.ChildIdArr.Value).ChildName:masterResult[0].ServiceUnitName) : masterResult[0].ChildName;
        //        var reportPath = path + fileName + "体测报告.html";
        //        File.WriteAllText(@reportPath, textVal);
        //        return "导出成功";

        //    }
        //    else
        //    {
        //        return "导出失败，没有孩子体测信息或测试未完成！";
        //    }

        //}


        /*B产品家长报告导出，两次对比，睿莱导出*/
        public string Get(ServiceFamilyContrast request)
        {
            var masterResult = new List<RT_ResultMaster>();
            var oldProgramId = request.ProgramId;
            if (request.ProgramId == null)
            {
                if (request.ChildIdArr != null)
                {
                    masterResult =
                    Db.Select(
                        Db.From<RT_ResultMaster>()
                            .Where(m => m.ChildId == request.ChildIdArr && m.TestStatus == true).OrderByDescending(m => m.CreateTime).Take(1));
                }
                if (request.Id != null)
                {
                    masterResult = Db.Select(Db.From<RT_ResultMaster>().Where(m => m.Id == request.Id && m.TestStatus == true));
                }
                if (masterResult.Count > 0)
                {
                    request.ProgramId = masterResult[0].ProgramId;
                }
            }
            else
            {
                if (request.ChildIdArr != null)
                {
                    var testMaster = Db.Select(
                        Db.From<RT_ResultMaster>()
                            .Where(
                                m =>
                                    m.ProgramId == request.ProgramId && m.ChildId == request.ChildIdArr &&
                                    m.TestStatus == true)).FirstOrDefault();

                    masterResult =
                        Db.Select(
                            Db.From<RT_ResultMaster>()
                                .Where(
                                    s =>
                                        s.ChildId == testMaster.ChildId && s.CreateTime <= testMaster.CreateTime &&
                                        s.TestStatus == true)
                                .OrderByDescending(s => s.CreateTime)
                                .Take(2));
                }
                else
                {
                     masterResult = 
                        Db.Select(
                           Db.From<RT_ResultMaster>()
                               .Where(m => m.ProgramId == request.ProgramId && m.TestStatus == true));
                    //var masterLinq = from s in masterList select new
                    //{
                    //    everyChild = Db.Select(Db.From<RT_ResultMaster>().Where( m => m.TestStatus == true && m.CreateTime <= s.CreateTime && m.ChildId == s.ChildId).OrderByDescending( m => m.CreateTime).Take(2)),
                    //};
                    //masterResult.AddRange(masterLinq.SelectMany(master => master.everyChild));
                }
            }
            if (masterResult.Count > 0)
            {
                var programInfo = Db.Select(Db.From<RT_Program>().Where(p => p.Id == request.ProgramId));
                var serviceUnitInfo =
                    Db.Select(Db.From<DB_ServiceUnit>().Where(s => s.Id == masterResult[0].ServiceUnitId)).FirstOrDefault();
                var result = ExportData(masterResult);
                //var weightStandard = Db.Select(Db.From<BMIStandard>());
                var testResult = new Dictionary<object, object>();
                testResult.Add("Program", programInfo);
                testResult.Add("TestResult", result);
                testResult.Add("ServiceUnit", serviceUnitInfo);
                if (serviceUnitInfo.UnitType == 3)
                {
                    var weightStandard = Db.Select(Db.From<BMIStandard>());
                    testResult.Add("WeightStandard", weightStandard);
                }
                var standard = Db.Select(Db.From<RT_Standard>());
                testResult.Add("Standard", standard);
                var json = JsonSerializer.SerializeToString(testResult);
                var path = AppConfig.RootDirectory + "ExportMIReport/";
                var demolPath = AppConfig.DemolDirectory + "BFamilyReportDemo.html";
                var fileContent = File.ReadAllText(demolPath);
                var textVal = fileContent.Contains("$information$")
                    ? fileContent.Replace("$information$", json)
                    : fileContent;
                var fileName = oldProgramId != null ? (request.ChildIdArr != null ? masterResult[0].ChildName : masterResult[0].ServiceUnitName) : masterResult[0].ChildName;
                var reportPath = path + fileName + "体测报告.html";
                File.WriteAllText(@reportPath, textVal);
                return "导出成功";

            }
            else
            {
                return "导出失败，没有孩子体测信息或测试未完成！";
            }

        }
        /*B产品家长报告导出，Ikid导出*/

        public string Get(ServiceFamilyIkidContrast request)
        {
            var masterResult = new List<RT_ResultMaster>();
            var oldProgramId = request.ProgramId;
            if (request.ProgramId == null)
            {
                if (request.ChildIdArr != null)
                {
                    masterResult =
                    Db.Select(
                        Db.From<RT_ResultMaster>()
                            .Where(m => m.ChildId == request.ChildIdArr && m.TestStatus == true).OrderByDescending(m => m.CreateTime).Take(1));
                }
                if (request.Id != null)
                {
                    masterResult = Db.Select(Db.From<RT_ResultMaster>().Where(m => m.Id == request.Id && m.TestStatus == true));
                }
                if (masterResult.Count > 0)
                {
                    request.ProgramId = masterResult[0].ProgramId;
                }
            }
            else
            {
                if (request.ChildIdArr != null)
                {
                    var testMaster = Db.Select(
                        Db.From<RT_ResultMaster>()
                            .Where(
                                m =>
                                    m.ProgramId == request.ProgramId && m.ChildId == request.ChildIdArr &&
                                    m.TestStatus == true)).FirstOrDefault();

                    masterResult =
                        Db.Select(
                            Db.From<RT_ResultMaster>()
                                .Where(
                                    s =>
                                        s.ChildId == testMaster.ChildId && s.CreateTime <= testMaster.CreateTime &&
                                        s.TestStatus == true)
                                .OrderByDescending(s => s.CreateTime)
                                .Take(2));
                }
                else
                {
                    masterResult =
                       Db.Select(
                          Db.From<RT_ResultMaster>()
                              .Where(m => m.ProgramId == request.ProgramId && m.TestStatus == true));
                    //var masterLinq = from s in masterList select new
                    //{
                    //    everyChild = Db.Select(Db.From<RT_ResultMaster>().Where( m => m.TestStatus == true && m.CreateTime <= s.CreateTime && m.ChildId == s.ChildId).OrderByDescending( m => m.CreateTime).Take(2)),
                    //};
                    //masterResult.AddRange(masterLinq.SelectMany(master => master.everyChild));
                }
            }
            if (masterResult.Count > 0)
            {
                var programInfo = Db.Select(Db.From<RT_Program>().Where(p => p.Id == request.ProgramId));
                var serviceUnitInfo =
                    Db.Select(Db.From<DB_ServiceUnit>().Where(s => s.Id == masterResult[0].ServiceUnitId)).FirstOrDefault();
                var result = ExportData(masterResult);
                //var weightStandard = Db.Select(Db.From<BMIStandard>());
                var testResult = new Dictionary<object, object>();
                testResult.Add("Program", programInfo);
                testResult.Add("TestResult", result);
                testResult.Add("ServiceUnit", serviceUnitInfo);
                if (serviceUnitInfo.UnitType == 3)
                {
                    var weightStandard = Db.Select(Db.From<BMIStandard>());
                    testResult.Add("WeightStandard", weightStandard);
                }
                var standard = Db.Select(Db.From<RT_Standard>());
                testResult.Add("Standard", standard);
                var json = JsonSerializer.SerializeToString(testResult);
                var path = AppConfig.RootDirectory + "ExportMIReport/";
                var demolPath = AppConfig.DemolDirectory + "BFamilyIkidReportDemo.html";
                var fileContent = File.ReadAllText(demolPath);
                var textVal = fileContent.Contains("$information$")
                    ? fileContent.Replace("$information$", json)
                    : fileContent;
                var fileName = oldProgramId != null ? (request.ChildIdArr != null ? masterResult[0].ChildName : masterResult[0].ServiceUnitName) : masterResult[0].ChildName;
                var reportPath = path + fileName + "体测报告.html";
                File.WriteAllText(@reportPath, textVal);
                return "导出成功";

            }
            else
            {
                return "导出失败，没有孩子体测信息或测试未完成！";
            }
        }
     
        private List<UV_RT_RestultDetail_Project> Detail(long masterId)
        {
            var detail = Db.Select(Db.From<UV_RT_RestultDetail_Project>().Where(d => d.MasterId == masterId));
            return detail;
        }

        private DB_Child ChildInfo(long childId)
        {
            var info = Db.Select(Db.From<DB_Child>().Where(c => c.Id == childId)).FirstOrDefault();
            return info;
        }

        private Dictionary<long, object> ExportData(List<RT_ResultMaster> master)
        {
            var results = new Dictionary<long, object>();
            foreach (var item in master)
            {
                var testResult =
                    Db.Select(Db.From<RT_ResultMaster>().Where(m => m.Id <= item.Id && m.ChildId == item.ChildId && m.TestStatus == true).OrderByDescending(m => m.Id).Take(2));
                var result = from r in testResult
                             select new
                             {
                                 master = r,
                                 detail = Detail(r.Id),
                                 childCode = r.ChildId == 0 ? null : ChildInfo(r.ChildId)
                             };
                results.Add(item.ChildId,result);
            }
            return results;
        }
    }
}
