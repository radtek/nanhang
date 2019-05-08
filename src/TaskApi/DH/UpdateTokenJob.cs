﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using ZHXY.Common;
using ZHXY.Dorm.Device.tools;

namespace TaskApi.DH
{
    class UpdateTokenJob : IJob
    {
        public static string X_SUBJECT_TOKEN = null; // 令牌 Token
        public static string X_SUBJECT_TEMP = null; // 加密字符 （第四次加密的字符串）
        public static string REDIS_TOKEN_SET_KEY = "huawang_token_redis_key"; //流水数据存储Redis的key
        public static int REDIS_LINE_RECORD_DB_LEVEL = 15;

        public void Execute(IJobExecutionContext context)
        {
            string token = DHUpdate();
            IDatabase db = RedisHelper.GetDatabase(REDIS_LINE_RECORD_DB_LEVEL);
            db.StringSet(REDIS_TOKEN_SET_KEY, token);
            Console.WriteLine("更新鉴权Token信息：" + token);
        }

        /// <summary>
        /// 鉴权
        /// </summary>
        static UpdateTokenJob()
        {
            Console.WriteLine("登陆一次！！");
            ///第一次鉴权（带上用户名）
            Dictionary<string, string> DataDic = new Dictionary<string, string>();
            DataDic.Add("UserName", Constants.DAHUA_LOGIN_USERNAME);
            DataDic.Add("ipAddress", "");
            DataDic.Add("clientType", "WINPC");
            string response = HttpHelper.ExecutePost(Constants.AUTHENTICATION_URI, DataDic.ToJson(), "");
            JObject jo1 = response.ToJObject();
            string Realm = jo1["realm"].ToString(); //域信息
            string RandomKey = jo1["randomKey"].ToString(); //随机值
            string EncryptType = jo1["encryptType"].ToString(); //加密方式

            //第二次鉴权（带上签名）
            MD5 md5 = MD5.Create();
            string temp = Md5Tool.Md532(Constants.DAHUA_LOGIN_PASSWORD); //把密码生成32位小写加密字符串
            temp = Md5Tool.Md532(Constants.DAHUA_LOGIN_USERNAME + temp); //继续加密
            temp = Md5Tool.Md532(temp); //继续加密
            temp = Md5Tool.Md532(Constants.DAHUA_LOGIN_USERNAME + ":" + Realm + ":" + temp); // 继续加密 (后续会用到这个值)
            X_SUBJECT_TEMP = temp;
            string signature = Md5Tool.Md532(temp + ":" + RandomKey);// 最终签名

            Dictionary<string, string> SecondDic = new Dictionary<string, string>();
            SecondDic.Add("userName", Constants.DAHUA_LOGIN_USERNAME);
            SecondDic.Add("randomKey", RandomKey);
            SecondDic.Add("mac", "");
            SecondDic.Add("encryptType", "MD5");
            SecondDic.Add("ipAddress", "");
            SecondDic.Add("signature", signature);
            SecondDic.Add("clientType", "WINPC");
            string SecondParam = JsonConvert.SerializeObject(SecondDic);
            string SecondResponse = HttpHelper.ExecutePost(Constants.AUTHENTICATION_URI, SecondParam, "");
            JObject jo2 = (JObject)JsonConvert.DeserializeObject(SecondResponse);
            string duration = jo2.Value<string>("duration"); //保活时间
            string token = jo2.Value<string>("token"); //令牌
            string userId = jo2.Value<string>("userId"); //用户ID
            string updateUrl = jo2["versionInfo"]["updateUrl"].ToString(); //客户端下载地址
            string sipNum = jo2.Value<string>("sipNum"); //sipNum
            X_SUBJECT_TOKEN = token;
        }

        /// <summary>
        /// 更新TOKEN
        /// </summary>
        public static string DHUpdate()
        {
            string signature = Md5Tool.Md532(X_SUBJECT_TEMP + ":" + X_SUBJECT_TOKEN); //生成签名
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("signature", signature);
            string response = HttpHelper.ExecutePost(Constants.UPDATE_TOKEN_URI, JsonConvert.SerializeObject(dic), X_SUBJECT_TOKEN);
            JObject jo = (JObject)JsonConvert.DeserializeObject(response);
            int returnCode = jo.Value<int>("code");
            if (returnCode == 1101)
            {
                return JsonConvert.SerializeObject(jo);
            }
            string newToken = jo["data"]["token"].ToString(); //生成新的Token信息
            X_SUBJECT_TOKEN = newToken;
            return newToken;
        }
    }
}