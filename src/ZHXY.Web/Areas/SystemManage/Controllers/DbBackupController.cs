﻿using System.Web.Mvc;
using ZHXY.Application;using ZHXY.Domain;
using ZHXY.Common;
namespace ZHXY.Web.SystemManage.Controllers
{
    public class DbBackupController : ZhxyController
    {
        private DbBackupService App { get; }
        public DbBackupController(DbBackupService app) => App = app;

        [HttpGet]
        
        public ActionResult GetGridJson(string queryJson)
        {
            var data = App.GetList(queryJson);
            return Content(data.ToJson());
        }

        [HttpPost]
        
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(DbBackup dto)
        {
            dto.FilePath = Server.MapPath("~/Resource/DbBackup/" + dto.FileName + ".bak");
            dto.FileName += ".bak";
            App.Add(dto);
            return Result.Success();
        }

        [HttpPost]
        
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            App.Delete(keyValue);
            return Result.Success();
        }

        [HttpPost]
        [HandlerAuthorize]
        public void DownloadBackup(string keyValue)
        {
            var data = App.GetById(keyValue);
            var filename = Server.UrlDecode(data.FileName);
            var filepath = Server.MapPath(data.FilePath);
            if (FileDownHelper.FileExists(filepath))
            {
                FileDownHelper.DownLoadold(filepath, filename);
            }
        }
    }
}