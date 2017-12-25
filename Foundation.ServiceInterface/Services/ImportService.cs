using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using BabyBusSSApi.ServiceModel.Test;
using Foundation.ServiceModel.Import;
using ServiceStack;
using ServiceStack.Logging;

namespace Foundation.ServiceInterface.Services
{
    public class ImportService:Service
    {
        public static bool IsImporting = false;
        public static ILog Log = LogManager.GetLogger(typeof(ImportService));
        public IServerEvents ServerEvents { get; set; }
//        public object Post(TestDto request)
//        {
//            return new TestResponse { Result = "Just a test, done by {0}!".Fmt(request.Name) };
//        }

        public async Task Post(ImportExcel request)
        {
            var sub = ServerEvents.GetSubscriptionInfo(request.From);

            if (sub == null)
                throw HttpError.NotFound("Subscription {0} does not exist".Fmt(request.From));

            if (IsImporting)
            {
                throw HttpError.Forbidden("已经有一个导入的任务，请等待这个任务完成以后再尝试导入");
            }
        }
    }
}
