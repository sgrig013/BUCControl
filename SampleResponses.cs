using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BUCControl
{
    /// <summary>
    /// This is for demo only to simulate responses from ATOM BUC device.
    /// Sample Command-Response pairs are defined in 'BUCSampleResponses.json' file.
    /// </summary>
    class SampleResponses
    {
        private Dictionary<string, string> sampleResponses;

        public void LoadSampleResponses()
        {
            string fileName = "..\\..\\BUCSampleResponses.json";
            try
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();
                    sampleResponses = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            string dbg = sampleResponses["MuteBUC"];
        }

        public string GetSampleResponse(Commands cmd)
        {
            string command = cmd.ToString();
            return sampleResponses[command];
        }
    }
}
