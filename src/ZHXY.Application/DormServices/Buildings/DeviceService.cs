﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ZHXY.Domain;

namespace ZHXY.Application
{
    /// <summary>
    /// 大屏设备
    /// </summary>
    public class DeviceService : AppService
    {
        public DeviceService(IZhxyRepository r) : base(r) { }

        /// <summary>
        /// 根据大屏设备号获取绑定的楼栋列表
        /// </summary>
        /// <param name="relevance"></param>
        /// <returns></returns>
        public List<Building> GetBindGate(Relevance relevance)
        {
            if (relevance == null)
            {
                return Read<Building>().ToList();
            }

            var relevances = Read<Relevance>(p => p.FirstKey == relevance.FirstKey && p.Name == "Device_Building").FirstOrDefault()?.SecondKey.Split(',').ToList();

            var list = Read<Building>(p => relevances.Contains(p.Id)).ToList();

            return list;
        }

        /// <summary>
        /// 大屏绑定楼栋信息
        /// </summary>
        /// <param name="deviceNumber"></param>
        /// <param name="buildingId"></param>
        public void BindBuildings(string deviceNumber, string buildingIds)
        {
            var relevance = new Relevance();
            relevance.Name = "Device_Building";
            relevance.FirstKey = deviceNumber;
            relevance.SecondKey = buildingIds;

            AddAndSave(relevance);
        }

        /// <summary>
        /// 大屏解绑楼栋信息
        /// </summary>
        /// <param name="deviceNumber"></param>
        public void UnBindBuildings(string deviceNumber)
        {
            var relevance = Read<Relevance>(p => p.Name == "Device_Building" && p.FirstKey == deviceNumber).FirstOrDefault();

            if (relevance != null)
            {
                DelAndSave(relevance);
            }
        }

        /// <summary>
        /// 获取楼栋学生基本外出在寝信息
        /// </summary>
        /// <param name="buildingIds"></param>
        /// <returns></returns>
        public dynamic GetDormOutInInfo(string buildingIds)
        {

            // 根据楼栋ID，查找所有宿舍信息
            var ids = buildingIds.Split(',').ToList();

            var list = new List<Object>();

            ids.ForEach(item =>
            {
                var dorms = Read<DormRoom>(t => t.UnitNumber == item).Select(t => t.Id).ToList();

                // 根据宿舍查找学生宿舍对应表
                var students = Read<DormStudent>(t => dorms.Contains(t.DormId)).Select(t => t.StudentId).ToList();

                // 在寝人数
                var ins = Read<Student>(t => students.Contains(t.Id) && t.InOut == "0").Count();

                var outs = Read<Student>(t => students.Contains(t.Id) && t.InOut == "1").Count();

                list.Add(new
                {
                    F_BuildId = item,
                    F_OutNum = outs,
                    F_InNum = ins
                });
            });

            return list;
        }

        /// <summary>
        /// 获取访客清单
        /// </summary>
        /// <param name="buildingIds"></param>
        /// <returns></returns>
        public dynamic GetVisitorList(string buildingIds, int lastNum = 10)
        {
            // 根据楼栋ID，查找所有宿舍信息
            var ids = buildingIds.Split(',').ToList();

            var dorms = Read<DormRoom>(t => ids.Contains(t.UnitNumber)).Select(t => t.Id).ToList();

            // 根据宿舍查找学生宿舍对应表
            var dormStudents = Read<DormStudent>(t => dorms.Contains(t.DormId)).Select(t => t.StudentId).ToList();

            // 获取到访记录名单
            var vistors = Read<VisitApply>(t => dormStudents.Contains(t.ApplicantId)).OrderByDescending(t => t.VisitStartTime).Select(t => new
            {
                F_Name = t.Student.Name,
                F_Dorm = t.DormRoom.Title,
                F_Avatar = t.Student.FacePic,
                F_VisitedName = t.VisitorName,
                F_InTime = t.VisitStartTime
            }).Take(lastNum).ToList();

            return vistors;
        }

        /// <summary>
        /// 获取最新出入记录
        /// </summary>
        /// <param name="buildingIds"></param>
        /// <returns></returns>
        public dynamic GetLatestInOutRecord(string buildingIds)
        {
            // 根据楼栋ID，查找所有宿舍信息
            var ids = buildingIds.Split(',').ToList();

            var list = new List<Object>();

            var year = DateTime.Now.Year.ToString("0000");
            var month = DateTime.Now.Month.ToString("00");

            ids.ForEach(item =>
            {
                var dorms = Read<DormRoom>(t => t.UnitNumber == item).Select(t => t.Id).ToList();

                // 根据宿舍查找学生宿舍对应表
                var students = Read<DormStudent>(t => dorms.Contains(t.DormId)).Select(t => t.StudentId).ToList();

                // 获取最近一次进入的记录，需要从大华原始表里去查
                var inSql = string.Format(" SELECT TOP 1 firstName AS F_Name,CASE gender WHEN 0 THEN '女' ELSE '男' END AS F_Sex,date AS F_Time,idNum AS F_StuNum,pictureUrl AS F_Avater,picture1 as F_SnapAvater FROM [dbo].[DHFLOW_{0}{1}] WHERE inOut=0 ORDER BY date DESC ", year, month);
                // 获取最近一次出去的记录，需要从大华原始表里去查
                var outSql = string.Format(" SELECT TOP 1 firstName AS F_Name,CASE gender WHEN 0 THEN '女' ELSE '男' END AS F_Sex,date AS F_Time,idNum AS F_StuNum,pictureUrl AS F_Avater,picture1 as F_SnapAvater  FROM [dbo].[DHFLOW_{0}{1}] WHERE inOut=1 ORDER BY date DESC ", year, month);

                list.Add(new
                {
                    F_BuildId = item,
                    F_Iner = GetDataTable(inSql, new System.Data.Common.DbParameter[] { }),
                    F_Outer = GetDataTable(outSql, new System.Data.Common.DbParameter[] { }),
                });
            });

            return list;
        }

        /// <summary>
        /// 获取考勤统计
        /// </summary>
        /// <param name="buildingIds"></param>
        /// <returns></returns>
        public dynamic GetSignInfo(string buildingIds)
        {
            // 根据楼栋ID，查找所有宿舍信息
            var ids = buildingIds.Split(',').ToList();

            var list = new List<Object>();


            ids.ForEach(item =>
            {
                var dorms = Read<DormRoom>(t => t.UnitNumber == item).Select(t => t.Id).ToList();

                // 根据宿舍查找学生宿舍对应表
                var dormStudents = Read<DormStudent>(t => dorms.Contains(t.DormId)).Select(t => t.StudentId).ToList();

                // 1= 请假 2= 晚归 3=未归 4=未出 5=异常
                var sql = "SELECT '4' AS F_Status ,'未出' AS F_StatusName,"
 + "(SELECT COUNT(0) FROM [Dorm_NoOutReport] WHERE F_Dorm IN ( '" + string.Join("','", dorms.ToArray()) + "' )) AS F_Num "
 + " UNION "
 + " SELECT '3' AS F_Status ,'未归' AS F_StatusName,"
 + "(SELECT COUNT(0) FROM [Dorm_NoReturnReport] WHERE F_Dorm IN ('" + string.Join("','", dorms.ToArray()) + "' )) AS F_Num "
 + " UNION "
 + " SELECT '2' AS F_Status ,'晚归' AS F_StatusName,"
 + "(SELECT COUNT(0) FROM [Dorm_LateReturnReport] WHERE F_Dorm IN ('" + string.Join("','", dorms.ToArray()) + "' )) AS F_Num "
 + " UNION "
 + " SELECT '1' AS F_Status ,'请假' AS F_StatusName,"
 + "(SELECT COUNT(0) FROM [School_Stu_Leave] WHERE F_StudentID IN ('" + string.Join("','", dormStudents.ToArray()) + "' ) AND CONVERT(VARCHAR(10),F_StartTime,120)< GETDATE() AND CONVERT(VARCHAR(10),F_EndTime,120)> GETDATE()  ) AS F_Num "
 + " UNION "
 + " SELECT '5' AS F_Status, '其他异常' AS F_StatusName,0 AS F_Num ";

                list.Add(new
                {
                    F_BuildId = item,
                    F_Data = GetDataTable(sql, new System.Data.Common.DbParameter[] { })
                });
            });

            return list;
        }

        /// <summary>
        /// 24小时进出最近记录
        /// </summary>
        /// <param name="buildingIds"></param>
        /// <returns></returns>
        public dynamic GetInOutNumInLatestHours(string buildingIds)
        {
            // 根据楼栋ID，查找所有宿舍信息
            var ids = buildingIds.Split(',').ToList();

            var dorms = Read<DormRoom>(t => ids.Contains(t.UnitNumber)).Select(t => t.Id).ToList();

            // 根据宿舍查找学生宿舍对应表
            var dormStudents = Read<DormStudent>(t => dorms.Contains(t.DormId)).Select(t => t.Student.StudentNumber).ToList();

            var year = DateTime.Now.Year.ToString("0000");
            var month = DateTime.Now.Month.ToString("00");

            var today = DateTime.Now.ToString("yyyy-MM-dd");


            var inSql = string.Format(" SELECT CONVERT(VARCHAR,COUNT(0)) AS F_Num,DATEPART(HH,DATEADD(SS,Convert(INT, swipDate),'1970-1-1 08:00:00')) AS F_Hour  FROM [dbo].[DHFLOW_{0}{1}] "
             + " WHERE DATEADD(SS,Convert(INT, swipDate),'1970-1-1 08:00:00') >='{2}' AND inOut=0 "
             + " AND idNum IN ('" + string.Join("','", dormStudents) +"') "
             + " GROUP BY DATEPART(HH,DATEADD(SS,Convert(INT, swipDate),'1970-1-1 08:00:00')) "
             + " ORDER BY F_Hour ", year, month,today);

            var outSql = string.Format(" SELECT CONVERT(VARCHAR,COUNT(0)) AS F_Num,DATEPART(HH,DATEADD(SS,Convert(INT, swipDate),'1970-1-1 08:00:00')) AS F_Hour  FROM [dbo].[DHFLOW_{0}{1}] "
             + " WHERE DATEADD(SS,Convert(INT, swipDate),'1970-1-1 08:00:00') >='{2}' AND inOut=1 "
             + " AND idNum IN ('" + string.Join("','", dormStudents) + "') "
             + " GROUP BY DATEPART(HH,DATEADD(SS,Convert(INT, swipDate),'1970-1-1 08:00:00')) "
             + " ORDER BY F_Hour ", year, month,today);

            // 时间轴，1-24小时，如果数据中未包含此时段的数据，人工补0
            var inDt = GetDataTable(inSql, new System.Data.Common.DbParameter[] { });

            var outDt = GetDataTable(outSql, new System.Data.Common.DbParameter[] { });

            for (int i = 0; i < 24; i++)
            {
                var item = (from a in inDt.AsEnumerable() select a.Field<int?>("F_Hour")).Where(t => t == i).FirstOrDefault();
                if (item == null)
                {
                    DataRow row = inDt.NewRow();
                    row["F_Num"] = 0;
                    row["F_Hour"] = i;
                    inDt.Rows.InsertAt(row, i);
                }

                item = (from a in outDt.AsEnumerable() select a.Field<int?>("F_Hour")).Where(t => t == i).FirstOrDefault();
                if (item == null)
                {
                    DataRow row = outDt.NewRow();
                    row["F_Num"] = 0;
                    row["F_Hour"] = i;
                    outDt.Rows.InsertAt(row, i);
                }
            }

            return new
            {
                F_Iner = inDt,
                F_Outer = outDt
            };
        }

        /// <summary>
        /// 获取最新版本号
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        public dynamic GetLatestAppVersion(string currentVersion)
        {

            var a = currentVersion.Split('.');

            var appVersion = Read<AppVersion>().OrderByDescending(t=>t.Id).FirstOrDefault();

            if (appVersion != null)
            {
                bool flag = false;

                var b = appVersion.Version.Split('.');

                if (Convert.ToInt16(b[0]) > Convert.ToInt16(a[0]))
                {
                    flag = true;
                }
                else if(Convert.ToInt16(b[0]) == Convert.ToInt16(a[0]))
                {
                    if(Convert.ToInt16(b[1]) > Convert.ToInt16(a[1]))
                    {
                        flag = true;
                    }
                    else if(Convert.ToInt16(b[1]) == Convert.ToInt16(a[1]))
                    {
                        if (Convert.ToInt16(b[2]) > Convert.ToInt16(a[2]))
                        {
                            flag = true;
                        }
                    }
                }

                if (flag)
                {
                    return new
                    {
                        F_NewVersion = appVersion.Version,
                        F_Url = appVersion.Url,
                        F_Description = appVersion.Description
                    };
                }
            }

            return null;
           
        }
    }
}