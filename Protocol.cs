using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUCControl
{
    public enum Commands { 
        GetTemperature, 
        MuteBUC, 
        UnmuteBUC, 
        GetMuteState, 
        GetAttenuation, 
        SetAttenuation, 
        GetSerialNumber,
        GetPower,
        GetStatusOfAllFaults
    };
    
    class Protocol
    {
        public const string CR = "\r";
        public const string LF = "\n";
 
        public string GetResultValue(string response, Commands cmd)
        {
            string strValue = String.Empty;
            switch (cmd)
            {
                case Commands.GetTemperature:
                    strValue = GetTemperatureValue(response);
                    break;
                case Commands.MuteBUC:
                case Commands.UnmuteBUC:
                    strValue = GetMuteBUCResponse(response);
                    break;
                case Commands.GetMuteState:
                    strValue = GetMuteStateValue(response);
                    break;
                case Commands.GetAttenuation:
                    strValue = GetAttenuationValue(response);
                    break;
                case Commands.SetAttenuation:
                    strValue = GetSetAttenuationResponse(response);
                    break;
                case Commands.GetSerialNumber:
                    strValue = GetSerialNumberValue(response);
                    break;
                case Commands.GetPower:
                    strValue = GetPowerValue(response);
                    break;
                case Commands.GetStatusOfAllFaults:
                    strValue = GetStatusOfAllFaultsValue(response);
                    break;
                default:
                    Console.WriteLine("Warning: unknown command: {0}", cmd);
                    break;
            }

            return strValue;
        }

        /// <summary>
        /// Format: 'gettemp id <ID><CR>'
        /// The ID of the power module for which the temperature will be read. Valid values range from 1 to 5.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTemperatureCommand(byte id = 1)
        {
            return "gettemp id " + id + CR;
        }

        /// <summary>
        /// Temperature sensors that are not hooked up to a Power Module will always read -40°C.
        /// Response: <CR><LF>ok tempC <Temperature In °C> binary <Binary Temperature> adc <Temperature ADC><CR><LF>
        /// Error format: <CR><LF>err "Invalid Command"<CR><LF>
        /// </summary>
        /// <param name="response"></param>
        /// <returns>value in °C</returns>
        public string GetTemperatureValue(string response)
        {
            return GetValue(response, "tempC");
        }

        /// <summary>
        /// Format: setmute uc <Mute Invert> bias <Mute Bias> cmd <Mute Command><CR>
        /// 1 if the device should be muted
        /// </summary>
        /// <returns>command string</returns>
        public string GetMuteBUCCommand(string onOff = "1 ")
        {
            // 1 if the device should be muted when 0V is driven on the Mute Input hardware line, or
            // 0 if the device should be muted when 5V is driven on the Mute Input hardware line
            string muteInvert = "setmute uc " + onOff;
            // 1 if the device should pull a floating Mute Input hardware line up to 5V, or 
            // 0 if the device should pull a floating Mute Input hardware line down to 0V.
            string bias = "bias " + onOff;
            // 1 if the software mute should be enabled, or 
            // 0 if the software mute should be disabled
            string muteCommand = "cmd " + onOff;

            return muteInvert + bias + muteCommand + CR;
        }
        public string GetUnmuteBUCCommand()
        {
            return GetMuteBUCCommand("0 ");
        }

        /// <summary>
        /// Response format: <CR><LF>ok<CR><LF> or <CR><LF>err “Missing Parameter”<CR><LF>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetMuteBUCResponse(string response)
        {
            return GetSetCommandsResponse(response);
        }

        /// <summary>
        /// This command obtains mute status information.
        /// Format: getmute<CR>
        /// </summary>
        /// <returns>command string</returns>
        public string GetMuteStateCommand()
        {
            return "getmute" + CR;
        }
        /// <summary>
        /// Response format: <CR><LF>ok gate <Mute Status> uc <Mute Invert> in <Mute Input> bias <MuteBias> 
        /// ovrd <Mute Override> cmd <Mute Command> fault <Mute Fault><CR><LF>
        /// Error format: <CR><LF>err "Invalid Command"<CR><LF>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetMuteStateValue(string response)
        {
            string strValue = String.Empty;
            if (!String.IsNullOrEmpty(response))
            {
                string[] substrings = response.Split(' ');
                if (substrings.Length > 2 && substrings[0].Contains("ok") && substrings[1] == "gate")
                {
                    // 1 if the device is currently Muted, or 0 if the device is currently Unmuted.
                    // Here just for convenience let's return verbose result
                    strValue = (substrings[2] == "0") ? "0 (Unmuted)" : "1 (Muted)";
                }
                else
                {
                    strValue = response.Trim(); // error message or any response
                }
            }
            else // something's wrong..  
            {
                strValue = "Unexpected error: response string is null or empty";
            }
            return strValue;
        }
        
        /// <summary>
        /// This command obtains Digital Attenuator information from the ATOM device
        /// </summary>
        /// <returns></returns>
        public string GetAttenuationCommand()
        {
            return "getdat" + CR;
        }
        
        /// <summary>
        /// Response format: <CR><LF>ok value <DAT Value><CR><LF>
        /// </summary>
        /// <param name="string">response</param>
        /// <returns></returns>
        public string GetAttenuationValue(string response)
        {
            // NOTE. There is a table that shows the actual attenuation in dB based on the returned value.
            // For this demo program we're going to skip mapping.
            return GetValue(response, "value");
        }
        
        /// <summary>
        /// Format: setdat value <DAT Value><CR>
        /// Valid values range from 0 to 63. 
        /// Note. There are tables in protocol document showing mapping of DAT values to actual Attenuation in dB!
        /// For this demo program we're going to skip it.
        /// </summary>
        /// <returns></returns>
        public string GetSetAttenuationCommand(UInt16 val)
        {
            string strVal = Convert.ToString(val);
            return "setdat value " + strVal + CR;
        }

        /// <summary>
        /// Response format: <CR><LF>ok<CR><LF> or <CR><LF>err “Missing Parameter”<CR><LF>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetSetAttenuationResponse(string response)
        {
            return GetSetCommandsResponse(response);
        }

        /// <summary>
        /// This command obtains identification information from the ATOM device.
        /// Response format: <CR><LF>ok sn <Serial Number><CR><LF>
        /// </summary>
        /// <returns></returns>
        public string GetSerialNumberCommand()
        {
            return "getsn" + CR;
        }

        /// <summary>
        /// Response format: <CR><LF>ok sn <Serial Number><CR><LF>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetSerialNumberValue(string response)
        {
            return GetValue(response, "sn");
        }

        /// <summary>
        /// This command obtains RF Power readings from the ATOM device.
        /// Format: getrfpwr dir <Direction><CR>
        /// 1 to read the RF Forward Power, or 2 to read the RF Reverse Power.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string GetPowerCommand(string dir = "1")
        {
            return "getrfpwr dir " + dir + CR;
        }

        /// <summary>
        /// Response format: <CR><LF>ok dBm <Power Reading> binary <Binary Power Value> adc <ADC Reading><CR><LF>
        /// For example: <CR><LF>ok dBm +18.5 binary 585 adc 19<CR><LF>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetPowerValue(string response)
        {
            return GetValue(response, "dBm");
        }

        public string GetStatusOfAllFaultsCommand()
        {
            return "getfaults" + CR;
        }

        /// <summary>
        /// Response format: <CR><LF>ok mute <Mute Fault> overTemp <Over Temperature Fault> pll <PLLFault><CR><LF>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetStatusOfAllFaultsValue(string response)
        {
            string strValue = String.Empty;
            if (String.IsNullOrEmpty(response))
            {
                return "Unexpected error: response string is null or empty";
            }
            // return as is on success to see all values or err with message
            return response.Trim();
        }

        /// <summary>
        /// This is a generic response for Set commands
        /// Format: <CR><LF>ok<CR><LF> or <CR><LF>err “Missing Parameter”<CR><LF>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string GetSetCommandsResponse(string response)
        {
            string strValue = "Unexpected error: response string is null or empty"; ;
            if (!String.IsNullOrEmpty(response))
            {
                strValue = response.Trim(); // Return as is: ok or err
            }
            return strValue;
        }

        /// <summary>
        /// This is generic parser to get value from response by keyword.
        /// For example, response format: <CR><LF>ok sn <Serial Number><CR><LF>
        /// where 'sn' is a keyword
        /// </summary>
        /// <param name="response"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private string GetValue(string response, string keyword)
        {
            string strValue = String.Empty;
            if (String.IsNullOrEmpty(response))
            {
                return "Unexpected error: response string is null or empty";
            }
            string[] substrings = response.Split(' '); // space is devider
            if (substrings.Length > 2 && substrings[0].Contains("ok") && substrings[1] == keyword)
            {
                strValue = substrings[2];
            }
            else
            {
                strValue = response; // error message or any response
            }
            return strValue.Trim();
        }
    }
}
