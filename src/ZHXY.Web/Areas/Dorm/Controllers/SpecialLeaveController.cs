﻿using System.Threading.Tasks;
using System.Web.Mvc;
using ZHXY.Application;
using ZHXY.Common;

namespace ZHXY.Web.Dorm.Controllers
{
    /// <summary>
    /// 不计考勤请假控制器
    /// </summary>
    public class SpecialLeaveController : ZhxyController
    {
        public LeaveService App { get; }
        public SpecialLeaveController(LeaveService app) => App = app;
        public async Task<ViewResult> ApplyForm() => await Task.Run(() => View());
        public async Task<ViewResult> SelectStudent() => await Task.Run(() => View());

        /// <summary>
        /// 不计考勤请假
        /// </summary>
        [HttpPost]
        public ActionResult Apply(BulkLeaveDto input) 
        {
            App.SpecialApply(input, Operator.GetCurrent().Id);
            return Resultaat.Success();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpGet]
        public ActionResult Load(GetSpecialLeaveDto input)
        {
            input.CurrentUserId = Operator.GetCurrent().Id;
            var (list, recordCount, pageCount) = App.GetSpecialList(input);
            return Resultaat.PagingRst(list,recordCount,pageCount);
        }

        /// <summary>
        /// 机构树
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetStudentByOrg(string orgId, string keyword) => await Task.Run(() => Resultaat.Success(App.GetStuByOrg(orgId, keyword)));
    }
}