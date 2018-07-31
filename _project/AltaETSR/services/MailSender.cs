using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AltaETSR.services {
    public class MailSender {
        public static void SendMail(string msg = "") {
            try {
                MailMessage mailMsg = new MailMessage();

                mailMsg.From = new MailAddress("andreyzenin2014@gmail.com");
                mailMsg.To.Add(new MailAddress("a.zenin@altatender.kz"));
                mailMsg.Subject = "Запуск ЕТС Робота.";
                mailMsg.Body = "Дата: " + DateTime.Now.ToString() + "\n"
                    + "My Host: " + Dns.GetHostName() + "\n"
                    + "Comp IP: " + Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].MapToIPv4().ToString() + "\n"
                    + "UserNameWin: " + Environment.UserName + "\n"
                    + "CompName: " + Environment.MachineName + "\n"
                    + "Flag: " + msg;

                SmtpClient smtpClient = new SmtpClient();

                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("andreyzenin2014@gmail.com", "mdybcuzamplwxjjl");
                smtpClient.Send(mailMsg);
            } catch(Exception ex) { Loger.Write("MailSender", "Sending mail err:"+ex.ToString()); }
        }
    }
}
