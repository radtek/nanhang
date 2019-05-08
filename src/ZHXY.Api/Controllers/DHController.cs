﻿using System.Linq;
using System.Web.Http;
using TaskApi.NanHang;
using ZHXY.Dorm.Device.DH;
using ZHXY.Dorm.Device.tools;

namespace ZHXY.Api.Controllers
{
    public class DHController : ApiController
    {
        [HttpGet]
        public object F_DH_TEST(string name, string pname, int level)
        {

            //process();

            //登出 取消鉴权
            //return DHAccount.DHLogOut();

            //更新令牌
            //return DHAccount.DHUpdate();

            //推送人员信息至大华
            //PersonMoudle personMoudle = new PersonMoudle();
            //personMoudle.orgId = "org001";
            //personMoudle.code = "20145014sdfsdagh52265";
            //personMoudle.idCode = "360421199311144018";
            //personMoudle.name = "田忠华";
            //personMoudle.roleId = "student001"; //teacher001
            //personMoudle.sex = 1;
            //personMoudle.colleageCode = "55f67dcc42a5426fb0670d58dda22a5b"; //默认分院
            //personMoudle.dormitoryCode = "fe8a5225be5f43478d0dd0c85da5dd1d";//楼栋  例如： 11栋
            //personMoudle.dormitoryFloor = "8e447843bc8c4e92b9ffdf777047d20d"; //楼层  例如：3楼
            //personMoudle.dormitoryRoom = "20c70f65b54b4f96851e26343678c4ec"; //宿舍号  例如：312
            //personMoudle.photoUrl = "http://169.254.234.66:8080/file/test.jpg";
            //return DHAccount.PUSH_DH_ADD_PERSON(personMoudle);

            //创建宿舍
            //return DHAccount.CREATE_DORMITORY(name, pname, level);

            //查询宿舍
            //return DHAccount.SELECT_DORMITOR(name, pid);

            //修改人员信息至大华
            //return DHAccount.PUSH_DH_UPDATE_PERSON(personMoudle);

            //删除人员信息 至大华
            //return DHAccount.PUSH_DH_DELETE_PERSON(new string[] { "35" });

            //查询人员信息  大华
            //PersonMoudle personMoudleTest = new PersonMoudle();
            //personMoudleTest.name = "";
            //return DHAccount.SELECT_DH_PERSON(personMoudleTest);

            //导入人员照片（.zip压缩包）
            //DHAccount.PUSH_DH_BATCHPHOTO_ZIP("C:\\imgFile\\imgFile");

            //导入人员信息（excel文件）
            //DHAccount.PUSH_DH_PERSON_EXCEL("C:\\人员信息.xlsx");
            DHAccount.PUSH_DH_PERSON_EXCEL("C:\\" + name);
            return null;
            //获取大华设备信息
            //return DHAccount.GetMachineInfo("001", "01;01,02,05,07_4,08_3,08_5,34,38;01@9,07,12", "0", "");

            //获取MQ相关配置
            //return DHAccount.GetMQConfig(new MQMoudle());

        }

        public static void process()
        {
            //批量同步11栋和12栋学生数据，到12栋1楼101室
            NHModel model = new NHModel();
            var stuList = model.StudentInfoes.Where(p => p.studentBuildingId.Contains("12栋") || p.studentBuildingId.Contains("11栋")).Select(p => new PersonMoudle
            {
                orgId = "org001",
                code = p.studentNo,
                idCode = p.certificateNo,
                name = p.studentName,
                roleId = "student001", //teacher001
                sex = 0,
                colleageCode = "55f67dcc42a5426fb0670d58dda22a5b", //默认分院
                dormitoryCode = "fe8a5225be5f43478d0dd0c85da5dd1d",//楼栋  例如： 11栋
                dormitoryFloor = "8e447843bc8c4e92b9ffdf777047d20d", //楼层  例如：3楼
                dormitoryRoom = "20c70f65b54b4f96851e26343678c4ec", //宿舍号  例如：312
                photoUrl = p.ImgUri
            }).ToList();

            foreach (PersonMoudle person in stuList)
            {
                DHAccount.PUSH_DH_ADD_PERSON(person);
            }
        }
    }
}