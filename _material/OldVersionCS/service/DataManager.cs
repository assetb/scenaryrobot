using ETSRobot_v2.model;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace ETSRobot_v2.service {
    public class DataManager {
        #region Variables
        public DSSERVERLib.Online tableIssues;
        public DSSERVERLib.Online tableFirms;
        public DSSERVERLib.Online tableSettlPair;

        public ObservableCollection<Issue> issueData;
        public ObservableCollection<Firm> firmData;
        public ObservableCollection<SettlPair> clientData;
        public ObservableCollection<Mode> modeData;

        protected int connectionType;
        private DateTime lastAddRowTime = DateTime.Now.AddDays(1);
        #endregion

        #region Variables
        // Конструктор
        public DataManager() { }


        // Заполнение полей полученными с сервера данными
        // Инструменты (лоты)
        public virtual Boolean FillIssueData() {
            lastAddRowTime = DateTime.Now.AddDays(1);

            connectionType = IssuesOpen();
            if(connectionType == 0) return false;
            else {
                LoadTimeManagement();
                return true;
            };
        }


        public int IssuesOpen() {
            AppJournal.Write("DataManager", "Trying open issue table");

            tableIssues = new DSSERVERLib.Online();
            issueData = new ObservableCollection<Issue>();
            tableIssues.AddRow += TableIssuesAddRow;

            //tableIssues.Connected += TableIssues_Connected;
            //tableIssues.Disconnected += TableIssues_Disconnected;
            //tableIssues.Error += TableIssues_Error;
            //tableIssues.PropertyChanged += TableIssues_PropertyChanged;

            try {
                connectionType = tableIssues.Open(DSSERVERLib.ConnectionType.RTSONL_DYNAMIC, "Issue", "id, name, nominal", "id", null, null, DSSERVERLib.Sort.RTSONL_BACKWARD);
            } catch(Exception ex) {
                AppJournal.Write("DataManager", "Open table err: " + ex.ToString());
                MessageBox.Show("Ошибка доступа к таблице инструментов!");
            }

            return connectionType;
        }

        private void TableIssues_PropertyChanged(string PropertyName) {
            MessageBox.Show("PropertyChaned: " + PropertyName);
        }

        private void TableIssues_Error(int IDConnect, string Description) {
            MessageBox.Show("Error: " + IDConnect + "   " + Description);
        }

        private void TableIssues_Disconnected(int IDConnect) {
            MessageBox.Show("Disconnecte: " + IDConnect);
        }

        private void TableIssues_Connected(int IDConnect) {
            MessageBox.Show("Connected: " + IDConnect);
        }


        // Фирмы брокеры
        public virtual Boolean FillFirmBrokerData() {
            lastAddRowTime = DateTime.Now.AddDays(1);
            connectionType = FirmsOpen();

            if(connectionType == 0)
                return false;
            else {
                LoadTimeManagement();
                return true;
            }
        }


        protected int FirmsOpen() {
            AppJournal.Write("DataManager", "Trying open firm table");

            tableFirms = new DSSERVERLib.Online();
            firmData = new ObservableCollection<Firm>();
            tableFirms.AddRow += TableFirmsAddRows;

            try {
                connectionType = tableFirms.Open(DSSERVERLib.ConnectionType.RTSONL_DYNAMIC, "Firm", "id, name", "name", null, null, DSSERVERLib.Sort.RTSONL_FORWARD);
            } catch(Exception ex) {
                AppJournal.Write("DataManager", "Opening table err: " + ex.ToString());
                MessageBox.Show("Ошибка доступа к таблице клиентов!");
            }

            return connectionType;
        }


        // Клиенты
        public Boolean FillClientData() {
            AppJournal.Write("DataManager", "Trying open client table");

            tableSettlPair = new DSSERVERLib.Online();
            clientData = new ObservableCollection<SettlPair>();
            tableSettlPair.AddRow += TableClientsAddRow;

            lastAddRowTime = DateTime.Now.AddDays(1);

            try {
                connectionType = tableSettlPair.Open(DSSERVERLib.ConnectionType.RTSONL_DYNAMIC, "SettlPair", "id, firm_name, settl_pair", "id", null, null, DSSERVERLib.Sort.RTSONL_SORT_EMPTY);
            } catch(Exception ex) {
                AppJournal.Write("DataManager", "Opening table err: " + ex.ToString());
                MessageBox.Show("Ошибка доступа к таблице Workstation!");
            }

            if(connectionType == 0)
                return false;
            else {
                LoadTimeManagement();
                return true;
            }
        }


        // Добавление фирм брокеров
        void TableFirmsAddRows(int IDConnect, int IDRecord, object Fields) {
            IList collection = (IList)Fields;
            if(collection[1].ToString() != string.Empty) {
                if(collection[1].ToString().Contains("TRN") || collection[1].ToString().Contains("ALTA")
                    || collection[1].ToString().Contains("KORD") || collection[1].ToString().Contains("ALTK")
                    || collection[1].ToString().Contains("AKAL")) {
                    firmData.Add(new Firm {
                        id = Convert.ToInt32(collection[0]),
                        name = collection[1].ToString()
                    });
                }
            }

            lastAddRowTime = DateTime.Now;
        }



        // Добавление инструментов (лотов)
        protected virtual void TableIssuesAddRow(int IDConnect, int IDRecord, object Fields) {
            AddIssueFromRow(Fields);
            lastAddRowTime = DateTime.Now;
        }


        protected Issue AddIssueFromRow(object Fields) {
            IList collection = (IList)Fields;
            if(collection[1].ToString() != string.Empty) {
                if(collection[1].ToString().Contains("0T") || collection[1].ToString().Contains("0G")) {
                    decimal rez;
                    decimal.TryParse(collection[2].ToString(), NumberStyles.Any, new CultureInfo("en-US"), out rez);

                    Issue issue = new Issue {
                        id = Convert.ToInt32(collection[0]),
                        name = collection[1].ToString(),
                        nominal = rez
                    };
                    issueData.Add(issue);
                    return issue;
                }
            }
            return null;
        }


        // Добавление клиентов
        void TableClientsAddRow(int IDConnect, int IDRecord, object Fields) {
            IList collection = (IList)Fields;
            clientData.Add(new SettlPair {
                id = Convert.ToInt32(collection[0]),
                firmName = collection[1].ToString(),
                settlPair = collection[2].ToString()
            });

            lastAddRowTime = DateTime.Now;
        }


        // Добавление режимов
        public void MakeModes() {
            AppJournal.Write("DataManager", "Make modes");

            modeData = new ObservableCollection<Mode>();

            modeData.Add(new Mode { type = "Конкурирующий" });
            modeData.Add(new Mode { type = "Поточный" });
            modeData.Add(new Mode { type = "Разовый" });
        }


        // Отслеживание загрузки данных из базы
        private void LoadTimeManagement() {
            while((DateTime.Now - lastAddRowTime).Seconds < 3) {
                Thread.Sleep(300);
            }
        }
        #endregion
    }
}
