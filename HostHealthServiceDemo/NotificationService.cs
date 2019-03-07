using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using LineBot;

namespace HostHealthServiceDemo
{
    /// <summary>
    /// 寄送mail相關函數
    /// </summary>
    public class MailService
    {
        //站存設定檔
        private SmtpClient _mySmtp;

        public MailService()
        {
            //設定smtp主機
            _mySmtp = new SmtpClient(Config.SMTPServer);

            //設定smtp帳密
            //_mySmtp.Credentials = new System.Net.NetworkCredential(Config.SMTPFrom, Config.SMTPFromPwd);
        }

        /// <summary>
        /// 發送信件
        /// </summary>
        /// <param name="context">信件內容</param>
        /// <returns>是否成功</returns>
        public bool Send(string subject, string context)
        {
            bool check = false;
            try
            {
                string today = DateTime.Now.ToLocalTime().ToString();
                string pcontect = $"系統管理者您好：<br><br>　　<h4>{context} IIS 服務已於 {DateTime.Now} 重新啟動，相關問題可由事件檢事器進行故障排除。</h4>";

                //設定mail內容
                MailMessage msgMail = new MailMessage();

                //寄件者
                msgMail.From = new MailAddress(Config.SMTPFrom);

                //收件者
                msgMail.To.Add(new MailAddress(Config.SMTPToUser01));
                msgMail.To.Add(new MailAddress(Config.SMTPToUser02));

                //主旨
                msgMail.Subject = subject;

                //信件內容(含HTML時)
                AlternateView alt = AlternateView.CreateAlternateViewFromString(pcontect, null, "text/html");
                msgMail.AlternateViews.Add(alt);

                //寄mail
                _mySmtp.Send(msgMail);
                check = true;
            }
            catch (Exception)
            {
                check = false;
            }

            return check;
        }
    }

    /// <summary>
    /// 寄送Line相關函數
    /// </summary>
    public class LineService
    {
        isRock.LineBot.Bot bot;
        public LineService()
        {
            try
            {
                bot = new isRock.LineBot.Bot(Config.ChannelAccessToken);
            }
            catch
            {
                bot = null;
            }
        }

        public bool Send(string context)
        {
            bool result = false;
            if (bot != null)
            {
                try
                {
                    string LineContext = $"管理者您好，我們偵測到服務( {context} )已多次重新啟動仍無法解決問題，請您儘速登入伺服器查看。";

                    //寄送通知給 01 管理者
                    if (string.IsNullOrEmpty(Config.LintToUser01) == false)
                    {
                        bot.PushMessage(Config.LintToUser01, LineContext);
                    }

                    //寄送通知給 02 管理者
                    if (string.IsNullOrEmpty(Config.LintToUser02) == false)
                    {
                        bot.PushMessage(Config.LintToUser02, LineContext);
                    }

                    //寄送通知給 03 管理者
                    if (string.IsNullOrEmpty(Config.LintToUser03) == false)
                    {
                        bot.PushMessage(Config.LintToUser03, LineContext);
                    }

                    result = true;
                }
                catch
                {
                    result = false;
                }

            }

            return result;
        }
    }

    internal class DetectRound
    {
        public string IPAddress { get; set; }
        public DateTime ErrTime { get; set; }
    }
}
