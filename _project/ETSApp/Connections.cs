using System;
using System.Windows;

namespace ETSApp {
    public class Connections {
        #region Variables
        public static DSSERVERLib.Connection etsConnection;
        public static DSSERVERLib.Online lotsTable, quotesTable;
        private static int lotsTableCon = 0, quotesTableCon = 0;
        #endregion

        #region Methods
        public static bool GetConnection(bool isTest = false) {
            if(etsConnection == null) OpenConnection(isTest);

            return etsConnection == null ? false : true;
        }


        private static void OpenConnection(bool isTest = false) {
            etsConnection = new DSSERVERLib.Connection();

            try {
                etsConnection.Open(@"Online_" + (isTest ? "test" : "war") + ".ini", "", "", "");
            } catch(Exception ex) {
                MessageBox.Show("Ошибка подключения к ЕТС: " + ex.ToString());

                etsConnection = null;
            }
        }


        public static void CloseConnection() {
            if(etsConnection != null) etsConnection.Close();
        }


        public static int GetLotsConnection() {
            if(lotsTableCon == 0) {
                lotsTable = new DSSERVERLib.Online();

                lotsTable.AddRow += Tables.LotsAddRows;
                lotsTable.Error += LotsTable_Error;

                try {
                    lotsTableCon = lotsTable.Open(DSSERVERLib.ConnectionType.RTSONL_DYNAMIC, "Issue", "id, name, nominal", "id", null, null, DSSERVERLib.Sort.RTSONL_BACKWARD);
                } catch(Exception ex) {
                    MessageBox.Show("Ошибка подключения к таблице лотов: " + ex.ToString());

                    lotsTableCon = 0;
                }
            }

            return lotsTableCon;
        }


        private static void LotsTable_Error(int IDConnect, string Description) {
            MessageBox.Show("Quotes table err: " + Description);
        }


        public static int GetQuotesConnection() {
            if(quotesTableCon == 0) {
                quotesTable = new DSSERVERLib.Online();

                quotesTable.AddRow += Tables.QuotesAddRows;

                try {
                    quotesTableCon = quotesTable.Open(DSSERVERLib.ConnectionType.RTSONL_DYNAMIC, "Quote", "issue_name, price, firm_name, moment", "id", null, null, DSSERVERLib.Sort.RTSONL_SORT_EMPTY);
                } catch(Exception ex) {
                    MessageBox.Show("Ошибка подключения к таблице котировок: " + ex.ToString());

                    quotesTableCon = 0;
                }
            }

            return quotesTableCon;
        }
        #endregion
    }
}