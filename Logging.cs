using System;
using System.IO;

namespace DynDNS_API
{
    public class Logging
    {
        public Logging()
        {
            if (!(File.Exists(".//api.log")))
            {
                File.Create(".//api.log");
            }
        }

        public void WriteToLog(string logText) 
        {
            DateTime timeStamp = DateTime.Now;

            File.AppendAllText(".//api.log", timeStamp + " " + logText);
        }
    }
}
