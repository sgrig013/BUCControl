using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUCControl
{
    public class CommandRequest
    {
        public Commands commandType;
        public string commandSring;
        public ICommDevice commDevice;

        public string ProcessRequest()
        {
            string responseValue = "Error: Invalid parameter";

            if (String.IsNullOrEmpty(commandSring))
            {
                return "Error: command string is null or empty!";
            }

            // Depending on communicationType setting we use appropriate device(Ethernet or RS232).
            try
            {
                commDevice.Write(commandSring, commandType);
                responseValue = commDevice.Read();
            }
            catch (Exception e)
            {
                responseValue = e.Message;
            }
            return responseValue;
        }
    }
}
