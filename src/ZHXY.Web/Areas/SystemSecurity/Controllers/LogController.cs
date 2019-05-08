﻿
using System.Threading.Tasks;
using System.Web.Mvc;
using ZHXY.Application;
namespace ZHXY.Web.SystemSecurity.Controllers
{
    public class LogController : ZhxyWebControllerBase
    {
        private SysLogAppService App { get; }
        public LogController(SysLogAppService app) => App = app;
        public async Task<ViewResult> LoginHis() => await Task.Run(() => View());

        [HttpGet]
        public ActionResult Load(GetLogListDto input)
        {
            var (list, recordCount, pageCount) = App.Load(input);
            return Resultaat.PagingRst(list, recordCount, pageCount);
        }
    }
}