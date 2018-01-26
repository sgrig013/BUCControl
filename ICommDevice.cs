using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUCControl
{
    /// <summary>
    /// Here we define generic methods that should be implenented in each communication device class.
    /// Only few functions are defined for demo.
    /// Actual implementation of device drivers is out of scope. 
    /// </summary>
    public interface ICommDevice
    {
        // Note: Command type parameter is used here only to support demo
        void Write(string cmdString, Commands commandType);
        string Read();
        bool Connect(string ip, int port);
        void Disconnect();
    }
}
