using altaik.baseapp.vm;
using ETSRobot_v2.model;
using System.Collections.ObjectModel;

namespace ETSRobot_v2.vm {
    public class OrdersVM:BaseVM
    {
        private ObservableCollection<Order> orders;
        public ObservableCollection<Order> Orders
        {
            get
            {
                return orders;
            }
            set
            {
                orders = value;
                RaisePropertyChangedEvent("Orders");
            }
        }


        // Выбранный лот
        private Order selectedOrder;
        public Order SelectedOrder
        {
            get { return selectedOrder; }
            set
            {
                selectedOrder = value;
                RaisePropertyChangedEvent("SelectedOrder");
            }
        }
    }
}
