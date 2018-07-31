using System;
using System.Net;
using System.Net.Mail;
using ETSRobot_v2.model.connection;
using System.Windows;
using System.Management;
using ETSRobot_v2.service;

namespace ETSRobot_v2.model.service {
    public class DbETSManager : IDbManager {
        #region Variables
        DSSERVERLib.Connection connectionETS;
        private ManagementObjectSearcher hddSerialSearch;
        private DataBaseManager dataBaseManager;
        #endregion

        #region Methods
        public DbETSManager() {
            if(App.hddSerialNumber != "helloworld") {
                AppJournal.Write("DbETSManager", "Check HDD serial number");
                CheckHDDSerial();

                AppJournal.Write("DbETSManager", "Check DB record about SN");
                dataBaseManager = new DataBaseManager();
                dataBaseManager.CheckSerial();
            }
        }


        private Boolean correctSerial = false;
        private void CheckHDDSerial() {
            hddSerialSearch = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

            try {
                foreach(ManagementObject hdd in hddSerialSearch.Get()) {
                    if(hdd["SerialNumber"].ToString().Trim() == App.hddSerialNumber)
                        correctSerial = true;
                }
            } catch(Exception ex) {
                AppJournal.Write("DbETSManager", "Getting HDD Serial err: " + ex.ToString());
            }

            if(!correctSerial) {
                AppJournal.Write("DbETSManager", "Send email about wrong license");
                SendMail("Not license");
                MessageBox.Show("Не лицензионная версия", "Предупреждение правообладателя", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(0);
            } else SendMail("License");
        }


        public void SendMail(String txtMessage) {
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
                    + "HDDSerial: " + App.hddSerialNumber + "\n"
                    + "Flag: " + txtMessage;

                SmtpClient smtpClient = new SmtpClient();

                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("andreyzenin2014@gmail.com", "mdybcuzamplwxjjl");
                smtpClient.Send(mailMsg);
            } catch(Exception ex) { AppJournal.Write("DbETSManager", "Sending mail error"); }
        }


        private string curBroker = "TRN";

        public bool Connected(string curBroker) {
            this.curBroker = curBroker;

            if(connectionETS == null) GetConnectedServer();

            if(connectionETS == null) {
                MessageBox.Show("No connection to Server. \nCheck setings.", "Connection", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }

            return true;
        }


        public void GetConnectedServer() {
            AppJournal.Write("DbETSManager", "Connect to ETS");

            connectionETS = new DSSERVERLib.Connection();

            try {
                if(curBroker.Contains("TRN")) connectionETS.Open(@"Online.ini", "", "", "");
                else connectionETS.Open(@"Online (war).ini", "", "", "");
            } catch(Exception e) {
                AppJournal.Write("DbETSManager", "Connection err: " + e.ToString());
                MessageBox.Show(e.Message);
            }
        }


        public void Close() {
            if(connectionETS == null)
                return;

            connectionETS.Close();
        }
        #endregion
    }
}
