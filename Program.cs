using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using BUCControl;

namespace BUCControl
{
    class Program
    {
        static void Main(string[] args)
        {
            BUCDriver driver = new BUCDriver(HandleCommandResponse);
            Console.WriteLine("Communication type: {0}", driver.GetCommunicationType());
            driver.GetSerialNumber();

            driver.SetCommunicationTypeEthernet("127.0.0.1", 5353);
            Console.WriteLine("Communication type: {0}", driver.GetCommunicationType());
            
            driver.SetAttenuation(1);
            driver.GetAttenuation();
            driver.GetSerialNumber();
            driver.getPower();
            driver.MuteTheBUC();
            driver.getMuteState();
            driver.UnmuteTheBUC();
            driver.GetTemperature();

            System.Threading.Thread.Sleep(100);
            Console.WriteLine();

            driver.SetCommunicationTypeRS232(889);
            Console.WriteLine("Communication type: {0}", driver.GetCommunicationType());

            driver.GetAllFaults();
            driver.GetSerialNumber();
            driver.getPower();
            driver.MuteTheBUC();
            driver.getMuteState();
            
            Console.ReadLine();
        }

        // This is callback to handle responses
        public static void HandleCommandResponse(string response, Commands cmdType)
        {
            Console.WriteLine("Command {0}: {1}", cmdType, response);

            // If we need to handle each case differently
            // we can use following switch
            bool handleach = false;
            if (handleach)
            {
                switch (cmdType)
                {
                    case Commands.GetTemperature:
                        Console.WriteLine("Temperature is: {0}", response);
                        break;
                    case Commands.GetMuteState:
                        Console.WriteLine("Mute Status is: {0}", response);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
