﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ZHXY.Application;
using ZHXY.Common;

namespace ZHXY.Web.Dorm.Controllers
{
    //报表统计控制器
    public class ReportController : ZhxyWebControllerBase
    {
        private LateReturnReportService LateReturnReportApp { get; }
        private OriginalReportService OriginalReportApp { get; }
        public ReportController(LateReturnReportService app_1,OriginalReportService app_2 )
        {
            LateReturnReportApp = app_1;
            OriginalReportApp = app_2;
        }

        #region View
        public ActionResult LateReturn() => View();
        public ActionResult NoReturn() => View();
        public ActionResult NoOut() => View();
        public ActionResult Original() => View();
        #endregion
        #region List
        [HttpGet]
        public ActionResult GetLateReturnList(Pagination pagination, string startTime, string endTime, string classId)
        {
            var list = LateReturnReportApp.GetList(pagination, startTime, endTime, classId).Select(p =>
             new
             {
                 Account = p.F_Account,
                 Name = p.F_Name,
                 Class = p.Class?.F_FullName,
                 Dorm = p.Dorm?.Title,
                 College = p.F_College,
                 InTime = p.F_InTime,
             });
            return PagingResult(list, pagination);
        }
        [HttpGet]
        public ActionResult GetNoReturnList(Pagination pagination, string startTime, string endTime, string classId)
        {
            var list = LateReturnReportApp.GetList(pagination, startTime, endTime, classId).Select(p =>
            new
            {
                Account = p.F_Account,
                Name = p.F_Name,
                Class = p.Class?.F_FullName,
                Dorm = p.Dorm?.Title,
                College = p.F_College,
                Time = p.F_CreatorTime
            });
            return PagingResult(list, pagination);
        }
        [HttpGet]
        public ActionResult GetNoOutList(Pagination pagination, string startTime, string endTime, string classId)
        {
            var list = LateReturnReportApp.GetList(pagination, startTime, endTime, classId).Select(p =>
            new
            {
                Account = p.F_Account,
                Name = p.F_Name,
                Class = p.Class?.F_FullName,
                Dorm = p.Dorm?.Title,
                College = p.F_College,
                InTime = p.F_InTime,
                Time = p.F_Time
            });
            return PagingResult(list, pagination);
        }
        [HttpGet]
        public ActionResult GetOriginalList(Pagination pagination, string studentNum,string startTime,string endTime)
        {
            //StudentAppService stuApp = new StudentAppService();
            //var stuList= stuApp.GetList();
            //var dormList= new DormStudentAppService().GetList();
            var list = OriginalReportApp.GetOriginalList(pagination, studentNum,startTime,endTime).Select(p =>
            {
                //var student = stuList.FirstOrDefault(t => t.F_StudentNum.Equals(p.Code));
                //var data = dormList.FirstOrDefault(t => t.F_Student_ID.Equals(student?.F_Id));
                return new
                {
                    p.ChannelName,
                    p.DepartmentName,
                    Name = p.LastName+p.FirstName,
                    //DormName = data?.F_Memo,
                    p.CardNum,
                    p.Tel,
                    Gender = p.Gender == "1" ? "女" : "男",
                    p.InOut,
                    p.Date
                };
            });
            return PagingResult(list, pagination);
        }
        #endregion
        #region export
        public FileResult LateReturnExport(string classId, string startTime, string endTime)
        {
            IDictionary<string, string> parms = new Dictionary<string, string>();
            var exportSql = new ReportHelper().CreateSql("Dorm_LateReturnReport", classId, startTime, endTime, ref parms);
            var dbParameter = CreateParms(parms);
            var reports = LateReturnReportApp.GetDataTable(exportSql, dbParameter);
            var ms = new NPOIExcel().ToExcelStream(reports, "晚归报表");
            ms.Seek(0, SeekOrigin.Begin);
            var filename = "晚归报表" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            return File(ms, "application/ms-excel", filename);
        }
        public FileResult NoReturnExport(string classId, string startTime, string endTime)
        {
         
            IDictionary<string, string> parms = new Dictionary<string, string>();
            var exportSql = new ReportHelper().CreateSql("Dorm_NoReturnReport", classId, startTime, endTime, ref parms);
            var dbParameter = CreateParms(parms);
            var reports = LateReturnReportApp.GetDataTable(exportSql, dbParameter);
            var ms = new NPOIExcel().ToExcelStream(reports, "未归报表");
            ms.Seek(0, SeekOrigin.Begin);
            var filename = "未归报表" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            return File(ms, "application/ms-excel", filename);
        }
        public FileResult NoOutExport(string classId, string startTime, string endTime)
        {
            IDictionary<string, string> parms = new Dictionary<string, string>();
            var exportSql = new ReportHelper().CreateSql("Dorm_NoOutReport", classId, startTime, endTime, ref parms);
            var dbParameter = CreateParms(parms);
            var reports = LateReturnReportApp.GetDataTable(exportSql, dbParameter);
            var ms = new NPOIExcel().ToExcelStream(reports, "长时间未出报表");
            ms.Seek(0, SeekOrigin.Begin);
            var filename = "长时间未出报表" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            return File(ms, "application/ms-excel", filename);
        }
        public FileResult OriginalExport(string studentNum)
        {
            string stuId = "";
            if (!string.IsNullOrEmpty(studentNum))
            {
                stuId = new StudentAppService().GetIdByStuNum(studentNum);
                if (stuId == null) throw new Exception("未找到该学生");
            }
            IDictionary<string, string> parms = new Dictionary<string, string>();
            var exportSql = OriginalReportApp.CreateSql(stuId, ref parms);
            var dbParameter = CreateParms(parms);
            var reports = OriginalReportApp.GetDataTable(exportSql, dbParameter);
            var ms = new NPOIExcel().ToExcelStream(reports, "原始流水数据报表");
            ms.Seek(0, SeekOrigin.Begin);
            var filename = "原始流水数据报表" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            return File(ms, "application/ms-excel", filename);
        }
        #endregion
    }
}