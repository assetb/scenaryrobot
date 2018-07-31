#define DEBUG
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
using ETSRobot_v2.service;

namespace ETSRobot_v2.model {
    public class Order : BaseVM {
        #region Variables
        DSSERVERLib.Online tableQuotes;
        DSSERVERLib.GMsgQuoteS msgQuotes;

        private int iCount = 0;
        private Task[] baseTask = new Task[4];
        private ScenaryOrder item;
        private DataBaseManager dataBaseManager = new DataBaseManager();
        private Task logWriter; //= new Task(LogWriter);
        #endregion

        #region Methods
        // Конструктор
        public Order() { }


        // Кнопка подачи разовой заявки
        public ICommand PreviouslyCmd { get { return new DelegateCommand(PreviouslyFunc); } }


        // Подача заявки
        private void PreviouslyFunc() {
            // Данные заявки
            msgQuotes.FirmID = SelectedFirmBroker.id;
            msgQuotes.Settl_pair = SelectedClient.settlPair;
            msgQuotes.IssueID = SelectedIssue.id;
            msgQuotes.Issue_name = SelectedIssue.name;

            // Проверка минимальной цены
            if(BottomPrice > LastPrice) {
                proc = false;
                MessageBox.Show("Минимальная цена больше стартовой");
            } else {
                string info;

                msgQuotes.Price = LastPrice;
                bool rez = msgQuotes.Send(out info);

                if(info.Contains("has been added")) {
                    // Определение ранга в торгах
                    Place = 1;
                }
            }
        }


        // Кнопка обработки заявки
        public ICommand StartCmd { get { return new DelegateCommand(StartOrder); } }


        // Обработка заявки
        private void StartOrder() {
            if(StartCmdName == "Запуск") {
                AppJournal.Write("Order", "Start set price offers process");
                // Данные заявки
                msgQuotes.FirmID = SelectedFirmBroker.id;
                msgQuotes.Settl_pair = SelectedClient.settlPair;
                msgQuotes.IssueID = SelectedIssue.id;
                msgQuotes.Issue_name = SelectedIssue.name;

                // Параметры таймера и его запуск
                StartCmdName = "Стоп";

                Task timerTask = new Task(TimerTask);

                proc = true;

                timerTask.Start();
            } else {
                AppJournal.Write("Order", "Stop price offer process");

                proc = false;
                StartCmdName = "Запуск";
            }
        }


        // Таймер обработки заявки
        private Boolean proc; // Флаг запуска

        private void TimerTask() {
            while(proc) {
                // Проверка минимальной цены
                if(BottomPrice > CurrentPrice || BottomPrice > (CurrentPrice - Step)) {
                    AppJournal.Write("Order", "Bottom price more than current");

                    proc = false;
                    StartCmdName = "Запуск";
                    MessageBox.Show("Минимальная цена больше стартовой");
                } else {
                    // Поточный режим
                    if(SelectedMode.type == "Поточный") SendOrder(1);
                    // Конкурирующий режим
                    else if(SelectedMode.type == "Конкурирующий") {
                        if(CurrentPrice < LastPrice || LastPrice == 0) SendOrder(1);
                    } else if(SelectedMode.type == "Разовый") {
                        if(LastPrice < BottomPrice) MessageBox.Show("Заявляемая цена меньше минимальной");
                        else SendOrder(2);
                    }
                }
            }
        }


        // Отправка заявки
        private string info;
        private void SendOrder(int type) {

            if(type != 2) LastPrice = CurrentPrice - Step;
            else {
                proc = false;
                StartCmdName = "Запуск";
            }

            msgQuotes.Price = LastPrice;

            bool rez = msgQuotes.Send(out info);

            if(info.Contains("has been added")) {
                //AppJournal.Write("Order", "Price offer setted");
                Place = 1;
            } else { //else MessageBox.Show("Сообщение об ошибке:\n" + info);
                //AppJournal.Write("Order", "Price offer err: " + info);
            }

            logWriter = new Task(() => LogWriter(SelectedMode.type + " | Сообщение от сервера при подаче | ", info + " | Цена подачи | " + LastPrice));
            logWriter.Start();
        }

        private void LogWriter(string sender, string msg) {
            AppJournal.Write(sender, msg);
        }

        // Котировки
        private void FillQuotesData() {
            AppJournal.Write("Order", "Trying open quotes table");

            tableQuotes = new DSSERVERLib.Online();
            tableQuotes.AddRow += TableQuotesAddRow;

            try {
                tableQuotes.Open(DSSERVERLib.ConnectionType.RTSONL_DYNAMIC, "Quote", "issue_name, price, moment, firm_name", "id", null, null, DSSERVERLib.Sort.RTSONL_SORT_EMPTY);
            } catch(Exception ex) {
                AppJournal.Write("Order", "Opening table err: " + ex.ToString());
                MessageBox.Show("Ошибка доступа к таблице котировок!");
            }
        }

        // Определение текущей цены
        private DateTime date;

        void TableQuotesAddRow(int IDConnect, int IDRecord, object Fields) {
            IList collection = (IList)Fields;
            if(collection[0].ToString() == SelectedIssue.name) {
                decimal rez;
                decimal.TryParse(collection[1].ToString(), NumberStyles.Any, new CultureInfo("en-US"), out rez);
                CurrentPrice = rez;

                FirmName = collection[3].ToString();
                /*date = new DateTime(1970, 1, 1).AddSeconds(Convert.ToInt32(collection[2].ToString()));

                logWriter = new Task(() => LogWriter("New quote | ", "Lot - " + collection[0].ToString() + ", price - " + CurrentPrice + ", time - " +
                    date.Hour + ":" + date.Minute + ":" + date.Second + ":" + date.Millisecond
                    + ", sender - " + collection[3].ToString()));
                logWriter.Start();*/
            }

            if(CurrentPrice == 0) CurrentPrice = SelectedIssue.nominal - Step;

            // Определение ранга в торгах
            if(LastPrice > CurrentPrice) Place = 2;
        }


        // Параметры заявки
        private void FillMsgQout() {
            msgQuotes = new DSSERVERLib.GMsgQuoteS();

            msgQuotes.Msg_action = 78;
            msgQuotes.Id = 0;
            msgQuotes.type = 65;
            msgQuotes.IssueID = SelectedIssue.id;
            msgQuotes.Issue_name = SelectedIssue.name;
            msgQuotes.Type_wks = 1;
            msgQuotes.Price = SelectedIssue.nominal;
            msgQuotes.Qty = 1;
            msgQuotes.Paycond = 84;
            msgQuotes.Dcc = "";
            msgQuotes.Delivery_days = 10;
            msgQuotes.Settl_pair = SelectedClient.settlPair;
            msgQuotes.Mm = 0;
            msgQuotes.Leave = 1;
            msgQuotes.E_s = 0;
        }
        #endregion

        #region Bindings
        // Номер заявки
        public int Number { get; set; }


        // Фирма-брокер
        private ObservableCollection<Firm> firmBroker = new ObservableCollection<Firm>();
        public ObservableCollection<Firm> FirmBroker {
            get { return firmBroker; }
            set { firmBroker = value; RaisePropertyChangedEvent("FirmBroker"); }
        }


        // Выбранная фирма-брокер
        private Firm selectedfirmBroker;
        public Firm SelectedFirmBroker {
            get { return selectedfirmBroker; }
            set { selectedfirmBroker = value; RaisePropertyChangedEvent("SelectedFirmBroker"); }
        }


        // Клиент
        private ObservableCollection<SettlPair> client = new ObservableCollection<SettlPair>();
        public ObservableCollection<SettlPair> Client {
            get { return client; }
            set { client = value; RaisePropertyChangedEvent("Client"); }
        }


        // Выбранный клиент
        private SettlPair selectedClient;
        public SettlPair SelectedClient {
            get { return selectedClient; }
            set { selectedClient = value; RaisePropertyChangedEvent("SelectedClient"); }
        }


        // Инструмент (лот)
        private ObservableCollection<Issue> issue = new ObservableCollection<Issue>();
        public ObservableCollection<Issue> Issue {
            get { return issue; }
            set { issue = value; RaisePropertyChangedEvent("Issue"); }
        }


        // Выбранный лот
        private Issue selectedIssue;
        public Issue SelectedIssue {
            get { return selectedIssue; }
            set { selectedIssue = value; RaisePropertyChangedEvent("SelectedIssue"); FillQuotesData(); FillMsgQout(); }
        }


        // Минимальная цена
        private Decimal bottomPrice;
        public Decimal BottomPrice {
            get {
                return bottomPrice;
            }
            set {
                bottomPrice = value; RaisePropertyChangedEvent("BottomPrice");
            }
        }


        // Шаг
        private Decimal step;
        public Decimal Step {
            get { return step; }
            set { step = value; RaisePropertyChangedEvent("Step"); }
        }


        // Последняя цена
        private Decimal lastPrice;
        public Decimal LastPrice {
            get { return lastPrice; }
            set {
                if(value != lastPrice) {
                    logWriter = new Task(() => LogWriter(SelectedMode.type + " | Подача ЦП | ", "Lot - " + SelectedIssue.name + "| price - " + LastPrice + "| time - " +
                    DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond));
                    logWriter.Start();
                }

                lastPrice = value;
                RaisePropertyChangedEvent("LastPrice");
            }
        }


        // Текущая цена
        private Decimal currentPrice;
        public Decimal CurrentPrice {
            get { return currentPrice; }
            set {
                if(value != currentPrice) {
                    logWriter = new Task(() => LogWriter(SelectedMode.type + " | Подтверждение ЦП | ", "Lot - " + SelectedIssue.name + "| price - " + CurrentPrice + "| time - " +
                    DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond + "| firm - " + FirmName));
                    logWriter.Start();
                }
                currentPrice = value;
                RaisePropertyChangedEvent("CurrentPrice");
            }
        }


        // Режим
        private ObservableCollection<Mode> modeType = new ObservableCollection<Mode>();
        public ObservableCollection<Mode> ModeType {
            get { return modeType; }
            set { modeType = value; RaisePropertyChangedEvent("ModeType"); }
        }


        // Выбранный режим
        private Mode selectedMode;
        public Mode SelectedMode {
            get { return selectedMode; }
            set { selectedMode = value; RaisePropertyChangedEvent("SelectedMode"); }
        }


        // Текущее место в рейтинге
        private int place;
        public int Place {
            get { return place; }
            set { place = value; RaisePropertyChangedEvent("Place"); }
        }


        // Именованный статус кнопки
        private String startCmdName;
        public String StartCmdName {
            get { return startCmdName; }
            set { startCmdName = value; RaisePropertyChangedEvent("StartCmdName"); }
        }

        private string _firmName;
        public string FirmName {
            get { return _firmName; }
            set { _firmName = value; RaisePropertyChangedEvent("FirmName"); }
        }
        #endregion
    }
}
