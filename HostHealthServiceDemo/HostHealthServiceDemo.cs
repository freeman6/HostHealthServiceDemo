using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
//using System.Windows.Forms;
using System.Timers;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.InteropServices;

namespace HostHealthServiceDemo
{
    public partial class HostHealthServiceDemo : ServiceBase
    {
        //取得磁碟剩餘空間的API
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        Timer HostTimer, WebSitesTimer;
        HostHealthService cpuHealth;
        
        HostInfo host, Webhost;
        string url = @"請掛上自己寫的 WebAPI 儲存蒐集來的資訊";
        HttpClient client = new HttpClient();
        HttpClient WebClient = new HttpClient();
        List<DetectRound> WebSvcErrLog = new List<DetectRound>();
        List<DetectRound> HostsErrLog = new List<DetectRound>();

        public int HostAutoServiceDetect { get; set; }
        public int WebServiceAutoDetectSecond { get; set; }
        public int SQLServiceAutoDetectSecond { get; set; }

        public HostHealthServiceDemo()
        {
            InitializeComponent();
            host = new HostInfo();
            Webhost = new HostInfo();

            Console.WriteLine($"主機健康狀態開始監控中…{DateTime.Now}");
            cpuHealth = new HostHealthService();

            //取得自動偵測服務的時間
            try
            {
                HostAutoServiceDetect = int.Parse(Config.GetConfigData("HostAutoServiceDetect")) * 1000;
                WebServiceAutoDetectSecond = int.Parse(Config.GetConfigData("WebServiceAutoDetectSecond")) * 1000;
                SQLServiceAutoDetectSecond = int.Parse(Config.GetConfigData("SQLServiceAutoDetectSecond")) * 1000;
            }
            catch
            {
                //MessageBox.Show("自動偵測服務時間設定有誤，目前暫時以預設值處理，建議請重新設定。", "系統訊息管制中心", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HostAutoServiceDetect = 30 * 1000;
                WebServiceAutoDetectSecond = 10 * 1000;
                SQLServiceAutoDetectSecond = 10 * 1000;
            }

            //主機資料
            host.ServiceID = new Guid(Config.GetConfigData("HostID"));
            host.CreateDate = DateTime.UtcNow;
            host.Memo = "";

            //Web服務資料
            Webhost.ServiceID = new Guid(Config.GetConfigData("HostID"));
            Webhost.CreateDate = DateTime.UtcNow;
            Webhost.Memo = "";
        }

        protected override void OnStart(string[] args)
        {
            var WebSites = GetWebSites();

            if (HostAutoServiceDetect > 0)
            {
                HostTimer = new System.Timers.Timer();
                HostTimer.Elapsed += new ElapsedEventHandler(HostTimer_Elapsed);
                HostTimer.Interval = HostAutoServiceDetect;
                HostTimer.AutoReset = true;
                HostTimer.Start();
            }

            if (WebServiceAutoDetectSecond > 0)
            {
                WebSitesTimer = new System.Timers.Timer();
                WebSitesTimer.Elapsed += new ElapsedEventHandler(WebSitesTimer_Elapsed);
                WebSitesTimer.Interval = WebServiceAutoDetectSecond;
                WebSitesTimer.AutoReset = true;
                WebSitesTimer.Start();
            }
        }

        protected override void OnStop()
        {

        }

        /// <summary>
        /// 伺服器主機健康偵測
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HostTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            host.IPAddress = "localhost";
            host.CPUUsage = cpuHealth.cpu;
            host.MemoryUsage = cpuHealth.ram;
            host.DiskUsage = cpuHealth.disk;
            host.SystemDiskFreeBytes = GetDiskFreeGigabytes("SYSTEM");
            host.DataDiskFreeBytes = GetDiskFreeGigabytes("DATA");

            var source = JsonConvert.SerializeObject(host, Newtonsoft.Json.Formatting.Indented);
            var content = new StringContent(source, System.Text.Encoding.Unicode, "application/json");

            try
            {
                var response = client.PostAsync(url, content);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 網站服務偵測
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WebSitesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 各項 WebService 的 CPU、Memory、Disk 使用率可參考黑暗大的文章處理即可
            // https://blog.darkthread.net/blog/get-task-manager-list-with-csharp/
            Webhost.CPUUsage = 0.0f;
            Webhost.MemoryUsage = 0.0f;
            Webhost.DiskUsage = 0.0f;
            Webhost.SystemDiskFreeBytes = 0.0f;
            Webhost.DataDiskFreeBytes = 0.0f;

            var websites = GetWebSites();
            HttpClient WebService = new HttpClient();
            MailService mail = new MailService();
            LineService line = new LineService();

            foreach (var item in websites)
            {
                try
                {
                    Webhost.IPAddress = item;
                    var WebSiteResp = await WebService.GetAsync(item);

                    if (WebSiteResp.IsSuccessStatusCode == true)
                    {
                        Webhost.IISLiveStatus = true;
                    }
                    else
                    {
                        Webhost.IISLiveStatus = false;
                        WebSvcErrLog.Add(new DetectRound { IPAddress = item, ErrTime = DateTime.Now });

                        if (Config.AutoRestart == true)
                        {
                            //第一個解決方案：重新啟動 Application Pool
                            //if (GetApplicationPool(item) == false)
                            //{
                            //    RestartApplicationPool(item);
                            //}

                            WebSiteResp = await WebService.GetAsync(item);
                            if (WebSiteResp.IsSuccessStatusCode == false)
                            {
                                //第二個解決方案：重新啟動 IIS Service
                                Process.Start(@"C:\Windows\System32\iisreset.exe");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                }

                var source = JsonConvert.SerializeObject(Webhost, Newtonsoft.Json.Formatting.Indented);
                var content = new StringContent(source, System.Text.Encoding.Unicode, "application/json");

                try
                {
                    var response = WebClient.PostAsync(url, content);
                    var span = DateTime.Now - WebSvcErrLog.LastOrDefault().ErrTime;

                    //在3分鐘內出現超過3次錯誤時，每4次通知一次(太頻繁通知，胃會不好…XD)
                    if ((WebSvcErrLog.Count >= 3) && (span.TotalMinutes <= 3) && (WebSvcErrLog.Count % Config.Counter == 0))
                    {
                        line.Send(item);
                        mail.Send($"{item} 服務重啟通知", item);
                        WebSvcErrLog = new List<DetectRound>();
                    }
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// 取得本機要偵測服務的網站資訊
        /// </summary>
        /// <returns></returns>
        private List<string> GetWebSites()
        {
            List<string> WebSites = new List<string>();

            for (int i = 1; i < 10; i++)
            {
                var item = "WebSite" + ("00" + i.ToString()).Substring(1, 2);

                if (string.IsNullOrEmpty(Config.GetConfigData(item)) == false)
                {
                    WebSites.Add(Config.GetConfigData(item));
                }
            }

            return WebSites;
        }

        /// <summary>
        /// 取得主機磁碟的剩餘空間
        /// </summary>
        /// <param name="QueryDrive"></param>
        /// <returns></returns>
        private double GetDiskFreeGigabytes(string QueryDrive)
        {
            string drive = string.Empty;      // 要查詢剩餘空間的磁碟 
            double free_gigabytes;

            switch (QueryDrive.ToUpper())
            {
                case "SYSTEM":
                    drive = Config.SystemDisk;
                    break;
                case "DATA":
                    drive = Config.DataDisk;
                    break;
                default:
                    break;
            }
            ulong FreeBytesAvailable;
            ulong TotalNumberOfBytes;
            ulong TotalNumberOfFreeBytes;

            if (string.IsNullOrEmpty(drive) == false)
            {
                bool success = GetDiskFreeSpaceEx(drive, out FreeBytesAvailable, out TotalNumberOfBytes, out TotalNumberOfFreeBytes);

                if (!success)
                    throw new System.ComponentModel.Win32Exception();

                double free_kilobytes = (double)(Int64)TotalNumberOfFreeBytes / 1024.0;
                double free_megabytes = free_kilobytes / 1024.0;
                free_gigabytes = free_megabytes / 1024.0;
            }
            else
            {
                free_gigabytes = 0.0;
            }
            return free_gigabytes;
        }

    }
}
