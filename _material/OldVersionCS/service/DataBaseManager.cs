using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;
using ETSRobot_v2.model;
using System.Data;

namespace ETSRobot_v2.service
{
    public class DataBaseManager
    {
        #region Variables
        //private const String connectionStr = "server=localhost;port=3306;database=brokerbase;uid=tester;password=test;"; // local
        //private const String connectionStr = "server=10.1.2.11;port=3306;database=brokerbase;uid=broker;password=KorPas$77&db;"; // with VPN
        private const String connectionStr = "server=88.204.230.203;port=3306;database=brokerbase;uid=broker;password=KorPas$77&db;"; // with VPN
        
        private MySqlConnection mySqlConn;
        private MySqlCommand mySqlComm;
        private DataSet dataSet = new DataSet();
        private MySqlDataAdapter sqlAdapter = new MySqlDataAdapter();
        private static List<ScenaryOrder> scnearyOrderLst = new List<ScenaryOrder>();

        private int connectionStatus;
        #endregion

        #region Methods
        public DataBaseManager() { }


        public void CheckSerial()
        {
            if (CheckConnection(0) != 0)
                if (Execute("select * from robots where serial='" + App.hddSerialNumber + "'").Tables[0].Rows.Count == 0) {
                    AppJournal.Write("DataBaseManager", "Record about SN not exist in DB");
                    MessageBox.Show("Подключение не легально.\nДанного ключа программы нет в базе.", "Предупреждение правообладателя", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Environment.Exit(0);
                }
        }


        private int CheckConnection(int trying)
        {
            mySqlConn = new MySqlConnection(connectionStr);

            try {
                mySqlConn.Open();
            } catch (Exception ex) {
                AppJournal.Write("DataBaseManager", "Connect to DB err: "+ex.ToString());
                MessageBox.Show("Нет доступа к базе", "Предупреждение правообладателя", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (trying == 0) Environment.Exit(0);
                else return 0;
            }
            connectionStatus = 1;

            return connectionStatus;
        }


        private void ModifyRecord(String queryStr)
        {
            if (mySqlConn.State != System.Data.ConnectionState.Open) connectionStatus = CheckConnection(1);

            if (connectionStatus != 0) {
                mySqlComm = new MySqlCommand(queryStr, mySqlConn);

                mySqlComm.CommandText = queryStr;

                try {
                    mySqlComm.ExecuteNonQuery();
                    CloseConnection();
                } catch (Exception) { }
            }
        }


        public void MakePriceOffer(ScenaryOrder scenaryOrder)
        {
            int lotid = Execute("select id, number from lots where number like '%" + scenaryOrder.LotNumber + "%'").Tables[0].Rows[0].Field<int>("id");

            if (Execute("select lotid from priceoffers where lotid='" + lotid + "'").Tables[0].Rows.Count != 0)
                ModifyRecord("update priceoffers set price='" + scenaryOrder.PriceTo + "' where lotid='" + lotid + "'");
            else {
                int procuratoryid = Execute("select * from procuratories where lotid='" + lotid +
                    "' and supplierid=(select supplierid from suppliersjournal where code like '%" + scenaryOrder.ClientCode + "%')").Tables[0].Rows[0].Field<int>("id");
                ModifyRecord("insert into priceoffers (procuratoryid, lotid, price)values('" + procuratoryid + "','" + lotid + "','" + scenaryOrder.PriceTo + "')");
            }
        }


        public void MakeTestOffer(ScenaryOrder scenaryOrder)
        {
            ModifyRecord("insert into etsresults (brokername, clientcode, lotnumber, sum, date, userserial)values('" +
                scenaryOrder.SelectedBroker.name + "','" + scenaryOrder.ClientCode + "','" + scenaryOrder.LotNumber + "','" + scenaryOrder.PriceTo + "','" +
                DateTime.Now.ToString("yyyy-MM-dd") + "','" + App.hddSerialNumber + "')");
        }


        private DataSet Execute(String queryStr)
        {
            if (mySqlConn.State != ConnectionState.Open) connectionStatus = CheckConnection(1);

            if (connectionStatus != 0) {
                sqlAdapter.SelectCommand = new MySqlCommand(queryStr, mySqlConn);

                try {
                    sqlAdapter.Fill(dataSet);
                    return dataSet;
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    AppJournal.Write("DataBaseManager", "Get data from db query err: "+ex.ToString());
                }
            }
            return null;
        }


        public void AppendList(ScenaryOrder scnenaryOrder)
        {
            scnearyOrderLst.Add(scnenaryOrder);
        }


        public void OnAppExit(object sender, EventArgs e)
        {
            /*foreach(var item in scnearyOrderLst) {
                MakePriceOffer(item);
            }*/
        }


        private void CloseConnection()
        {
            mySqlConn.Close();
        }
        #endregion
    }
}