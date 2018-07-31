namespace BObjects {
    public class ScenaryLot : Lot {
        public int number { get; set; }
        public string brokerCode { get; set; }
        public string clientCode { get; set; }
        public decimal priceOffer { get; set; }
        public bool status { get; set; }
    }
}
