using BObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETSApp {
    public class Tables {
        #region Variables
        private static List<Lot> lots = new List<Lot>();
        private static List<Quote> quotes = new List<Quote>();
        private static List<Broker> brokers = new List<Broker>();
        #endregion

        #region Methods
        public static void LotsAddRows(int IDConnect, int IDRecord, object Fields) {
            IList collection = (IList)Fields;

            decimal result;
            decimal.TryParse(collection[2].ToString(), NumberStyles.Any, new CultureInfo("en-US"), out result);

            lots.Add(new Lot() {
                id = Convert.ToInt32(collection[0]),
                lotCode = collection[1].ToString(),
                startPrice = result
            });
        }

        public static List<Lot> GetLots() {
            var lotsInfo = lots;

            if (lotsInfo != null) return lotsInfo;
            else return new List<Lot>();
        }

        public static void QuotesAddRows(int IDConnect, int IDRecord, object Fields) {
            IList collection = (IList)Fields;

            decimal result;
            decimal.TryParse(collection[1].ToString(), NumberStyles.Any, new CultureInfo("en-US"), out result);

            quotes.Add(new Quote() {
                lotCode = collection[0].ToString(),
                priceOffer = result,
                brokerCode = collection[2].ToString(),
                moment = collection[3].ToString()
            });
        }

        public static List<Quote> GetQuotes() {
            var quotesInfo = quotes;

            if (quotesInfo != null) return quotesInfo;
            else return new List<Quote>();
        }

        public static List<Broker> GetBrokers() {
            if (brokers != null && brokers.Count > 0) return brokers;
            else return BrokersAddRows();
        }

        private static List<Broker> BrokersAddRows() {
            brokers.Clear();

            brokers.Add(new Broker() { id = 443, brokerCode = "KORD" });
            brokers.Add(new Broker() { id = 470, brokerCode = "AKAL" });
            brokers.Add(new Broker() { id = 430, brokerCode = "ALTA" });
            brokers.Add(new Broker() { id = 455, brokerCode = "ALTK" });
            brokers.Add(new Broker() { id = 447, brokerCode = "TRN8" });
            brokers.Add(new Broker() { id = 448, brokerCode = "TRN9" });

            return brokers;
        }
        #endregion
    }
}
