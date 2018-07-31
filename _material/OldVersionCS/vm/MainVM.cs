using altaik.baseapp.vm;
using altaik.baseapp.vm.command;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ETSRobot_v2.model;
using ETSRobot_v2.model.service;
using ETSRobot_v2.service;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Windows.Threading;

namespace ETSRobot_v2.vm {
    public class MainVM : BaseVM {
        #region Variables
        private DbETSManager dbETSManager;
        private ObservableCollection<Order> orderData;
        private ObservableCollection<AutoOrder> autoOrderData;
        private ObservableCollection<ScenaryOrder> scenaryOrderData;
        private DataManager dataMananger;
        private int modeType = 2;
        private Broker curBroker = new Broker();
        private DispatcherTimer dTimer = new DispatcherTimer();
        private DataBaseManager dataBaseManager;
        #endregion

        #region Methods
        public MainVM() {
            AppJournal.Write("MainVM", "Start programm");

            DefaultParametrs();
        }

        private void DefaultParametrs() {
            AppJournal.Write("MainVM", "Set default parametrs");

            dbETSManager = new DbETSManager();

            orderData = new ObservableCollection<Order>();
            autoOrderData = new ObservableCollection<AutoOrder>();
            scenaryOrderData = new ObservableCollection<ScenaryOrder>();
            dataMananger = new DataManager();

            // Mode configuration
            ModeFirstCmdChk = false;
            ModeSimpleCmdChk = true;
            ModeScenaryCmdChk = false;
            AddCmdEnable = false;
            DeleteCmdEnable = false;
            AutoOrdersVisible = Visibility.Hidden;
            OrdersVisible = Visibility.Visible;
            ScenaryOrdersVis = Visibility.Hidden;
            TableTitleTxt = "Таблица лотов и их статус в обычном режиме";

            // Default broker            
            CheckCurBroker();

            // Make watch
            dTimer.Tick += new EventHandler(dTimerTick);
            dTimer.Interval = new TimeSpan(0, 0, 1);
            dTimer.Start();

            // DB Connection
            if(App.hddSerialNumber != "helloworld") dataBaseManager = new DataBaseManager();
        }


        private void CheckCurBroker() {
            AppJournal.Write("MainVM", "Check for current broker");

            Process[] procs = Process.GetProcesses();

            String bName = "";

            foreach(var item in procs) {
                if(item.MainWindowTitle.Contains("ETS Plaza Workstation")) {
                    bName = item.MainWindowTitle.Substring(item.MainWindowTitle.IndexOf("[") + 1, 4);
                }
            }

            SetBroker(bName.ToUpper());
        }


        private void dTimerTick(object sender, EventArgs e) {
            CTime = DateTime.Now.ToLongTimeString();
        }


        public ICommand ModeFirstCmd { get { return new DelegateCommand(ModeFirst); } }
        private void ModeFirst() {
            modeType = 1;

            ModeFirstCmdChk = true;
            ModeSimpleCmdChk = false;
            ModeScenaryCmdChk = false;

            AddCmdEnable = true;
            DeleteCmdEnable = true;
            AutoOrdersVisible = Visibility.Visible;
            OrdersVisible = Visibility.Hidden;
            ScenaryOrdersVis = Visibility.Hidden;

            TableTitleTxt = "Таблица лотов и их статус в режиме [авто подачи]";
        }


        public ICommand ModeSimpleCmd { get { return new DelegateCommand(ModeSimple); } }
        private void ModeSimple() {
            modeType = 2;

            ModeFirstCmdChk = false;
            ModeSimpleCmdChk = true;
            ModeScenaryCmdChk = false;

            AddCmdEnable = false;
            DeleteCmdEnable = false;
            AutoOrdersVisible = Visibility.Hidden;
            OrdersVisible = Visibility.Visible;
            ScenaryOrdersVis = Visibility.Hidden;

            TableTitleTxt = "Таблица лотов и их статус в [обычном режиме]";
        }


        public ICommand ModeScenaryCmd { get { return new DelegateCommand(ModeScenary); } }
        private void ModeScenary() {
            modeType = 3;

            ModeFirstCmdChk = false;
            ModeSimpleCmdChk = false;
            ModeScenaryCmdChk = true;

            AddCmdEnable = true;
            DeleteCmdEnable = true;

            AutoOrdersVisible = Visibility.Hidden;
            OrdersVisible = Visibility.Hidden;
            ScenaryOrdersVis = Visibility.Visible;

            TableTitleTxt = "Таблица лотов и их статус в [сценарном режиме]";
        }


        public ICommand AddCmd { get { return new DelegateCommand(AddOrder); } }
        private void AddOrder() {
            AppJournal.Write("MainVM", "Add order record");

            if(modeType == 2) {
                orderData.Add(new Order {
                    Number = (orderData.Count + 1),
                    FirmBroker = dataMananger.firmData,
                    Client = dataMananger.clientData,
                    Issue = dataMananger.issueData,
                    BottomPrice = 0,
                    Step = 0,
                    LastPrice = 0,
                    CurrentPrice = 0,
                    ModeType = dataMananger.modeData,
                    Place = 0,
                    StartCmdName = "Запуск"
                });

                OrdersVM orderVM = new OrdersVM();
                orderVM.Orders = orderData;
                OrdersVM = orderVM;
            } else if(modeType == 1) {
                autoOrderData.Add(new AutoOrder { Number = (autoOrderData.Count + 1) });

                AutoOrdersVM autoOrderVM = new AutoOrdersVM();
                autoOrderVM.AutoOrders = autoOrderData;
                AutoOrdersVM = autoOrderVM;
            } else {
                scenaryOrderData.Add(new ScenaryOrder { Number = (scenaryOrderData.Count + 1), TimeWhen = DateTime.Now.ToLongTimeString() });

                ScenaryOrdersVM scenaryOrderVM = new ScenaryOrdersVM();
                scenaryOrderVM.ScenaryOrders = scenaryOrderData;
                ScenaryOrdersVM = scenaryOrderVM;

                if(curBroker.name == null) SetBroker("ALTA");

                ScenaryOrdersVM.ScenaryOrders[ScenaryOrdersVM.ScenaryOrders.Count - 1].Broker.FindLast(x => x.name == curBroker.name).name = "ALTA";
                ScenaryOrdersVM.ScenaryOrders[ScenaryOrdersVM.ScenaryOrders.Count - 1].Broker.FindLast(x => x.id == curBroker.id).id = 430;
                ScenaryOrdersVM.ScenaryOrders[ScenaryOrdersVM.ScenaryOrders.Count - 1].Broker[0].name = curBroker.name;
                ScenaryOrdersVM.ScenaryOrders[ScenaryOrdersVM.ScenaryOrders.Count - 1].Broker[0].id = curBroker.id;
            }

            ServerLogsTxt += "Заявка добавлена\n";
        }


        public ICommand DeleteCmd { get { return new DelegateCommand(DeleteOrder); } }
        private void DeleteOrder() {
            AppJournal.Write("MainVM", "Delete order record");

            if(modeType == 2) orderData.Remove(OrdersVM.SelectedOrder);
            else if(modeType == 1) autoOrderData.Remove(AutoOrdersVM.SelectedAutoOrder);
            else scenaryOrderData.Remove(ScenaryOrdersVM.SelectedScenaryOrder);

            ServerLogsTxt += "Заявка удалена\n";
        }


        public ICommand ConnectCmd { get { return new DelegateCommand(Connect); } }
        private void Connect() {
            AppJournal.Write("MainVM", "Start connecting to ETS");

            dataMananger.MakeModes();

            if(dbETSManager.Connected(curBroker.name)) {
                ServerLogsTxt += "Соединение с сервером установлено\n";

                if(dataMananger.FillIssueData()) ServerLogsTxt += "Данные инструментов (лотов) загружены\n";
                else ServerLogsTxt += "Данные инструментов (лотов) не загружены. Проверьте соединение!\n";

                if(dataMananger.FillFirmBrokerData()) ServerLogsTxt += "Данные фирм брокеров загружены\n";
                else ServerLogsTxt += "Данные фирм брокеров не загружены. Проверьте соединение!\n";

                if(dataMananger.FillClientData()) {
                    ServerLogsTxt += "Данные клиентов загружены\n";
                    AddCmdEnable = true;
                    DeleteCmdEnable = true;
                } else ServerLogsTxt += "Данные клиентов не загружены. Проверьте соединение!\n";
            } else ServerLogsTxt = "Соединение с сервером не установлено\n";
        }


        private void InitConnect() {
            if(dbETSManager.Connected(curBroker.name)) {
                ServerLogsTxt += "Начальное Соединение с сервером установлено\n";
            } else
                ServerLogsTxt = "Начальное Соединение с сервером не установлено\n";
        }


        RestrictLoadedDataManager restrictedDB;

        private void AutoConnect(List<ProcessedLot> plots) {
            InitConnect();

            restrictedDB = new RestrictLoadedDataManager(dataMananger);
            restrictedDB.SetForIssue(plots);

            if(restrictedDB.FillIssueData()) ServerLogsTxt += "Данные инструментов (лотов) загружены\n";
            else ServerLogsTxt += "Данные инструментов (лотов) не загружены. Проверьте соединение!\n";
        }


        public ICommand AppCloseCmd { get { return new DelegateCommand(CloseApp); } }
        private void CloseApp() {
            dbETSManager.Close();
            Environment.Exit(0);
        }


        public ICommand AutoSendCmd { get { return new DelegateCommand(AutoSend); } }
        private void AutoSend() {
            List<ProcessedLot> pLots = new List<ProcessedLot>();

            if(modeType == 1) {
                if(AutoOrdersVM.AutoOrders.Count != 0) {
                    foreach(var autoOrder in AutoOrdersVM.AutoOrders) {
                        pLots.Add(new ProcessedLot {
                            lotNo = autoOrder.LotName.ToUpper(), startPrice = autoOrder.Nominal, brokerId = autoOrder.SelectedBroker.id,
                            clientCode = autoOrder.ClientCode, percent = autoOrder.Procent
                        });
                    }
                }
            } else if(modeType == 3) {
                if(ScenaryOrdersVM.ScenaryOrders.Count != 0) {
                    foreach(var sceneOrder in ScenaryOrdersVM.ScenaryOrders) {
                        if(sceneOrder.SelectedBroker.id == curBroker.id)
                            pLots.Add(new ProcessedLot {
                                lotNo = sceneOrder.LotNumber.ToUpper(), startPrice = sceneOrder.PriceTo, brokerId = sceneOrder.SelectedBroker.id,
                                clientCode = sceneOrder.ClientCode, timeWhen = sceneOrder.TimeWhen
                            });
                        else sceneOrder.Comments = "not applicant";
                    }

                    ServerLogsTxt += "\n" + "Count of lots is " + pLots.Count.ToString() + "\n";
                }
            }

            AutoConnect(pLots);

            Task timerTask = new Task(() => LotsPass(pLots));
            timerTask.Start();
        }


        private bool isBusy;
        private int statusId = 0;

        private void LotsPass(List<ProcessedLot> plots) {
            DSSERVERLib.GMsgQuoteS[] msgQuotes = new DSSERVERLib.GMsgQuoteS[plots.Count];
            Task[] timerTask = new Task[plots.Count];
            Task[] baseTask = new Task[4];

            int iCount = 0, iTask = 0;

            foreach(var plot in plots) {
                isBusy = true;
                decimal tmpPrice = 0;
                statusId = 0;

                if(modeType == 1) tmpPrice = GetPrice(plot.startPrice, plot.percent);
                if(modeType == 3) tmpPrice = plot.startPrice;

                msgQuotes[iCount] = GetNewMsg(plot.lotNo, tmpPrice.ToString(), plot.clientCode, plot.brokerId);

                if(modeType == 1) timerTask[iCount] = new Task(() => AutoSendTask(msgQuotes[iCount]));
                if(modeType == 3) timerTask[iCount] = new Task(() => AutoSendTask(msgQuotes[iCount], plot.timeWhen, plot.startPrice));

                timerTask[iCount].Start();

                while(isBusy) ;

                if(modeType == 3) {
                    foreach(var item in ScenaryOrdersVM.ScenaryOrders) {
                        if(item.TimeWhen == plot.timeWhen && item.LotNumber.ToUpper() == plot.lotNo.ToUpper() && item.PriceTo == plot.startPrice) {
                            switch(statusId) {
                                case 0:
                                    break;
                                case 1:
                                    item.Comments = "added";
                                    break;
                                case 2:
                                    item.Comments = "error";
                                    break;
                            }
                        }
                    }
                }
                iCount++;
            }
        }


        private decimal GetPrice(decimal price, int percent) {
            return Math.Round(price - (price / Convert.ToDecimal(100) * Convert.ToDecimal(percent) - Convert.ToDecimal(1)));
        }


        private DSSERVERLib.GMsgQuoteS GetNewMsg(String lotNo, String price, String clientCode, int brokerId) {
            DSSERVERLib.GMsgQuoteS msg = new DSSERVERLib.GMsgQuoteS();

            msg.Msg_action = 78;
            msg.Id = 0;
            msg.type = 65;
            msg.IssueID = restrictedDB.GetIssueId(lotNo);
            msg.Issue_name = lotNo;
            msg.Type_wks = 1;
            msg.Price = price;
            msg.Qty = 1;
            msg.Paycond = 84;
            msg.Dcc = "";
            msg.Delivery_days = 10;
            msg.Settl_pair = clientCode;
            msg.Mm = 0;
            msg.Leave = 1;
            msg.E_s = 0;
            msg.FirmID = brokerId;

            return msg;
        }


        private string info = "";
        private void AutoSendTask0(DSSERVERLib.GMsgQuoteS msg) {
            while(!info.Contains("has been added")) msg.Send(out info);
        }


        private void AutoSendTask(DSSERVERLib.GMsgQuoteS msg, String tWhen = "", decimal price = 0) {
            if(modeType == 1) {
                while(!info.Contains("has been added")) {
                    msg.Send(out info);
                    isBusy = false;
                }
            } else if(modeType == 3) {
                while(isBusy) {
                    if(Convert.ToInt32(tWhen.Substring(0, 2)) <= Convert.ToInt32(DateTime.Now.Hour)) {
                        if(Convert.ToInt32(tWhen.Substring(3, 2)) <= Convert.ToInt32(DateTime.Now.Minute)) {
                            if(Convert.ToInt32(tWhen.Substring(6, 2)) <= Convert.ToInt32(DateTime.Now.Second)) {
                                msg.Send(out info);
                                ServerLogsTxt += "\n" + tWhen + " - " + info;

                                if(info.Contains("has been added")) statusId = 1;
                                else statusId = 2;

                                isBusy = false;
                            }
                        }
                    }
                }
            }
        }


        public ICommand ScenaryLoadCmd { get { return new DelegateCommand(ScenaryLoad); } }
        private void ScenaryLoad() {
            OpenFileDialog openFD = new OpenFileDialog();
            openFD.Filter = "Scenary files (*.scn)|*.scn";

            if(openFD.ShowDialog() == DialogResult.OK) {
                ObservableCollection<ScenaryOrder> tmpSO = new ObservableCollection<ScenaryOrder>();

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<ScenaryOrder>));
                TextReader FileStream = new StreamReader(openFD.FileName);

                tmpSO = (ObservableCollection<ScenaryOrder>)xmlSerializer.Deserialize(FileStream);

                FileStream.Close();
                scenaryOrderData.Clear();

                int iCount = 0;

                foreach(var item in tmpSO) {
                    scenaryOrderData.Add(new ScenaryOrder {
                        Number = item.Number,
                        ClientCode = item.ClientCode,
                        LotNumber = item.LotNumber,
                        PriceTo = item.PriceTo,
                        SelectedBroker = item.SelectedBroker,
                        TimeWhen = item.TimeWhen
                    });

                    ScenaryOrdersVM scenaryOrderVM = new ScenaryOrdersVM();
                    scenaryOrderVM.ScenaryOrders = scenaryOrderData;
                    ScenaryOrdersVM = scenaryOrderVM;
                    ScenaryOrdersVM.ScenaryOrders[iCount].Broker.FindLast(x => x.name == item.SelectedBroker.name).name = "ALTA";
                    ScenaryOrdersVM.ScenaryOrders[iCount].Broker.FindLast(x => x.id == item.SelectedBroker.id).id = 430;
                    ScenaryOrdersVM.ScenaryOrders[iCount].Broker[0].name = item.SelectedBroker.name;
                    ScenaryOrdersVM.ScenaryOrders[iCount].Broker[0].id = item.SelectedBroker.id;

                    iCount++;
                }
            }
        }


        public ICommand ScenarySaveCmd { get { return new DelegateCommand(ScenarySave); } }
        private void ScenarySave() {
            SaveFileDialog saveFD = new SaveFileDialog();

            saveFD.Filter = "Scenary files (*.scn)|*.scn";

            if(saveFD.ShowDialog() == DialogResult.OK) {
                if(File.Exists(saveFD.FileName)) File.Delete(saveFD.FileName);

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<ScenaryOrder>));
                TextWriter FileStream = new StreamWriter(saveFD.FileName);

                xmlSerializer.Serialize(FileStream, scenaryOrderData);
                FileStream.Close();
            }
        }


        public ICommand AltaCmd { get { return new DelegateCommand(() => SetBroker("ALTA")); } }
        public ICommand KordCmd { get { return new DelegateCommand(() => SetBroker("KORD")); } }
        public ICommand AkalCmd { get { return new DelegateCommand(() => SetBroker("AKAL")); } }
        public ICommand AltkCmd { get { return new DelegateCommand(() => SetBroker("ALTK")); } }
        public ICommand Trn8Cmd { get { return new DelegateCommand(() => SetBroker("TRN8")); } }
        public ICommand Trn9Cmd { get { return new DelegateCommand(() => SetBroker("TRN9")); } }
        public ICommand Trn10Cmd { get { return new DelegateCommand(() => SetBroker("TRN10")); } }
        public ICommand Trn11Cmd { get { return new DelegateCommand(() => SetBroker("TRN11")); } }

        private void SetBroker(String broker) {
            AppJournal.Write("MainVM", "Set broker");

            AltaCmdChk = false;
            KordCmdChk = false;
            AkalCmdChk = false;
            AltkCmdChk = false;
            Trn8CmdChk = false;
            Trn9CmdChk = false;
            Trn10CmdChk = false;
            Trn11CmdChk = false;

            switch(broker) {
                case "ALTA": AltaCmdChk = true; curBroker.id = 430; curBroker.name = broker; CurBroker = "Альтаир Нур"; break;
                case "KORD": KordCmdChk = true; curBroker.id = 443; curBroker.name = broker; CurBroker = "Корунд-777"; break;
                case "AKAL": AkalCmdChk = true; curBroker.id = 470; curBroker.name = broker; CurBroker = "Ак Алтын Ко"; break;
                case "ALTK": AltkCmdChk = true; curBroker.id = 455; curBroker.name = broker; CurBroker = "Альта и К"; break;
                case "TRN8": Trn8CmdChk = true; curBroker.id = 447; curBroker.name = broker; CurBroker = "Тестовый TRN8"; break;
                case "TRN9": Trn9CmdChk = true; curBroker.id = 448; curBroker.name = broker; CurBroker = "Тестовый TRN9"; break;
                case "TRN10": Trn10CmdChk = true; curBroker.id = 449; curBroker.name = broker; CurBroker = "Тестовый TRN10"; break;
                case "TRN11": Trn11CmdChk = true; curBroker.id = 452; curBroker.name = broker; CurBroker = "Тестовый TRN11"; break;
            }
        }


        public ICommand ScenarySortCmd { get { return new DelegateCommand(ScenarySort); } }
        private void ScenarySort() {
            scenaryOrderData = new ObservableCollection<ScenaryOrder>(scenaryOrderData.OrderBy(x => x.TimeWhen));
            ScenaryOrdersVM.ScenaryOrders = scenaryOrderData;

            int i = 0, iTmp = 0;
            string nTmp = "";

            foreach(var item in scenaryOrderData) {
                nTmp = ScenaryOrdersVM.ScenaryOrders[i].Broker[0].name;
                iTmp = ScenaryOrdersVM.ScenaryOrders[i].Broker[0].id;
                ScenaryOrdersVM.ScenaryOrders[i].Number = i + 1;
                ScenaryOrdersVM.ScenaryOrders[i].Broker[0].name = item.SelectedBroker.name;
                ScenaryOrdersVM.ScenaryOrders[i].Broker[0].id = item.SelectedBroker.id;
                ScenaryOrdersVM.ScenaryOrders[i].Broker.FindLast(x => x.name == item.SelectedBroker.name).name = nTmp;
                ScenaryOrdersVM.ScenaryOrders[i].Broker.FindLast(x => x.id == item.SelectedBroker.id).id = iTmp;

                i++;
            }
        }
        #endregion

        #region Bindings
        private OrdersVM ordersVM;
        public OrdersVM OrdersVM {
            get { return ordersVM; }
            set { ordersVM = value; RaisePropertyChangedEvent("OrdersVM"); }
        }

        private AutoOrdersVM autoOrdersVM;
        public AutoOrdersVM AutoOrdersVM {
            get { return autoOrdersVM; }
            set { autoOrdersVM = value; RaisePropertyChangedEvent("AutoOrdersVM"); }
        }

        private ScenaryOrdersVM scenaryOrdersVM;
        public ScenaryOrdersVM ScenaryOrdersVM {
            get { return scenaryOrdersVM; }
            set { scenaryOrdersVM = value; RaisePropertyChangedEvent("ScenaryOrdersVM"); }
        }

        public String Title {
            get { return "Автоматизированная система подачи заявок на ETS"; }
        }

        private String tableTitleTxt;
        public String TableTitleTxt {
            get { return tableTitleTxt; }
            set { tableTitleTxt = value; RaisePropertyChangedEvent("TableTitleTxt"); }
        }

        private Visibility ordersVisible;
        public Visibility OrdersVisible {
            get { return ordersVisible; }
            set { ordersVisible = value; RaisePropertyChangedEvent("OrdersVisible"); }
        }

        private Visibility autoOrdersVisible;
        public Visibility AutoOrdersVisible {
            get { return autoOrdersVisible; }
            set { autoOrdersVisible = value; RaisePropertyChangedEvent("AutoOrdersVisible"); }
        }

        private Visibility scenaryOrdersVis;
        public Visibility ScenaryOrdersVis {
            get { return scenaryOrdersVis; }
            set { scenaryOrdersVis = value; RaisePropertyChangedEvent("ScenaryOrdersVis"); }
        }

        private String serverLogsTxt;
        public String ServerLogsTxt {
            get { return serverLogsTxt; }
            set { serverLogsTxt = value; RaisePropertyChangedEvent("ServerLogsTxt"); }
        }

        private String cBroker;
        public String CurBroker {
            get { return cBroker; }
            set { cBroker = value; RaisePropertyChangedEvent("CurBroker"); }
        }

        private String cTime;
        public String CTime {
            get { return cTime; }
            set { cTime = value; RaisePropertyChangedEvent("CTime"); }
        }

        private Boolean modeFirstCmdChk;
        public Boolean ModeFirstCmdChk {
            get { return modeFirstCmdChk; }
            set { modeFirstCmdChk = value; RaisePropertyChangedEvent("ModeFirstCmdChk"); }
        }

        private Boolean altaCmdChk;
        public Boolean AltaCmdChk { get { return altaCmdChk; } set { altaCmdChk = value; RaisePropertyChangedEvent("AltaCmdChk"); } }

        private Boolean kordCmdChk;
        public Boolean KordCmdChk { get { return kordCmdChk; } set { kordCmdChk = value; RaisePropertyChangedEvent("KordCmdChk"); } }

        private Boolean akalCmdChk;
        public Boolean AkalCmdChk { get { return akalCmdChk; } set { akalCmdChk = value; RaisePropertyChangedEvent("AkalCmdChk"); } }

        private Boolean altkCmdChk;
        public Boolean AltkCmdChk { get { return altkCmdChk; } set { altkCmdChk = value; RaisePropertyChangedEvent("AltkCmdChk"); } }

        private Boolean trn8CmdChk;
        public Boolean Trn8CmdChk { get { return trn8CmdChk; } set { trn8CmdChk = value; RaisePropertyChangedEvent("Trn8CmdChk"); } }

        private Boolean trn9CmdChk;
        public Boolean Trn9CmdChk { get { return trn9CmdChk; } set { trn9CmdChk = value; RaisePropertyChangedEvent("Trn9CmdChk"); } }

        private Boolean trn10CmdChk;
        public Boolean Trn10CmdChk { get { return trn10CmdChk; } set { trn10CmdChk = value; RaisePropertyChangedEvent("Trn10CmdChk"); } }

        private Boolean trn11CmdChk;
        public Boolean Trn11CmdChk { get { return trn11CmdChk; } set { trn11CmdChk = value; RaisePropertyChangedEvent("Trn11CmdChk"); } }

        private Boolean modeSimpleCmdChk;
        public Boolean ModeSimpleCmdChk {
            get { return modeSimpleCmdChk; }
            set { modeSimpleCmdChk = value; RaisePropertyChangedEvent("ModeSimpleCmdChk"); }
        }

        private Boolean modeScenaryCmdChk;
        public Boolean ModeScenaryCmdChk {
            get { return modeScenaryCmdChk; }
            set { modeScenaryCmdChk = value; RaisePropertyChangedEvent("ModeScenaryCmdChk"); }
        }

        private Boolean addCmdEnable;
        public Boolean AddCmdEnable {
            get { return addCmdEnable; }
            set { addCmdEnable = value; RaisePropertyChangedEvent("AddCmdEnable"); }
        }

        private Boolean deleteCmdEnable;
        public Boolean DeleteCmdEnable {
            get { return deleteCmdEnable; }
            set { deleteCmdEnable = value; RaisePropertyChangedEvent("DeleteCmdEnable"); }
        }
        #endregion
    }
}