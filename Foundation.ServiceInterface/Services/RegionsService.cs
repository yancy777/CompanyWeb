using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyBusSSApi.ServiceInterface.Services;
using ServiceStack;
using ServiceStack.Logging;

namespace Foundation.ServiceInterface.Services
{
    public class RegionsService:Service
    {
        public static ILog Log = LogManager.GetLogger(typeof(RegionsService));
    }
}
