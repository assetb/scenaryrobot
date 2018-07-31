using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using ETSRobot_v2.model;
using System.Globalization;

namespace ETSRobot_v2.service {
    public class QuotesChecking {
        #region Variables
        DSSERVERLib.Online tableQuotes;
        List<ProcessedLot> Plots;

        public Boolean[] grant;
        public Boolean[] sended;
        public Boolean checkAll = false;
        public decimal[] curPrice;
        #endregion

        #region Methods
        public QuotesChecking(List<ProcessedLot> plots) {
            this.Plots = plots;

            grant = new Boolean[plots.Count];
            for(int g = 0; g < plots.Count; g++) grant[g] = false;
            sended = new Boolean[plots.Count];
            for(int s = 0; s < plots.Count; s++) sended[s] = false;
            curPrice = new decimal[plots.Count];

            // Table quotes connection
            tableQuotes = new DSSERVERLib.Online();
            tableQuotes.AddRow += TableQuotesAddRow;

            try {
                tableQuotes.Open(DSSERVERLib.ConnectionType.RTSONL_DYNAMIC, "Quote", "issue_name, price, firm_name", "id", null, null, DSSERVERLib.Sort.RTSONL_SORT_EMPTY);
            } catch { MessageBox.Show("Ошибка доступа к таблице котировок!"); }
        }


        String curFirm;
        String curLot;

        // Check current price and who send it
        void TableQuotesAddRow(int IDConnect, int IDRecord, object Fields) {
            IList collection = (IList)Fields;
            int iCount;

            iCount = 0;

            foreach(var plot in Plots) {
                if(collection[0].ToString() == plot.lotNo) {
                    curLot = plot.lotNo;
                    decimal rez;
                    decimal.TryParse(collection[1].ToString(), NumberStyles.Any, new CultureInfo("en-US"), out rez);

                    if(curPrice[iCount] != rez) {
                        curFirm = collection[2].ToString();

                        if(curFirm == "KORD" || curFirm == "ALTK" || curFirm == "ALTA" || curFirm == "AKAL" || curFirm == "TRN10" || curFirm == "TRN11" || curFirm == "TRN9" || curFirm == "TRN8") {
                            if(grant[iCount] != true) {
                                grant[iCount] = true;
                                CheckAll();
                            }

                        }
                    }

                    curPrice[iCount] = rez;
                }

                iCount++;
            }
        }

        // Check access
        private void CheckAccess(String lot) {
        }

        // Check all grants
        private void CheckAll() {
            foreach(var g in grant)
                if(g != true) return;

            checkAll = true;
            MessageBox.Show("All orders are sended");
        }
        #endregion
    }
}
