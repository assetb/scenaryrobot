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

namespace ETSRobot_v2.model
{
    public class AutoOrder:BaseVM
    {
        #region Variables
        #endregion

        #region Methods
        public AutoOrder() {
            Broker = new List<Broker>();

            Broker.Add(new Broker { id = 430, name = "ALTA" });
            Broker.Add(new Broker { id = 443, name = "KORD" });
            Broker.Add(new Broker { id = 447, name = "TRN8" });
            Broker.Add(new Broker { id = 448, name = "TRN9" });
            //Broker.Add(new Broker { id = 449, name = "TRN10" });
            //Broker.Add(new Broker { id = 452, name = "TRN11" });
            Broker.Add(new Broker { id = 455, name = "ALTK" });
            Broker.Add(new Broker { id = 470, name = "AKAL" });

            BrokerInd = 0;
        }
        #endregion

        #region Bindings
        public int Number { get; set; }

        private int brokerInd;
        public int BrokerInd {
            get { return brokerInd; }
            set { brokerInd = value;RaisePropertyChangedEvent("BrokerInd"); }
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


        private String lotName;
        public String LotName {
            get { return lotName; }
            set { lotName = value; RaisePropertyChangedEvent("LotName"); }
        }


        private decimal nominal;
        public decimal Nominal {
            get { return nominal; }
            set { nominal = value; RaisePropertyChangedEvent("Nominal"); }
        }


        private int procent;
        public int Procent {
            get { return procent; }
            set { procent = value; RaisePropertyChangedEvent("Procent"); }
        }
        #endregion
    }
}
