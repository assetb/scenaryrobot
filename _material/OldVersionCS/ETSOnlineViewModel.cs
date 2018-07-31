using System.Collections.Generic;
using altaik.baseapp.vm;
using altaik.baseapp.vm.command;
using System.Windows.Input;
using System.Threading.Tasks;
using EtsApp;
using System.Collections.ObjectModel;
using System.Threading;
using System.Linq;
using AltaBO;

namespace AltaArchive.vm {
    public class ETSOnlineViewModel : BaseViewModel {
        #region Variables
        public EtsApi testETSOnline;
        private Task quotesTask;
        private bool isRun = false;
        #endregion

        #region Methods
        public ETSOnlineViewModel() { }

        public ICommand ConnectETSOnlineCmd { get { return new DelegateCommand(ConnectETSOnline); } }
        private void ConnectETSOnline() {
            // Connect to ETS
            LogsTxt = "Start proccess...";

            testETSOnline = new EtsApi(true);

            if(testETSOnline.GetConnection()) {
                LogsTxt += "\nConnection to ETS is Ok";
                LogsTxt += "\nConnect to quotes table";

                if(testETSOnline.QuotesConnection() != 0) {
                    LogsTxt += "\nConnection to QuotesTable is Ok";

                    isRun = true;

                    quotesTask = new Task(QuotesTask);
                    quotesTask.Start();
                } else LogsTxt += "\nError with connection to Qutoes Table";
            } else LogsTxt += "\nError with connection to ETS";
        }

        private void QuotesTask() {
            while(isRun) {
                Apply();
                Thread.Sleep(2000);
            }
        }

        public ICommand ApplyCmd { get { return new DelegateCommand(Apply); } }
        private void Apply() {            
            if(string.IsNullOrEmpty(FilterTxt)) PriceOffersList = new ObservableCollection<PriceOffer>(testETSOnline.GetPriceOffers());
            else PriceOffersList = new ObservableCollection<PriceOffer>(testETSOnline.GetPriceOffers().Where(p=>p.lotCode.ToLower().Contains(FilterTxt.ToLower())||
                p.firmName.ToLower().Contains(FilterTxt.ToLower()) || p.lotPriceOffer.ToLower().Contains(FilterTxt.ToLower())));
        }
        #endregion

        #region Bindings
        private ObservableCollection<PriceOffer> _priceOffersList = new ObservableCollection<PriceOffer>();
        public ObservableCollection<PriceOffer> PriceOffersList {
            get { return _priceOffersList; }
            set { _priceOffersList = value; RaisePropertyChangedEvent("PriceOffersList"); }
        }

        private string _logsTxt;
        public string LogsTxt {
            get { return _logsTxt; }
            set { _logsTxt = value; RaisePropertyChangedEvent("LogsTxt"); }
        }

        private string _filterTxt;
        public string FilterTxt {
            get { return _filterTxt; }
            set { _filterTxt = value; RaisePropertyChangedEvent("FilterTxt"); }
        }
        #endregion
    }
}