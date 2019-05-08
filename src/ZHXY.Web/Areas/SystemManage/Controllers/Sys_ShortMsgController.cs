﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using ZHXY.Application;
using ZHXY.Common;
using ZHXY.Domain.Entity;

namespace ZHXY.Web.SystemManage.Controllers
{
    //短信发送记录

    public class Sys_ShortMsgController : ZhxyWebControllerBase
    {
        private ShortMsgAppService App { get; }
        public Sys_ShortMsgController(ShortMsgAppService app) => App = app;

        [HttpGet]
        
        public ActionResult GetGridJson(Pagination pagination, string keyword, string F_DepartmentId, string F_CreatorTime_Start, string F_CreatorTime_Stop)
        {
            var data = new
            {
                rows = App.GetList(pagination, keyword, F_DepartmentId, F_CreatorTime_Start, F_CreatorTime_Stop),
                total = pagination.Total,
                page = pagination.Page,
                records = pagination.Records
            };
            return Content(data.ToJson());
        }

        [HttpGet]
        
        public ActionResult GetFormJson(string keyValue)
        {
            var data = App.GetForm(keyValue);
            //将用户id替换成姓名
            //var creator = new UserEntity();
            // var modifier = new UserEntity();
            //Dictionary<string, UserEntity>  dic = CacheFactory.Cache().GetCache<Dictionary<string, UserEntity>>(Cons.USERS);
            //if (data.F_CreatorUserId != null && dic.TryGetValue(data.F_CreatorUserId,out creator)) {
            //     data.F_CreatorUserId = creator.F_RealName;
            // }
            // if (data.F_LastModifyUserId != null &&　dic.TryGetValue(data.F_LastModifyUserId, out modifier))
            // {
            //     data.F_LastModifyUserId = modifier.F_RealName;
            // }
            return Content(data.ToJson());
        }

        [HttpPost]
        
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(ShortMsg entity, string keyValue)
        {
            entity.F_DepartmentId = OperatorProvider.Current.DepartmentId;
            App.SubmitForm(entity, keyValue);
            return Message("操作成功。");
        }

        [HttpPost]
        [HandlerAuthorize]
        
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            App.DeleteForm(keyValue);
            return Message("删除成功。");
        }

        //导出excel
        [HttpGet]
        [HandlerAuthorize]
        public FileResult export(string keyword, string F_CreatorTime_Start, string F_CreatorTime_Stop)
        {
            //参数 字段名->string[]{"F_Id",value}
            IDictionary<string, string> parms = new Dictionary<string, string>();
            //过滤条件
            //if (!Ext.IsEmpty(keyword))
            //    parms.Add("F_ShortMsg", keyword);

            var dbParameter = CreateParms(parms);

            var exportSql = CreateExportSql("Sys_ShortMsg", parms);
            exportSql += " and t.F_DeleteMark != 'true' ";
            if (!string.IsNullOrEmpty(F_CreatorTime_Start))
            {
                var CreatorTime_Start = Convert.ToDateTime(F_CreatorTime_Start + " 00:00:00");
                exportSql += " and t.F_CreatorTime >= '" + CreatorTime_Start + "'";
            }

            if (!string.IsNullOrEmpty(F_CreatorTime_Stop))
            {
                var CreatorTime_Stop = Convert.ToDateTime(F_CreatorTime_Stop + " 23:59:59");
                exportSql += " and t.F_CreatorTime <= '" + CreatorTime_Stop + "'";
            }
            if (!keyword.IsEmpty())
            {
                exportSql += " and t.F_ShortMsg like '%" + keyword + "%' ";
            }
            //string exportSql = "";
            //Console.WriteLine("exportSql==>" + exportSql);
            var users = App.GetDataTable(exportSql, dbParameter);
            ///////////////////写流
            var ms = new NPOIExcel().ToExcelStream(users, "已发送列表");
            ms.Seek(0, SeekOrigin.Begin);
            var filename = "已发送列表" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            return File(ms, "application/ms-excel", filename);
        }

        //导入excel
        [HttpPost]
        
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult import(string filePath)
        {
            //////////////////定义规则：字段名，表头名称，字典
            //字段名->string[]{表头,字典}，若是一般字段 字典为空字符串
            IDictionary<string, string[]> rules = new Dictionary<string, string[]>();
            //rules.Add("F_Id", new string[] { "编号", "" });
            //rules.Add("F_RealName", new string[] { "姓名", "" });
            //rules.Add("F_Gender", new string[] { "性别", "104" });
            //rules.Add("F_OrganizeId", new string[] { "公司", "F_OrganizeId" });
            //rules.Add("F_DepartmentId", new string[] { "部门", "F_DepartmentId" });
            //rules.Add("F_AreaId", new string[] { "地区", "F_AreaId" });
            //rules.Add("F_RoleId", new string[] { "角色", "F_RoleId" });
            //rules.Add("F_DutyId", new string[] { "岗位", "F_DutyId" });
            //rules.Add("F_CreatorTime", new string[] { "创建时间", "" });
            //rules.Add("F_HeadIcon", new string[] { "头像", "" });

            //所有字段代码

            //rules.Add("F_Id", new string[] { "短信ID", "" });

            //rules.Add("F_Mobile", new string[] { "接收人手机号", "" });

            //rules.Add("F_ShortMsg", new string[] { "短信内容", "" });

            //rules.Add("F_FeesCount", new string[] { "计费条数", "" });

            //rules.Add("F_Send_Statu", new string[] { "发送状态", "" });

            //rules.Add("F_SendDTM", new string[] { "发送时间", "" });

            //rules.Add("F_SortCode", new string[] { "序号", "" });

            //rules.Add("F_DepartmentId", new string[] { "所属部门", "" });

            //rules.Add("F_DeleteMark", new string[] { "删除标记", "" });

            //rules.Add("F_EnabledMark", new string[] { "启用标记", "" });

            //rules.Add("F_CreatorTime", new string[] { "创建时间", "" });

            //rules.Add("F_CreatorUserId", new string[] { "创建者", "" });

            //rules.Add("F_LastModifyTime", new string[] { "修改时间", "" });

            //rules.Add("F_LastModifyUserId", new string[] { "修改者", "" });

            //rules.Add("F_DeleteTime", new string[] { "删除时间", "" });

            //rules.Add("F_DeleteUserId", new string[] { "删除者", "" });

            //////////////////处理数据(机构 岗位 等字典替换，过滤不要的字段，修改表头)
            var list = ExcelToList<ShortMsg>(Server.MapPath(filePath), rules);

            ///////////////////入库
            if (list == null)
                return Error("导入失败");
            App.Import(list);
            return Message("导入成功。");
        }
    }
}