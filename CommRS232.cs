using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUCControl
{
    class CommRS232 : ICommDevice
    {
        private Commands cmdType;
        private SampleResponses response;

        public CommRS232(SampleResponses resp)
        {
            response = resp;
        }

        public void Write(string cmdString, Commands commandType)
        {
            // Here we should send data over the RS232.
            // For demo command type is simply stored.
            cmdType = commandType;
        }

        public string Read()
        {
            string result = String.Empty;
            // For demo we use file with sample responses for the given command.
            result = response.GetSampleResponse(cmdType);

            return result;
        }

        public bool Connect(string ip/* unused */, int port)
        {
            return true;
        }

        public void Disconnect()
        {
        }
    }
}
