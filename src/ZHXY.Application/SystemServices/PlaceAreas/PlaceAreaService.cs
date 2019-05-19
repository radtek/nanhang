﻿using ZHXY.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace ZHXY.Application
{
    /// <summary>
    /// 地区管理
    /// </summary>
    public class PlaceAreaService : AppService
    {
        private IRepositoryBase<PlaceArea> Repository { get; }

        public PlaceAreaService() => Repository = new Repository<PlaceArea>();

        public List<PlaceArea> GetList() => Repository.QueryAsNoTracking().ToList();

        public List<PlaceArea> GetListByParentId(string parentId) => Repository.QueryAsNoTracking().Where(t => t.ParentId == parentId).ToList();

        public PlaceArea GetForm(string keyValue) => Repository.Find(keyValue);

        public void DeleteForm(string keyValue)
        {
            if (Repository.QueryAsNoTracking().Count(t => t.ParentId.Equals(keyValue)) > 0)
            {
                throw new Exception("删除失败！操作的对象包含了下级数据。");
            }
            else
            {
                Repository.BatchDelete(t => t.Id == keyValue);
            }
        }

        public void SubmitForm(PlaceArea areaEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                Repository.Update(areaEntity);
            }
            else
            {
                Repository.Insert(areaEntity);
            }
        }

        public DataTable getDataTable(string sql, DbParameter[] dbParameter) => Repository.GetDataTable(sql, dbParameter);
    }
}