﻿using System;
using System.Linq;
using ZHXY.Common;

using ZHXY.Domain;

namespace TaskApi
{
    public class SynUserService
    {
        protected WebHelper WebHelp { get { return new WebHelper(); } }
        private readonly string CallUrlAdd = Configs.GetValue("CallUrlAdd");
        protected string AppId { get { return Configs.GetValue("appid"); } }
        private string SchoolCode { get; }

        public SynUserService(string schoolCode) => SchoolCode = schoolCode;

        public void UpdOrAdd(UserModel data, string catetoryId)// string loginId, string mobilePhone, string orgId, string passWord, string telephone, string userId, string userName, string userStatus, string userType, string catetoryId, string gw,string num,string sex)
        {
            using (var db = new UnitWork().BeginTrans())
            {
                string gw = data.post_Description;
                if (data.isHeadmaster == "1")
                {
                    gw = "班主任";
                }
                if (data.isSubHeadmaster == "1")
                {
                    gw = "副班主任";
                }
                var userType = data.UserType;
                string num = data.user_Num;
                string loginId = data.LoginId;
                string mobilePhone = data.MOBILE;
                string orgId = data.OrgId;
                string passWord = data.PassWord;
                string telephone = data.TELEPHONE;
                string userId = data.UserId;
                string userName = data.UserName;
                string userStatus = data.UserStatus;
                string sex = data.sex;
                DateTime? inDate = null;
                DateTime tmpDate;
                if (DateTime.TryParse(data.join_school_time, out tmpDate))
                    inDate = tmpDate;
                var entity = AddOrUpdUser(db, loginId, mobilePhone, orgId, passWord, telephone, userId, userName, userStatus, userType, catetoryId, gw, sex, data.picture, data.has_school_admin, data.IS_DELETED);
                if (userType == "1")
                {
                    AddOrUpdTeacher(db, entity, catetoryId, gw, num, data.identity_num, data.major, data.political, data.identity_type, inDate);
                }
                if (userType == "6")
                {
                    AddOrUpdStudent(db, entity, catetoryId, num, data.identity_num, data.political, data.identity_type, inDate, data.GuardianId, data.GuardianName);
                }
                if (userType == "5")
                {
                }
                db.Commit();
            }
        }

        private void AddOrUpdStudent(IUnitWork db, User entity, string catetoryId, string num,
            string credNum, string politstatu, string credType, DateTime? entryTime, string parentId, string parentName)
        {
            var student = new Student();
            if (catetoryId == "Class")
            {
                var org = db.FindEntity<Organ>(p => p.Id == entity.OrganId);
                student.F_Class_ID = entity.OrganId;
                student.F_Grade_ID = org?.ParentId;
                student.F_Divis_ID = org?.Parent?.ParentId;
            }
            if (catetoryId == "Grade")
            {
                var org = db.FindEntity<Organ>(p => p.Id == entity.OrganId);
                student.F_Grade_ID = entity.OrganId;
                student.F_Divis_ID = org?.ParentId;
            }
            if (catetoryId == "Division")
                student.F_Divis_ID = entity.OrganId;
            student.F_Name = entity.Name;
            student.F_StudentNum = num;
            student.F_CredNum = credNum;
            student.F_CredType = credType;
            student.F_Gender = entity.Gender != false ? "1" : "0";
            student.F_PolitStatu = politstatu;
            student.F_InitDTM = entryTime;
            //student.F_Users_ID = entity.F_Id;
            student.F_Tel = entity.MobilePhone;
            student.F_DepartmentId = entity.OrganId;

            var s = db.FindEntity<Student>(p => p.F_Users_ID == entity.Id);
            if (s != null)
            {
                student.Modify(s.F_Id);
                db.Update(student);
            }
            else
            {
                student.F_Users_ID = entity.Id;
                student.F_CurStatu = "1";
                student.Create();
                db.Insert(student);
            }

        }
       
        private void AddOrUpdTeacher(IUnitWork db, User entity, string catetoryId, string gw, string num,
            string credNum, string profession, string politstatu, string credType, DateTime? entryTime)
        {
            var teacher = new Teacher();
            if (catetoryId == "Class")
            {
                var org = db.FindEntity<Organ>(p => p.Id == entity.OrganId);
                teacher.OrganId = org?.Parent?.ParentId;
            }
            if (catetoryId == "Grade")
            {
                var org = db.FindEntity<Organ>(p => p.Id == entity.OrganId);
                teacher.OrganId = org?.ParentId;
            }
            if (catetoryId == "Division")
                teacher.OrganId = entity.OrganId;
            teacher.Name = entity.Name;
            teacher.JobNumber = num;
            //student.F_Users_ID = entity.F_Id;
            teacher.MobilePhone = entity.MobilePhone;
            teacher.CredNum = credNum;
            //teacher.F_PolitStatu = politstatu;
            teacher.CredType = credType;
            teacher.EntryTime = entryTime;
            var t = db.FindEntity<Teacher>(p => p.UserId == entity.Id);
            if (t != null)
            {
                db.Update(teacher);
            }
            else
            {
                teacher.UserId = entity.Id;
                db.Insert(teacher);
            }

        }

      

        private User AddOrUpdUser(IUnitWork db, string loginId, string mobilePhone, string orgId, string passWord, string telephone, string userId, string userName, string userStatus, string userType, string catetoryId, string gw, string sex, string ico, string isAdmin, string isDelete)
        {
            var entity = new User();
            entity.EnabledMark = true;
            entity.Id = userId;
            entity.Account = loginId;
            entity.MobilePhone = mobilePhone;
            entity.NickName = userName;
            entity.Name = userName;
            entity.OrganId = orgId;
            entity.Gender = sex == "1" ? true : false;
            entity.HeadIcon = ico;
            entity.DeleteMark = isDelete == "1" ? true : false;
            if (catetoryId == "Class")
            {
                var org = db.FindEntity<Organ>(p => p.Id == orgId);
                entity.OrganId = org?.Parent?.ParentId;
            }
            if (catetoryId == "Grade")
            {
                var org = db.FindEntity<Organ>(p => p.Id == orgId);
                entity.OrganId = org?.ParentId;
            }
            if (catetoryId == "Division")
                entity.OrganId = orgId;
            if (userType == "6")
            {
                entity.DutyId = null;
                entity.RoleId = "student";
            }
            if (userType == "1")
            {
                entity.DutyId = "teacherDuty";
                entity.RoleId = "teacher";
                if ((gw == "班主任" || gw == "副班主任"))//catetoryId == "Class" &&
                {
                    entity.RoleId = "6D8BC58FF1F24924A73F6B86A718BD6C";
                }
            }
            if (userType == "5")
            {
                entity.DutyId = "parentDuty";
                entity.RoleId = "parent";
                entity.OrganId = "parent";
            }
            if (isAdmin == "1")
            {
                entity.RoleId = "7A6C0ECA17B9433DBD3A0C127E35A696";
            }
            var data = db.FindEntity<User>(t => t.Id == userId);
            if (data != null)
            {
                db.Update(entity);
            }
            else
            {
                entity.Id = userId;
                db.Insert(entity);

                setUserInfo(db, entity);
            }
            // 插入角色
            if (entity.RoleId != null)
            {
                var role = db.FindEntity<UserRole>(t => t.F_User == entity.Id);
                if (role != null)
                {
                    role.F_Role = entity.RoleId;
                    db.Update(role);
                }
                else
                {
                    var e = new UserRole();
                    e.F_Id = Guid.NewGuid().ToString();
                    e.F_Role = entity.RoleId;
                    e.F_User = entity.Id;
                    db.Insert(e);
                }
            }
            return entity;
        }

        private void setUserInfo(IUnitWork db, User entity)
        {
            if (db.FindEntity<UserLogin>(t => t.F_Id == entity.Id) != null)
                return;
            var userLogOnEntity = new UserLogin
            {
                F_Id = entity.Id,
                F_UserId = entity.Id,
                F_UserSecretkey = Md5EncryptHelper.Encrypt("0000", 16).ToLower()
            };
            userLogOnEntity.F_UserPassword = Md5EncryptHelper.Encrypt(DESEncryptHelper.Encrypt(Md5EncryptHelper.Encrypt("0000", 32).ToLower(), userLogOnEntity.F_UserSecretkey).ToLower(), 32).ToLower();
            db.Insert(userLogOnEntity);

            var isPost = true;
            var parameters = "sysid=" + entity.Id + "&amp;appid=" + AppId + "&amp;nickname=" +
                             entity.Name + "&amp;username=" + entity.Account +
                             "&amp;password=" + userLogOnEntity.F_UserPassword +
                             "&amp;sysgroupid=" + entity.OrganId + "&amp;headicon=" +
                             entity.HeadIcon + "&amp;birthday=" + entity.Birthday + "";
            WebHelper.SendRequest(CallUrlAdd, parameters, isPost, "application/json");
        }
    }
}