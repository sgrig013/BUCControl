using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUCControl
{
    class CommEthernet : ICommDevice
    {
        private Commands cmdType;
        private SampleResponses response; 

        public CommEthernet(SampleResponses resp)
        {
            response = resp;
        }

        public void Write(string cmdString, Commands commandType)
        {
            string cmd = cmdString;
            // Here we should send data over the Ethernet.
            // For demo command type is simply stored.
            cmdType = commandType;
        }

        public string Read()
        {
            string result = String.Empty;
            // For demo we will use file with sample responses for the given command.
            result = response.GetSampleResponse(cmdType);

            return result;
        }

        public bool Connect(string ip, int port)
        {
            return true;
        }

        public void Disconnect()
        {
        }
    }
}
