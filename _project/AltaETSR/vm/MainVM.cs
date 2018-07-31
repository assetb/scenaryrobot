using BObjects;
using ETSApp;
using MVVMApp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using AltaETSR.services;
using System;

namespace AltaETSR.vm {
    public class MainVM : MVVM {
        #region Variables
        private Task scenary;
        private bool isScenaryGo = false;
        #endregion

        #region Methods
        public MainVM() {
            Loger.Write("MainVM", "Start programm");
            Loger.Write("MainVM", "Sending mail about run programm");

            MonitorTxt = DateTime.Now + " | Программа запущенна " + "\n" + MonitorTxt;

            MailSender.SendMail();
            DefaultParametrs();
        }


        private void DefaultParametrs() {
            Loger.Write("MainVM", "Set default params");

            BrokersList = Tables.GetBrokers();

            SelectedBroker = BrokersList[0];
            CurBrokersList = BrokersList;
            SelectedCurBroker = CurBrokersList[0];
            SecPerPriceOfferTxT = 15;
        }


        public ICommand AddCmd { get { return new MVVMCommand(Add); } }
        private void Add() {
            Loger.Write("MainVM", "Add scenary record");

            MonitorTxt = DateTime.Now + " | Добавление сценарной записи " + "\n" + MonitorTxt;

            if (!string.IsNullOrEmpty(LotCodeTxt) && !string.IsNullOrEmpty(ClientCodeTxt)) {
                Loger.Write("MainVM", "Scenary record added");

                MonitorTxt = DateTime.Now + " | Запись добавленна " + "\n" + MonitorTxt;

                ScenaryList.Add(new ScenaryLot() {
                    number = ScenaryList.Count + 1,
                    brokerCode = SelectedBroker.brokerCode,
                    lotCode = LotCodeTxt,
                    clientCode = ClientCodeTxt,
                    priceOffer = PriceOfferTxt,
                    status = false
                });
            } else {
                Loger.Write("MainVM", "Adding scenary record fault, because not fields is set");

                MonitorTxt = DateTime.Now + " | Ошибка добавления записи " + "\n" + MonitorTxt;

                MessageBox.Show("Введены не все данные.");
            }
        }


        public ICommand UpdateCmd { get { return new MVVMCommand(Update); } }
        private void Update() {
            Loger.Write("MainVM", "Update scenary record");

            MonitorTxt = DateTime.Now + " | Обновление сценарной записи " + "\n" + MonitorTxt;

            if (SelectedScenary != null) {
                SelectedScenary.brokerCode = SelectedBroker.brokerCode;
                SelectedScenary.lotCode = LotCodeTxt;
                SelectedScenary.clientCode = ClientCodeTxt;
                SelectedScenary.priceOffer = PriceOfferTxt;
                ScenaryList = new ObservableCollection<ScenaryLot>(ScenaryList);

                Loger.Write("MainVM", "Scenary record updated");

                MonitorTxt = DateTime.Now + " | Запись обновленна " + "\n" + MonitorTxt;
            } else {
                Loger.Write("MainVM", "Scenary record update fault because item not selected");

                MonitorTxt = DateTime.Now + " | Ошибка обновления записи " + "\n" + MonitorTxt;

                MessageBox.Show("Нет выбранной записи для обновления.");
            }
        }


        public ICommand DeleteCmd { get { return new MVVMCommand(Delete); } }
        private void Delete() {
            Loger.Write("MainVM", "Scenary record deleting");

            MonitorTxt = DateTime.Now + " | Удаление сценарной записи " + "\n" + MonitorTxt;

            if (SelectedScenary != null) {
                ScenaryList.Remove(SelectedScenary);
                SetSerialNumbers();

                Loger.Write("MainVM", "Scenary record deleted");

                MonitorTxt = DateTime.Now + " | Запись удалена " + "\n" + MonitorTxt;
            } else {
                Loger.Write("MainVM", "Scenary record deleting fault because item not selected");

                MonitorTxt = DateTime.Now + " | Ошибка удаления записи " + "\n" + MonitorTxt;

                MessageBox.Show("Нет выбранной записи для удаления.");
            }
        }


        public ICommand NewCmd { get { return new MVVMCommand(New); } }
        private void New() {
            Loger.Write("MainVM", "Clear scenary list");

            MonitorTxt = DateTime.Now + " | Создание нового сценария " + "\n" + MonitorTxt;
            ScenaryList = new ObservableCollection<ScenaryLot>();
        }


        public ICommand SaveCmd { get { return new MVVMCommand(Save); } }
        private void Save() {
            Loger.Write("MainVM", "Save scenary");

            MonitorTxt = DateTime.Now + " | Сохранение сценария " + "\n" + MonitorTxt;

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "Файлы сценария (*.scn)|*.scn";

            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                if (File.Exists(saveFileDialog.FileName)) File.Delete(saveFileDialog.FileName);

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<ScenaryLot>));
                TextWriter FileStream = new StreamWriter(saveFileDialog.FileName);

                xmlSerializer.Serialize(FileStream, ScenaryList);
                FileStream.Close();

                MessageBox.Show("Сценарий сохранен.");
                Loger.Write("MainVM", "Scenary saved");

                MonitorTxt = DateTime.Now + " | Сохранение прошло успешно " + "\n" + MonitorTxt;
            } else {
                Loger.Write("MainVM", "Scenary save canceled");

                MonitorTxt = DateTime.Now + " | Отмена сохранения " + "\n" + MonitorTxt;
            }
        }


        public ICommand LoadCmd { get { return new MVVMCommand(Load); } }
        private void Load() {
            Loger.Write("MainVM", "Scenary loading");

            MonitorTxt = DateTime.Now + " | Загрузка сценария " + "\n" + MonitorTxt;

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Файлы сценария (*.scn)|*.scn";

            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                ObservableCollection<ScenaryLot> tmpSO = new ObservableCollection<ScenaryLot>();
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<ScenaryLot>));
                TextReader FileStream = new StreamReader(openFileDialog.FileName);

                tmpSO = (ObservableCollection<ScenaryLot>)xmlSerializer.Deserialize(FileStream);

                FileStream.Close();
                ScenaryList.Clear();

                foreach (var item in tmpSO) {
                    ScenaryList.Add(item);
                }

                MessageBox.Show("Сценарий загружен.");
                Loger.Write("MainVM", "Scenary loaded");

                MonitorTxt = DateTime.Now + " | Загрузка выполненна " + "\n" + MonitorTxt;
            } else {
                Loger.Write("MainVM", "Scenary loading canceled");

                MonitorTxt = DateTime.Now + " | Отмена загрузки " + "\n" + MonitorTxt;
            }
        }


        public ICommand RunCmd { get { return new MVVMCommand(Run); } }
        private void Run() {
            Loger.Write("MainVM", "Run scenary");

            MonitorTxt = DateTime.Now + " | Запуск сценария " + "\n" + MonitorTxt;

            if (InitConnect()) {
                scenary = new Task(ScenaryActions);
                isScenaryGo = true;

                scenary.Start();
            }
        }


        private bool InitConnect() {
            if (CheckCurBroker()) {
                if (Connections.GetConnection((SelectedCurBroker.id == 447 || SelectedCurBroker.id == 448) ? true : false)) {
                    MonitorTxt = DateTime.Now + " | Соединение с базой ЕТС установленно " + "\n" + MonitorTxt;

                    if (Connections.GetLotsConnection() > 0) {
                        MonitorTxt = DateTime.Now + " | Таблица лотов подключена " + "\n" + MonitorTxt;

                        if (Connections.GetQuotesConnection() > 0) {
                            MonitorTxt = DateTime.Now + " | Таблица котировок подключена " + "\n" + MonitorTxt;

                            return true;
                        } else {
                            MessageBox.Show("Ошибка подключения к таблице котировок.");
                            Loger.Write("MainVM", "Connection to quotes error");

                            MonitorTxt = DateTime.Now + " | Ошибка подключения к таблице котировок " + "\n" + MonitorTxt;
                        }
                    } else {
                        MessageBox.Show("Ошибка подключения к таблице лотов.");
                        Loger.Write("MainVM", "Connection to lots error");

                        MonitorTxt = DateTime.Now + " | Ошибка подключения к таблице лотов " + "\n" + MonitorTxt;
                    }
                } else {
                    MessageBox.Show("Ошибка подключения к ЕТС.");
                    Loger.Write("MainVM", "Connection to ETS error");

                    MonitorTxt = DateTime.Now + " | Ошибка подключения к базе ЕТС " + "\n" + MonitorTxt;
                }
            } else {
                MessageBox.Show("ЕТС Плаза не запущенна.");
                Loger.Write("MainVM", "Connection faulted because ETS Plaza not started or unknown broker");

                MonitorTxt = DateTime.Now + " | ЕТС плаза не запущенна " + "\n" + MonitorTxt;
            }

            return false;
        }


        private bool CheckCurBroker() {
            Loger.Write("MainVM", "Get cur broker from ETS Plaza");

            MonitorTxt = DateTime.Now + " | Получение текущего брокера из плазы " + "\n" + MonitorTxt;
            Process[] procs = Process.GetProcesses();

            string bName = "";

            foreach (var item in procs) {
                if (item.MainWindowTitle.Contains("ETS Plaza Workstation")) {
                    bName = item.MainWindowTitle.Substring(item.MainWindowTitle.IndexOf("[") + 1, 4);
                }
            }

            if (string.IsNullOrEmpty(bName)) return false;
            else {
                var curBroker = CurBrokersList.FirstOrDefault(c => c.brokerCode.ToLower() == bName.ToLower());

                if (curBroker != null) {
                    SelectedCurBroker = curBroker;
                    Loger.Write("MainVM", "Broker setted");

                    MonitorTxt = DateTime.Now + " | Текущий брокер установлен " + "\n" + MonitorTxt;

                    return true;
                } else {
                    Loger.Write("MainVM", "Broker setting fault");

                    MonitorTxt = DateTime.Now + " | Ошибка установки текущего брокера " + "\n" + MonitorTxt;

                    return false;
                }
            }
        }


        private void ScenaryActions() {
            Loger.Write("MainVM", "Start scenary proccess");

            MonitorTxt = DateTime.Now + " | Процес сценария запущен " + "\n" + MonitorTxt;

            int iAct = 0;
            int actQuantity = ScenaryList.Count;

            while (isScenaryGo && iAct < actQuantity) {
                Loger.Write("MainVM", "Check for record status");

                MonitorTxt = DateTime.Now + " | Проверка статуса записи " + "\n" + MonitorTxt;

                if (!ScenaryList[iAct].status) {
                    Loger.Write("MainVM", "Check for record broker");

                    MonitorTxt = DateTime.Now + " | Проверка брокера в записи " + "\n" + MonitorTxt;

                    if (ScenaryList[iAct].brokerCode == SelectedCurBroker.brokerCode) {
                        Loger.Write("MainVM", "Sending offer with scenary params");

                        MonitorTxt = DateTime.Now + " | Отправка ценового предложения " + "\n" + MonitorTxt;

                        if (SendingOffers.SendPriceOffer(ScenaryList[iAct], SecPerPriceOfferTxT)) {
                            ScenaryList[iAct].status = true;
                            ScenaryList = new ObservableCollection<ScenaryLot>(ScenaryList);
                            iAct++;

                            Loger.Write("MainVM", "Offer sended");

                            MonitorTxt = DateTime.Now + " | Ценовое предложение отправленно " + "\n" + MonitorTxt;
                        } else {
                            isScenaryGo = false;

                            Loger.Write("MainVM", "Stop scenary proccess because some fault");

                            MonitorTxt = DateTime.Now + " | Ошибка отправки ценового предложения " + "\n" + MonitorTxt;
                        }
                    } else {
                        Loger.Write("MainVM", "Check for quotes");

                        MonitorTxt = DateTime.Now + " | Проверка котировок " + "\n" + MonitorTxt;

                        var quotes = new List<Quote>(Tables.GetQuotes());

                        if (quotes != null && quotes.Count > 0) {
                            Loger.Write("MainVM", "Search needed quote");

                            MonitorTxt = DateTime.Now + " | Поиск необходимой котировки " + "\n" + MonitorTxt;

                            var quote = quotes.FirstOrDefault(q => q.lotCode.ToLower() == ScenaryList[iAct].lotCode.ToLower() && q.priceOffer == ScenaryList[iAct].priceOffer);

                            if (quote != null) {
                                ScenaryList[iAct].status = true;

                                ScenaryList = new ObservableCollection<ScenaryLot>(ScenaryList);

                                iAct++;

                                Loger.Write("MainVM", "Needed quote getted set scenary record to status true");

                                MonitorTxt = DateTime.Now + " | Получена необходимая котировка " + "\n" + MonitorTxt;
                            }
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }


        public ICommand StopCmd { get { return new MVVMCommand(Stop); } }
        private void Stop() {
            Loger.Write("MainVM", "Stop scenary");

            MonitorTxt = DateTime.Now + " | Остановка сценария " + "\n" + MonitorTxt;

            isScenaryGo = false;
        }


        public ICommand UpScenaryCmd { get { return new MVVMCommand(() => MoveScenary(1)); } }
        public ICommand DownScenaryCmd { get { return new MVVMCommand(() => MoveScenary(2)); } }
        private void MoveScenary(int direction) {
            if (SelectedScenary != null) {
                if (SelectedScenary.number > 1 && direction == 1) ScenaryList.Move(SelectedScenary.number - 1, SelectedScenary.number - 2);
                else if (SelectedScenary.number < ScenaryList.Count - 1 && direction == 2) ScenaryList.Move(SelectedScenary.number - 1, SelectedScenary.number);

                SetSerialNumbers();
            } else MessageBox.Show("Не выбрана запись");
        }


        private void SetSerialNumbers() {
            int iCount = 1;

            foreach (var item in ScenaryList) {
                item.number = iCount;

                iCount++;
            }

            ScenaryList = new ObservableCollection<ScenaryLot>(ScenaryList);
        }
        #endregion

        #region Bindings
        private List<Broker> _brokersList;
        public List<Broker> BrokersList {
            get { return _brokersList; }
            set { _brokersList = value; NotifyPropertyChanged("BrokersList"); }
        }


        private Broker _selectedBroker;
        public Broker SelectedBroker {
            get { return _selectedBroker; }
            set { _selectedBroker = value; NotifyPropertyChanged("SelectedBroker"); }
        }


        private List<Broker> _curBrokersList;
        public List<Broker> CurBrokersList {
            get { return _curBrokersList; }
            set { _curBrokersList = value; NotifyPropertyChanged("CurBrokersList"); }
        }


        private Broker _selectedCurBroker;
        public Broker SelectedCurBroker {
            get { return _selectedCurBroker; }
            set { _selectedCurBroker = value; NotifyPropertyChanged("SelectedCurBroker"); }
        }


        private ObservableCollection<ScenaryLot> _scenaryList = new ObservableCollection<ScenaryLot>();
        public ObservableCollection<ScenaryLot> ScenaryList {
            get { return _scenaryList; }
            set { _scenaryList = value; NotifyPropertyChanged("ScenaryList"); }
        }


        private ScenaryLot _selectedScenary;
        public ScenaryLot SelectedScenary {
            get { return _selectedScenary; }
            set {
                _selectedScenary = value;

                if (value != null) {
                    SelectedBroker = BrokersList.FirstOrDefault(b => b.brokerCode == SelectedScenary.brokerCode);
                    LotCodeTxt = SelectedScenary.lotCode;
                    ClientCodeTxt = SelectedScenary.clientCode;
                    PriceOfferTxt = SelectedScenary.priceOffer;
                }

                NotifyPropertyChanged("SelectedScenary");
            }
        }


        private string _lotCodeTxt;
        public string LotCodeTxt {
            get { return _lotCodeTxt; }
            set { _lotCodeTxt = value; NotifyPropertyChanged("LotCodeTxt"); }
        }


        private string _clientCodeTxt;
        public string ClientCodeTxt {
            get { return _clientCodeTxt; }
            set { _clientCodeTxt = value; NotifyPropertyChanged("ClientCodeTxt"); }
        }


        private decimal _priceOfferTxt;
        public decimal PriceOfferTxt {
            get { return _priceOfferTxt; }
            set { _priceOfferTxt = value; NotifyPropertyChanged("PriceOfferTxt"); }
        }


        private string _monitorTxt;
        public string MonitorTxt {
            get { return _monitorTxt; }
            set { _monitorTxt = value; NotifyPropertyChanged("MonitorTxt"); }
        }


        private int _secPerPriceOfferTxT;
        public int SecPerPriceOfferTxT {
            get { return _secPerPriceOfferTxT; }
            set { _secPerPriceOfferTxT = value; NotifyPropertyChanged("SecPerPriceOfferTxT"); }
        }
        #endregion
    }
}