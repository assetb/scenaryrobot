using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltaETSR.services {
    public class Loger {
        public static void Write(string sender, string message, bool withDateTime = true) {
            try {
                string fileName = "logs_" + DateTime.Now.ToShortDateString().Replace(".", "_") + ".log";

                File.AppendAllLines(fileName, new string[1] { (withDateTime ? DateTime.Now.ToString() + " | " : "") + sender + ": " + message });
            } catch(Exception) { }
        }
    }
}
