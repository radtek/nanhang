﻿using System.Threading.Tasks;
using System.Web.Mvc;
using ZHXY.Application;
using ZHXY.Common;

namespace ZHXY.Web.Dorm.Controllers
{
    /// <summary>
    /// 南航的异常消息推送控制器
    /// </summary>
    public class NHExceptionPushController : Controller
    {
        private MessageAppService App { get;}
        public NHExceptionPushController(MessageAppService app) => App = app;

        /// <summary>
        ///  通过 sys_org_leader表的参数，生成异常报表
        /// </summary>
        /// <param name="OrgId"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetLateReturnReport(string OrgId, string ReportDate)
        {
            return App.GetLateReturnReport(OrgId, ReportDate);
        }

        [HttpGet]
        public object GetNotReturnReport(string OrgId, string ReportDate)
        {
            return App.GetNotReturnReport(OrgId, ReportDate);
        }

        [HttpGet]
        public object GetNotOutReport(string OrgId, string ReportDate)
        {
            return App.GetNotOutReport(OrgId, ReportDate);
        }
    }
}