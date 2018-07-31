using ETSRobot_v2.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ETSRobot_v2.service {
    public class RestrictLoadedDataManager : DataManager
    {
        #region Variables
        bool issuesFound = false;
        private List<ProcessedLot> plots = new List<ProcessedLot>();
        private int nLots = 0;
        int iLot = 0;

        private int[] pfirms;
        private int nFirms = 0;
        int iFirm = 0;

        private bool isSetForIssue = false;
        private bool isSetForFirm = false;
        #endregion

        #region Methods
        public RestrictLoadedDataManager(DataManager dataManager)
        {
            tableIssues = dataManager.tableIssues;
            tableFirms = dataManager.tableFirms;
            tableSettlPair = dataManager.tableSettlPair;

            issueData = dataManager.issueData;
            firmData = dataManager.firmData;
            clientData = dataManager.clientData;
            modeData = dataManager.modeData;
        }


        public void SetForIssue(List<ProcessedLot> plots)
        {
            this.plots.Clear();
            string s = plots[0].lotNo;
            int i = 0;

            foreach (var plot in plots.OrderBy(x=>x.lotNo)) {
                if (i != 0) { 
                    if (plot.lotNo != plots[i - 1].lotNo) this.plots.Add(plot);
                } else this.plots.Add(plot);

                s = plot.lotNo;
                i++;
            }

            nLots = this.plots.Count;
            isSetForIssue = true;
        }


        public void SetForFirm(int[] pfirms)
        {
            this.pfirms = pfirms;
            nFirms = pfirms.Length;
            isSetForFirm = true;
        }


        // Добавление инструментов (лотов)
        protected override void TableIssuesAddRow(int IDConnect, int IDRecord, object Fields)
        {
            Issue issue = AddIssueFromRow(Fields);
            if (issue != null) {
                if (GetNames().Contains(issue.name)) iLot++;
                if (iLot == nLots) issuesFound = true;
            }
        }


        private List<string> GetNames()
        {
            List<string> names = new List<string>();
            bool isMatch;

            foreach (var plot in plots) {
                isMatch = false;

                foreach (var subPlot in plots) {
                    if (subPlot.lotNo == plot.lotNo) {
                        if(!isMatch) { names.Add(plot.lotNo); isMatch = true; }
                    }
                }
            }
            return names;
        }


        // Отслеживание загрузки данных из базы
        private void IssuesLoadChecking()
        {
            while (!issuesFound) Thread.Sleep(1);
        }


        public override bool FillIssueData()
        {
            if (isSetForIssue) {
                connectionType = IssuesOpen();
                if (connectionType == 0) return false;
                else {
                    IssuesLoadChecking();
                    return true;
                };
            } else {
                return base.FillIssueData();
            }
        }


        // Фирмы брокеры
        public override Boolean FillFirmBrokerData()
        {
            if (isSetForFirm) {
                connectionType = FirmsOpen();

                if (connectionType == 0)
                    return false;
                else {
                    IssuesLoadChecking();
                    return true;
                }
            } else return base.FillFirmBrokerData();
        }


        public int GetIssueId(String lotNo)
        {
            foreach (var issue in issueData) {
                if (issue.name.Equals(lotNo)) {
                    return issue.id;
                }
            }
            return -1;
        }
        #endregion
    }
}
