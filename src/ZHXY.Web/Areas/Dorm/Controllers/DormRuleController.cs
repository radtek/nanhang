﻿using System.Web.Mvc;
using ZHXY.Application;
namespace ZHXY.Web.Dorm.Controllers
{
    /// <summary>
    /// 宿舍规则
    /// </summary>
    public class DormRuleController : ZhxyController
    {
        private DormRuleAppService App { get; }
        public DormRuleController(DormRuleAppService app) => App = app;

        [HttpGet]
        public ActionResult Get(string id) => Result.Success(App.GetById(id));
        public ActionResult Update(UpdateDormRuleDto input)
        {
            App.Update(input);
            return Result.Success();
        }

        [HttpGet]
        public ActionResult Load() => Result.Success(App.Load());
       
    }
}