using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostHealthServiceDemo
{
    public static class Config
    {
        /// <summary>
        /// SMTP伺服器
        /// </summary>
        public static string SMTPServer
        {
            get { return GetConfigData("SMTPServer"); }
        }

        /// <summary>
        /// 寄件者帳號
        /// </summary>
        public static string SMTPFrom
        {
            get { return GetConfigData("SMTPFrom"); }
        }

        /// <summary>
        /// 寄件者密碼
        /// </summary>
        public static string SMTPFromPwd
        {
            get { return GetConfigData("SMTPFromPwd"); }
        }

        /// <summary>
        /// 第一個收件者
        /// </summary>
        public static string SMTPToUser01
        {
            get { return GetConfigData("SMTPToUser01"); }
        }

        /// <summary>
        /// Line ChannelToken
        /// </summary>
        public static string ChannelAccessToken
        {
            get
            {
                string result = GetConfigData("ChannelAccessToken");
                if (string.IsNullOrEmpty(result) == true)
                {
                    result = "請填入自己的Line ChannelAccessToken";
                }
                return result;
            }
        }

        /// <summary>
        /// NotifyUserID01
        /// </summary>
        public static string LintToUser01
        {
            get { return GetConfigData("Admin01LineID"); }
        }

        /// <summary>
        /// NotifyUserID02
        /// </summary>
        public static string LintToUser02
        {
            get { return GetConfigData("Admin02LineID"); }
        }

        /// <summary>
        /// NotifyUserID03
        /// </summary>
        public static string LintToUser03
        {
            get { return GetConfigData("Admin03LineID"); }
        }

        /// <summary>
        /// 第二個收件者
        /// </summary>
        public static string SMTPToUser02
        {
            get { return GetConfigData("SMTPToUser02"); }
        }

        /// <summary>
        /// 找出系統磁碟位置
        /// </summary>
        public static string SystemDisk
        {
            get { return GetConfigData("SystemDiskPath"); }
        }

        /// <summary>
        /// 找出資料磁碟位置
        /// </summary>
        public static string DataDisk
        {
            get { return GetConfigData("DataDiskPath"); }
        }

        /// <summary>
        /// 錯誤偵測計數器
        /// </summary>
        public static int Counter
        {
            get
            {
                int result = 4;
                try
                {
                    result = int.Parse(GetConfigData("NotificationAfterServiceFailCount"));
                }
                catch
                {
                    result = 4;
                }

                return result;
            }
        }

        /// <summary>
        /// 是否自動重新啟動
        /// </summary>
        public static bool AutoRestart
        {
            get
            {
                bool result = true;
                try
                {
                    result = bool.Parse(GetConfigData("AutoRestart"));
                }
                catch
                {
                    result = false;
                }

                return result;
            }
        }

        /// <summary>
        /// 取得組態檔資訊
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfigData(string key)
        {
            string Item = string.Empty;
            if (ConfigurationManager.AppSettings[key] != null)
            {
                Item = ConfigurationManager.AppSettings.GetValues(key).FirstOrDefault();
            }

            return Item;
        }

    }

}
