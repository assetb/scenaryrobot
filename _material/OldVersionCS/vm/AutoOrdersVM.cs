using altaik.baseapp.vm;
using ETSRobot_v2.model;
using System.Collections.ObjectModel;

namespace ETSRobot_v2.vm {
    public class AutoOrdersVM:BaseVM
    {
        private ObservableCollection<AutoOrder> autoOrders;
        public ObservableCollection<AutoOrder> AutoOrders
        {
            get { return autoOrders; }
            set { autoOrders = value; RaisePropertyChangedEvent("AutoOrders"); }
        }


        private AutoOrder selectedAutoOrder;
        public AutoOrder SelectedAutoOrder
        {
            get { return selectedAutoOrder; }
            set { selectedAutoOrder = value; RaisePropertyChangedEvent("SelectedAutoOrder"); }
        }
    }
}
