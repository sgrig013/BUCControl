using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BUCControl
{
    public enum CommunicationType { None, Ethernet, RS232 };

    class BUCDriver
    {
        public delegate void ClientCallback(string response, Commands cmdType);

        // Use BackgroundWorker component class to peocess communication on a separate thread. 
        // Pass CommandRequest parameters to it and get result asynchronosly.
        private BackgroundWorker BkgdWorkerObj;

        private ConcurrentQueue<CommandRequest> requestsQueue; 
        private CommunicationType communicationType;
        private CommEthernet ethAdapter;
        private CommRS232 serialAdapter;
        private readonly ClientCallback resultHandler;
        private Commands currentCommand;
        private Protocol protocolCmd;
        SampleResponses sampResponse;
        private bool workerThreadBusy;

        public BUCDriver(ClientCallback delegetedCallback)
        {
            resultHandler = delegetedCallback;
            Initialize();
        }
        
        private void Initialize()
        {
            sampResponse = new SampleResponses();
            sampResponse.LoadSampleResponses();

            ethAdapter = new CommEthernet(sampResponse);
            serialAdapter = new CommRS232(sampResponse);
            protocolCmd = new Protocol();
            requestsQueue = new ConcurrentQueue<CommandRequest>();
            BkgdWorkerObj = new BackgroundWorker();

            // Attach event handlers to the BackgroundWorker object.
            BkgdWorkerObj.DoWork += new DoWorkEventHandler(BkgdWorkerObj_DoWork);
            BkgdWorkerObj.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BkgdWorkerObj_RunWorkerCompleted);
            workerThreadBusy = false;
        }

        public void SetCommunicationTypeEthernet(string ip, int port)
        {
            if (communicationType == CommunicationType.Ethernet)
            {
                return;
            }
            // Note: Normally we would check for connect status and return OK/error here
            serialAdapter.Disconnect();
            ethAdapter.Connect(ip, port);
            communicationType = CommunicationType.Ethernet;
        }

        public void SetCommunicationTypeRS232(int port)
        {
            if (communicationType == CommunicationType.RS232)
            {
                return;
            }
            // Note: Normally we would check for connect status and return OK/error here
            ethAdapter.Disconnect();
            serialAdapter.Connect(null, port);
            communicationType = CommunicationType.RS232;
        }

        public string GetCommunicationType()
        {
            return communicationType.ToString();
        }

        public void MuteTheBUC()
        {
            string cmd = protocolCmd.GetMuteBUCCommand();
            ProcessCommandRequest(cmd, Commands.MuteBUC);
        }

        public void UnmuteTheBUC()
        {
            string cmd = protocolCmd.GetUnmuteBUCCommand();
            ProcessCommandRequest(cmd, Commands.UnmuteBUC);
        }
        /// <summary>
        /// muted or unmuted
        /// </summary>
        public void getMuteState()
        {
            string cmd = protocolCmd.GetMuteStateCommand();
            ProcessCommandRequest(cmd, Commands.GetMuteState);
        }

        /// <summary>
        /// Get the Synthesizer Attenuation in dB
        /// </summary>
        public void GetAttenuation()
        {
            string cmd = protocolCmd.GetAttenuationCommand();
            ProcessCommandRequest(cmd, Commands.GetAttenuation);
        }

        /// <summary>
        /// Set the Synthesizer Attenuation in dB.
        /// Valid values range from 0 to 63.
        /// </summary>
        public void SetAttenuation(UInt16 val)
        {
            string cmd = protocolCmd.GetSetAttenuationCommand(val);
            ProcessCommandRequest(cmd, Commands.SetAttenuation);
        }

        /// <summary>
        /// Obtains identification information from the ATOM device.
        /// </summary>
        public void GetSerialNumber()
        {
            string cmd = protocolCmd.GetSerialNumberCommand();
            ProcessCommandRequest(cmd, Commands.GetSerialNumber);
        }

        /// <summary>
        /// Get the RF Forward Output Power in dBm.
        /// </summary>
        public void getPower()
        {
            string cmd = protocolCmd.GetPowerCommand();
            ProcessCommandRequest(cmd, Commands.GetPower);
        }

        /// <summary>
        /// Get the status of all faults
        /// </summary>
        /// <returns></returns>
        public void GetAllFaults()
        {
            string cmd = protocolCmd.GetStatusOfAllFaultsCommand();
            ProcessCommandRequest(cmd, Commands.GetStatusOfAllFaults);
        }

        /// <summary>
        /// Get the internal temperature in ° C
        /// </summary>
        public void GetTemperature()
        {
            string cmd = protocolCmd.GetTemperatureCommand();
            ProcessCommandRequest(cmd, Commands.GetTemperature);
        }

        /// <summary>
        /// Check queue and if not empty, process command
        /// </summary>
        private void ProcessQueue()
        {
            CommandRequest request;
            if (!requestsQueue.IsEmpty && requestsQueue.TryDequeue(out request))
            {
                StartWorkerThread(request);
            }
            else
            {
                workerThreadBusy = false;
            }
        }

        /// <summary>
        /// Initialize command request object and add to queue
        /// </summary>
        private void ProcessCommandRequest(string cmdString, Commands cmdType)
        {
            CommandRequest request = new CommandRequest();
            request.commandType = cmdType;
            request.commandSring = cmdString;

            if (communicationType == CommunicationType.Ethernet)
            {
                request.commDevice = ethAdapter;
            }
            else if (communicationType == CommunicationType.RS232)
            {
                request.commDevice = serialAdapter;
            }
            else
            {
                resultHandler("Error: No connection", currentCommand);
                return;
            }

            // Note: we may add lock around these check to ensure thread safety.
            if (workerThreadBusy)
            {
                requestsQueue.Enqueue(request);
            }
            else
            {
                if (StartWorkerThread(request))
                {
                    workerThreadBusy = true;
                }
                else
                {
                    resultHandler("Error: Internal", currentCommand);   
                }
            }
        }

        /// <summary>
        /// Send command over the current adapter from thread.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Boolean StartWorkerThread(CommandRequest request)
        {
            try
            {
                BkgdWorkerObj.RunWorkerAsync(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        // Event handler to start command processing.
        private void BkgdWorkerObj_DoWork(object sender, DoWorkEventArgs e)
        {
            CommandRequest requestObj = (CommandRequest)e.Argument;
            currentCommand = requestObj.commandType;
            e.Result = requestObj.ProcessRequest();
        }

        // Event handler to process result. 
        private void BkgdWorkerObj_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = (string)e.Result;
            string resultValue = protocolCmd.GetResultValue(result, currentCommand);
            if (e.Error != null)
            {
                resultValue = "Error: " + e.Error.Message;
            }

            // Invoke provided client callback.  
            resultHandler(resultValue, currentCommand);
            ProcessQueue();
        }
    }
}
