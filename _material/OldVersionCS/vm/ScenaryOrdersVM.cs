using altaik.baseapp.vm;
using ETSRobot_v2.model;
using System.Collections.ObjectModel;

namespace ETSRobot_v2.vm {
    public class ScenaryOrdersVM:BaseVM
    {
        private ObservableCollection<ScenaryOrder> scenaryOrders;
        public ObservableCollection<ScenaryOrder> ScenaryOrders {
            get { return scenaryOrders; }
            set { scenaryOrders = value; RaisePropertyChangedEvent("ScenaryOrders"); }
        }


        private ScenaryOrder selectedScenaryOrder;
        public ScenaryOrder SelectedScenaryOrder {
            get { return selectedScenaryOrder; }
            set { selectedScenaryOrder = value; RaisePropertyChangedEvent("SelectedScenaryOrder"); }
        }
    }
}
