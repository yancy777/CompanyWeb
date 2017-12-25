using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabybusSSApi.DatabaseModel;
using Foundation.ServiceModel.Agencies;
using ServiceStack;
using ServiceStack.OrmLite;

namespace Foundation.ServiceInterface.Services
{
    public class AgencyServices : Service
    {
        public object Get(GetAgencyIdByMD5 request)
        {
            var resp = new IdByMD5Response();
            var agency = Db.Select(Db.From<PA_Agency>().Where(x => x.MD5 == request.MD5)).FirstOrDefault();
            var alliance = Db.Select(Db.From<AB_AllianceBusiness>().Where(x=>x.MD5 == request.MD5)).FirstOrDefault();
            if (agency == null || agency.Id == 0)
            {
                resp.AgencyId = 0;
            }
            else
            {
                resp.AgencyId = agency.Id;
            }
            if(alliance == null || alliance.Id == 0){
                resp.AllianceId = 0;
            }
            else{
                resp.AllianceId = alliance.Id;
            }
            return resp;
        }

        public object Get(AgencyChildPhyInformation request)
        {
            var childInfo = Db.Select(Db.From<DB_Child>().Where(c => c.Id == request.ChildId && c.Cancel == false)).FirstOrDefault();
            var phyInformation =
                Db.Select(
                    Db.From<RT2_Result>().Where(c => c.ChildId == request.ChildId).OrderByDescending(p => p.CreateTime))
                    .Take(1).FirstOrDefault();
            var result = new EveryChildTestResponse();
            result.ChildInformation = childInfo;
            result.TestInformation = phyInformation;
            return result;
        }

        public bool Put(UploadWxEncode request)
        {
            Db.UpdateOnly(new DB_ServiceUnit { WxImage = request.WxImg}, onlyFields: s => s.WxImage,
                where: s => s.Id == request.Id);
            return true;
        }
    }
}
