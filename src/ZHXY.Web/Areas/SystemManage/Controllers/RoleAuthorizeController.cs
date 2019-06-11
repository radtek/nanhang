﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ZHXY.Application;
using ZHXY.Domain;
using ZHXY.Web.Shared;

namespace ZHXY.Web.SystemManage.Controllers
{
    /// <summary>
    /// 角色授权
    /// </summary>
    public class RoleAuthorizeController : BaseController
    {
        private SysModuleAppService moduleService { get; }
        private SysRoleAuthorizeAppService roleAuthorizeService { get; }
        private SysButtonAppService buttonService { get; }

        public RoleAuthorizeController(SysModuleAppService moduleAppService,
            SysRoleAuthorizeAppService roleAuthorizeAppService,
            SysButtonAppService buttonAppService)
        {
            moduleService = moduleAppService;
            roleAuthorizeService = roleAuthorizeAppService;
            buttonService = buttonAppService;
        }
        public ActionResult GetPermissionTree(string roleId, string BeLong)
        {
            var moduledata = moduleService.GetList();
            if (!string.IsNullOrEmpty(BeLong))
            {
                moduledata = moduleService.GetList().Where(t => t.F_BelongSys.Equals(BeLong)).ToList();
            }
            var buttondata = buttonService.GetList();
            var authorizedata = new List<SysRoleAuthorize>();
            if (!string.IsNullOrEmpty(roleId))
            {
                authorizedata = roleAuthorizeService.GetList(roleId);
            }
            var treeList = new List<ViewTree>();
            foreach (var item in moduledata)
            {
                var tree = new ViewTree();
                var hasChildren = moduledata.Count(t => t.F_ParentId == item.F_Id) == 0 ? false : true;
                tree.Id = item.F_Id;
                tree.Text = item.F_FullName;
                tree.Value = item.F_EnCode;
                tree.ParentId = item.F_ParentId;
                tree.Isexpand = true;
                tree.Complete = true;
                tree.Showcheck = true;
                tree.Checkstate = authorizedata.Count(t => t.F_ItemId == item.F_Id);
                tree.HasChildren = true;
                tree.Img = item.F_Icon == string.Empty ? string.Empty : item.F_Icon;
                treeList.Add(tree);
            }
            foreach (var item in buttondata)
            {
                var tree = new ViewTree();
                var hasChildren = buttondata.Count(t => t.F_ParentId == item.F_Id) == 0 ? false : true;
                tree.Id = item.F_Id;
                tree.Text = item.F_FullName;
                tree.Value = item.F_EnCode;
                tree.ParentId = item.F_ParentId == "0" ? item.F_ModuleId : item.F_ParentId;
                tree.Isexpand = true;
                tree.Complete = true;
                tree.Showcheck = true;
                tree.Checkstate = authorizedata.Count(t => t.F_ItemId == item.F_Id);
                tree.HasChildren = hasChildren;
                tree.Img = item.F_Icon == string.Empty ? string.Empty : item.F_Icon;
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson());
        }
    }
}