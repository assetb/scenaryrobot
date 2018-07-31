using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETSRobot_v2.model.connection
{
    public interface IDbManager
    {
        bool Connected(string curBroker);
        void GetConnectedServer();
        void Close();
    }
}
