using System;
using System.IO;
using System.Threading.Tasks;

namespace ETSRobot_v2.service {
    public class AppJournal {
        public static void Write(string sender, string message, bool withDateTime = true) {
            try {
                string fileName = "serverApp_" + DateTime.Now.ToShortDateString().Replace(".", "_") + ".log";

                File.AppendAllLines(fileName, new string[1] { (withDateTime ? DateTime.Now.ToString() + "| " : "") + sender + ": " + message });
            } catch(Exception) { }
        }
    }
}


