﻿using log4net;
using ZHXY.Domain;using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZHXY.Common;

namespace ZHXY.Application
{
    public class VisitDormLimitService : AppService
    {

        private ILog Logger { get; } = LogManager.GetLogger(typeof(VisitDormLimitService));

        public object GetGridJson(Pagination pagination, string F_Building, string F_Floor)
        {
            var CountSql = new StringBuilder("select count(1) "
                                                        +" from Dorm_Visit_Limit limit "
                                                        +" left join Dorm_DormStudent dormStudent on limit.Student_Id=dormStudent.F_Student_ID "
                                                        +" left join Dorm_DormInfo dorm on dormStudent.F_DormID = dorm.F_ID " 
                                                        +" left join School_Students student on student.F_Id = dormStudent.F_Student_ID "
                                                        +" left join Sys_Organize organ on organ.F_Id = student.F_Grade_ID "
                                                        +" where limit.F_EnabledMark=1 ");
            if (null != F_Building && !"".Equals(F_Building))
            {
                CountSql.Append(" and dorm.F_Building_No='" + F_Building + "' ");
            }
            if (null != F_Floor && !"".Equals(F_Floor))
            {
                CountSql.Append(" and dorm.F_Floor_No='" + F_Floor + "' ");
            }
            pagination.Records = R.Db.Database.SqlQuery<int>(CountSql.ToString()).First();
            if (pagination.Page * pagination.Rows > pagination.Records)
            {
                pagination.Rows = pagination.Records % pagination.Rows;
            }

            var DataSql = new StringBuilder("select top "+ pagination.Rows + " * from (select top "+ pagination.Page * pagination.Rows + " dorm.F_Building_No as F_Build, dormStudent.F_Memo F_Dorm_Num,student.F_Name F_Student_Name, organ.F_FullName F_College ,"
                                                    +" limit.TotalLimit Total_Limit, limit.UsableLimit Usable_Limit, limit.F_EnabledMark F_EnabledMark"
                                                    +" from Dorm_Visit_Limit limit"
                                                    +" left"
                                                    +" join Dorm_DormStudent dormStudent on limit.Student_Id = dormStudent.F_Student_ID"
                                                    + " left"
                                                    + " join Dorm_DormInfo dorm on dormStudent.F_DormID = dorm.F_ID"
                                                    + " left"
                                                    + " join School_Students student on student.F_Id = dormStudent.F_Student_ID"
                                                    + " left"
                                                    + " join Sys_Organize organ on organ.F_Id = student.F_Grade_ID"
                                                    + " where limit.F_EnabledMark = 1 ORDER BY F_Dorm_Num desc) as a ORDER BY a.F_Dorm_Num ASC");
            return R.Db.Database.SqlQuery<DormVisitLimitMoudle>(DataSql.ToString()).ToList();
        }

        public object GetFloor(string BuildName)
        {
            return Read<DormRoom>(p => p.BuildingId.Equals(BuildName)).Select(p => new { id = p.FloorNumber, text = p.FloorNumber }).Distinct().ToList();
        }

        public void SubmitForm(int TimesOfWeek, string Organ, string OrganGrade, string OrganCourts, string OrganClass, int AutoSet)
        {
            var query = Read<Student>();
            //判断是否精确到班级
            if (null != OrganClass && !"".Equals(OrganClass)) {
                var StudentIds = query.Where(p => p.F_Class_ID.Equals(OrganClass)).Select(p => p.F_Id).ToArray<string>();
                SetVisitTimes(StudentIds, TimesOfWeek, AutoSet);
                return;
            }
            //判断是否精确到分院
            if (null != OrganCourts && !"".Equals(OrganCourts)) {
                var StudentIds = query.Where(p => p.F_Grade_ID.Equals(OrganCourts)).Select(p => p.F_Id).ToArray<string>();
                SetVisitTimes(StudentIds, TimesOfWeek, AutoSet);
                return;
            } 
            //判断是否精确到年级
            if(null != OrganGrade && !"".Equals(OrganGrade))
            {
                var StudentIds = query.Where(p => p.F_Divis_ID.Contains(OrganGrade)).Select(p => p.F_Id).ToArray<string>();
                SetVisitTimes(StudentIds, TimesOfWeek, AutoSet);
                return;
            }
            //判断是否精确到学院
            if (null != Organ && !"".Equals(Organ))
            {
                var StudentIds = query.Select(p => p.F_Id).ToArray<string>();
                SetVisitTimes(StudentIds, TimesOfWeek, AutoSet);
                return;
            }
        }

        //查询学院
        public object FindOrgan(string OrganName)
        {
            var query = Read<Organize>(p => p.F_ParentId.Equals("2"));
            if(null != OrganName && !"".Equals(OrganName))
            {
                query.Where(p => p.F_FullName.Contains(OrganName));
            }
            return query.Select(p => new { id = p.F_Id, text = p.F_FullName }).ToList();
        }

        //查询年级
        public object FindOrganGrade(string OrganId, string GradeName)
        {
            var query = Read<Organize>(p => p.F_ParentId.Equals(OrganId));
            if(null != GradeName && !"".Equals(GradeName))
            {
                query.Where(p => p.F_FullName.Contains(GradeName));
            }
            return query.Select(p => new { id = p.F_Id, text = p.F_FullName}).ToList();
        }

        //查询分院
        public object FindOrganCourts(string GradeId, string CourtName)
        {
            var query = Read<Organize>(p => p.F_ParentId.Equals(GradeId));
            if (null != CourtName && !"".Equals(CourtName))
            {
                query.Where(p => p.F_FullName.Contains(CourtName));
            }
            return query.Select(p => new { id = p.F_Id, text = p.F_FullName }).ToList();
        }

        //查询班级
        public object FindOrganClass(string CourtsId, string ClassName)
        {
            var query = Read<Organize>(p => p.F_ParentId.Equals(CourtsId));
            if (null != ClassName && !"".Equals(ClassName))
            {
                query.Where(p => p.F_FullName.Contains(ClassName));
            }
            return query.Select(p => new { id = p.F_Id, text = p.F_FullName }).ToList();
        }


        /// <summary>
        /// 工具方法 （批量设置学生每周访问额度）
        /// 已设置的学生则更新，没设置的则设置
        /// </summary>
        /// <param name="Ids"></param>
        /// <param name="TimesOfWeek"></param>
        /// <param name="AutoSet"></param>
        /// <returns></returns>
        public void SetVisitTimes(string[] Ids, int TimesOfWeek, int AutoSet)
        {
            List<string> DormStudents = Read<DormVisitLimit>(p => p.F_EnabledMark == true).Select(p => p.Student_Id).ToList<string>();
            var InsertIds = Ids.Except(DormStudents); //需添加的数据
            var UpdateIds = Ids.Intersect(DormStudents); // 需修改的数据
            var DateTimeNow = DateTime.Now;
            int ProcessRows = 999; //每次处理的数据长度
            ///添加
            if (null != InsertIds && InsertIds.Count() != 0)
            {
                if(InsertIds.Count() >= ProcessRows)
                {
                    int Page = InsertIds.Count() / ProcessRows;
                    StringBuilder SqlText = new StringBuilder("INSERT INTO [dbo].[Dorm_Visit_Limit]([F_Id], [Student_Id], [TotalLimit], [UsableLimit], [IsAutoSet], [CreateTime], [UpdaetTime], [F_EnabledMark]) VALUES ");

                    for(int i=1; i<= Page+1; i++)
                    {
                        List<string> processList = InsertIds.Skip((i - 1) * ProcessRows).Take(ProcessRows).ToList();
                        foreach (string id in processList)
                        {
                            SqlText.Append("('" + Guid.NewGuid().ToString() + "', '" + id + "', " + TimesOfWeek + ", " + TimesOfWeek + ", " + AutoSet + ", '" + DateTimeNow.ToString() + "', '" + DateTimeNow.ToString() + "', '" + true + "'),");
                        }
                        R.Db.Database.ExecuteSqlCommand(SqlText.ToString().Substring(0, SqlText.Length - 1));
                        SqlText = new StringBuilder("INSERT INTO [dbo].[Dorm_Visit_Limit]([F_Id], [Student_Id], [TotalLimit], [UsableLimit], [IsAutoSet], [CreateTime], [UpdaetTime], [F_EnabledMark]) VALUES ");
                    }
                }
                else
                {
                    StringBuilder SqlText = new StringBuilder("INSERT INTO [dbo].[Dorm_Visit_Limit]([F_Id], [Student_Id], [TotalLimit], [UsableLimit], [IsAutoSet], [CreateTime], [UpdaetTime], [F_EnabledMark]) VALUES ");
                    foreach (string id in InsertIds)
                    {
                        SqlText.Append("('" + Guid.NewGuid().ToString() + "', '" + id + "', " + TimesOfWeek + ", " + TimesOfWeek + ", " + AutoSet + ", '" + DateTimeNow.ToString() + "', '" + DateTimeNow.ToString() + "', '" + true + "'),");
                    }
                    R.Db.Database.ExecuteSqlCommand(SqlText.ToString().Substring(0, SqlText.Length - 1));
                }
                
            }

            ///修改
            if (null != UpdateIds && UpdateIds.Count() != 0)
            {
                if (UpdateIds.Count() >= ProcessRows)
                {
                    int Page = InsertIds.Count() / ProcessRows;
                    StringBuilder UpdateSql = new StringBuilder("UPDATE [dbo].[Dorm_Visit_Limit] SET [TotalLimit] = " + TimesOfWeek + ", [UsableLimit] = " + TimesOfWeek + ", [IsAutoSet] = " + AutoSet + ", [UpdaetTime] = '" + DateTimeNow + "' WHERE [Student_Id] in (");
                    for (int i = 1; i <= Page + 1; i++)
                    {
                        List<string> processList = UpdateIds.Skip((i - 1) * ProcessRows).Take(ProcessRows).ToList();
                        foreach (string id in processList)
                        {
                            UpdateSql.Append("'" + id + "',");
                        }
                        R.Db.Database.ExecuteSqlCommand(UpdateSql.ToString().Substring(0, UpdateSql.Length - 1) + ")");
                        UpdateSql = new StringBuilder("UPDATE [dbo].[Dorm_Visit_Limit] SET [TotalLimit] = " + TimesOfWeek + ", [UsableLimit] = " + TimesOfWeek + ", [IsAutoSet] = " + AutoSet + ", [UpdaetTime] = '" + DateTimeNow + "' WHERE [Student_Id] in (");
                    }
                }
                else
                {
                    StringBuilder UpdateSql = new StringBuilder("UPDATE [dbo].[Dorm_Visit_Limit] SET [TotalLimit] = " + TimesOfWeek + ", [UsableLimit] = " + TimesOfWeek + ", [IsAutoSet] = " + AutoSet + ", [UpdaetTime] = '" + DateTimeNow + "' WHERE [Student_Id] in (");
                    foreach (string id in UpdateIds)
                    {
                        UpdateSql.Append("'" + id + "',");
                    }
                    R.Db.Database.ExecuteSqlCommand(UpdateSql.ToString().Substring(0, UpdateSql.Length - 1) + ")");
                }
            }
        }

        private class DormVisitLimitMoudle
        {
            public string F_Build { get; set; }
            public string F_Dorm_Num { get; set; }
            public string F_Student_Name { get; set; }
            public string F_College { get; set; }
            public int Total_Limit { get; set; }
            public int Usable_Limit { get; set; }
            public bool F_EnabledMark { get; set; }

        }
    }
}