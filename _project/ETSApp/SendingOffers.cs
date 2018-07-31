using BObjects;
using System;
using System.Linq;
using System.Windows;
using ETSApp.Services;

namespace ETSApp {
    public class SendingOffers {
        #region Variables
        static DSSERVERLib.GMsgQuoteS msgQuote;
        private static ScenaryLot scenaryLot;
        private static bool inSearch = false;
        private static string info;
        #endregion

        #region Methods
        public static bool SendPriceOffer(ScenaryLot scenaryLotItem, int sleepSeconds) {
            Loger.Write("OfferSender", "Prepare price offer", true);

            scenaryLot = scenaryLotItem;

            FillMsgQuote();

            DateTime waitTime = DateTime.Now.AddSeconds(sleepSeconds);

            Loger.Write("OfferSender", "Wait for pause until time is not " + waitTime.ToShortTimeString(), true);

            while (waitTime > DateTime.Now) { }

            Loger.Write("OfferSender", "Sending price offer", true);

            bool result = msgQuote.Send(out info);

            Loger.Write("OfferSender", "Get request from server ETS: " + info, true);

            if (info.Contains("has been added")) return true;

            MessageBox.Show("Ошибка подачи: " + info);

            return false;
        }


        private static void FillMsgQuote() {
            Loger.Write("OfferSender", "Fill msg quote", true);

            msgQuote = new DSSERVERLib.GMsgQuoteS();

            msgQuote.Msg_action = 78;
            msgQuote.Id = 0;
            msgQuote.type = 65;
            msgQuote.FirmID = Tables.GetBrokers().FirstOrDefault(b => b.brokerCode == scenaryLot.brokerCode).id;

            inSearch = true;

            while (inSearch) {
                var lotsList = Tables.GetLots();

                if (lotsList != null && lotsList.Count > 0) {
                    var lotId = lotsList.FirstOrDefault(l => l.lotCode.ToLower() == scenaryLot.lotCode.ToLower());

                    if (lotId != null) {
                        scenaryLot.id = lotId.id;
                        inSearch = false;
                    }
                }
            }

            msgQuote.IssueID = scenaryLot.id;
            msgQuote.Issue_name = scenaryLot.lotCode.ToUpper();
            msgQuote.Type_wks = 1;
            msgQuote.Price = scenaryLot.priceOffer;
            msgQuote.Qty = 1;
            msgQuote.Paycond = 84;
            msgQuote.Dcc = "";
            msgQuote.Delivery_days = 10;
            msgQuote.Settl_pair = scenaryLot.clientCode;
            msgQuote.Mm = 0;
            msgQuote.Leave = 1;
            msgQuote.E_s = 0;
        }
        #endregion
    }
}