using System;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using altaik.baseapp.vm;
using System.Windows.Input;
using altaik.baseapp.vm.command;
using System.Windows;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Windows.Media;

namespace ETSRobot_v2.model
{
    [Serializable()]
    public class ScenaryOrder:BaseVM
    {
        #region Methods
        public ScenaryOrder() {
            Broker = new List<Broker>();

            Broker.Add(new Broker { id = 430, name = "ALTA" });
            Broker.Add(new Broker { id = 443, name = "KORD" });
            Broker.Add(new Broker { id = 447, name = "TRN8" });
            Broker.Add(new Broker { id = 448, name = "TRN9" });
            Broker.Add(new Broker { id = 449, name = "TRN10" });
            Broker.Add(new Broker { id = 452, name = "TRN11" });
            Broker.Add(new Broker { id = 455, name = "ALTK" });
            Broker.Add(new Broker { id = 470, name = "AKAL" });

            LotNumber = "0G";
            PriceTo = 0;            
        }
        #endregion

        #region Bindings
        public int Number { get; set; }


        private String comments;
        public String Comments {
            get { return comments; }
            set { comments = value;RaisePropertyChangedEvent("Comments"); }
        }


        private List<Broker> broker;        
        public List<Broker> Broker {
            get { return broker; }
            set { broker = value; RaisePropertyChangedEvent("broker"); }
        }


        private Broker selectedBroker;
        public Broker SelectedBroker {
            get { return selectedBroker; }
            set { selectedBroker = value;RaisePropertyChangedEvent("SelectedBroker"); }
        }


        private String clientCode;
        public String ClientCode {
            get { return clientCode; }
            set { clientCode = value; RaisePropertyChangedEvent("ClientCode"); }
        }


        private String lotNumber;
        public String LotNumber {
            get { return lotNumber; }
            set { lotNumber = value; RaisePropertyChangedEvent("LotNumber"); }
        }


        private decimal priceTo;
        public decimal PriceTo {
            get { return priceTo; }
            set { priceTo = value; RaisePropertyChangedEvent("PriceTo"); }
        }


        private String timeWhen;
        public String TimeWhen {
            get { return timeWhen; }
            set { timeWhen = value; RaisePropertyChangedEvent("TimeWhen"); }
        }
        #endregion
    }
}
