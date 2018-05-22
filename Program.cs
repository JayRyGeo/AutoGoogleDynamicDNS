using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.IO;

namespace DynDNS_API
{
    class Program
    {
        private const string IPURL = "http://checkip.dyndns.org";



        static void Main(string[] args)
        {
            //Create logging object
            Logging logging = new Logging();
            
            //Check for, and create, config file.
            if (!(File.Exists(".//Config.xml")))
            {
                File.AppendAllText(@".\Config.xml", Properties.Resources.Config);
                logging.WriteToLog("No XML file detected.  One has been created for you in the same directory as the program.  Please configure the XML file before running this program again");
                Console.ReadLine();
                return;
            }

            //DNS list that will be populated via XML document.
            List<DynDNS> DNSLIST = new List<DynDNS>();

            //Document object
            XmlDocument doc = new XmlDocument();
            doc.Load(".//Config.xml");

            //Get node list at the top level. <HostList>
            XmlNodeList hostNodes = doc.ChildNodes;

            //Iterate through objects.
            foreach (XmlNode hostNode in hostNodes)
            {
                //Get root
                if (hostNode.Name.Equals("HostList"))
                {
                    //Iterate though each <Host>
                    foreach (XmlNode node in hostNode.ChildNodes)
                    {
                        //ignore comment nodes.
                        if (node.NodeType != XmlNodeType.Comment)
                        {
                            //Temp DynDNS entry
                            DynDNS newEntry = new DynDNS();

                            //Go through each element
                            foreach (XmlNode attrib in node.ChildNodes)
                            {
                                //Assign values
                                switch (attrib.Name)
                                {
                                    case "IPAddress":
                                        newEntry.IPAddress = attrib.InnerText;
                                        break;

                                    case "Hostname":
                                        newEntry.Hostname = attrib.InnerText;
                                        break;

                                    case "UserName":
                                        newEntry.UserName = attrib.InnerText;
                                        break;

                                    case "Password":
                                        newEntry.Password = attrib.InnerText;
                                        break;

                                    default:
                                        logging.WriteToLog("XML Document Format Is Invalid.");
                                        return;
                                }
                            }
                            //Add to dns list
                            DNSLIST.Add(new DynDNS() { IPAddress = newEntry.IPAddress, Hostname = newEntry.Hostname, Password = newEntry.Password, UserName = newEntry.UserName });
                        }
                        
                    }  
                }
            }
            
            //Get IP Addresses if they are blank
            foreach (DynDNS entry in DNSLIST)
            {
                if (entry.IPAddress.Equals(""))
                {
                    //Get the current public facing URL
                    WebClient webClient = new WebClient();
                    string ipData = webClient.DownloadString(IPURL);

                    //Grab just the URL from the webscrape and apply it to the class
                    entry.IPAddress = ipData.Substring(ipData.IndexOf(':') + 2, (ipData.IndexOf("</body>") - 2) - ipData.IndexOf(':'));
                }
            }

            foreach (DynDNS current in DNSLIST)
            {
                //Build the initial URL with the username, password, and hostname (domain to be updated)
                string DNSURL = string.Format("https://{0}:{1}@domains.google.com/nic/update?hostname={2}", current.UserName, current.Password, current.Hostname);

                //Create new client object
                HttpClient client = new HttpClient();
                //Create the new base url URI object
                client.BaseAddress = new Uri(DNSURL);

                //Make new request, in this case it will be a POST to update the IP address
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);

                //Make a byte array of the username and password for authorization at google's servers
                var byteArray = Encoding.ASCII.GetBytes(current.UserName + ":" + current.Password);
                //Create the authorization base64 encoded string to pass as a header
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                //Create the user agent to pass
                request.Headers.UserAgent.ParseAdd("Chrome/41.0");

                //Send the request
                var task = client.SendAsync(request);
                //Wait for the task to complete before moving on.  This ensures that we get a response
                //before checking the response
                task.Wait();

                string content = task.Result.Content.ReadAsStringAsync().Result;
                string response = content.Split(' ')[0];

                //Display status for each entry
                Console.Write(current.Hostname + ": ");
                logging.WriteToLog("Request Sent: " + request);

                switch (response)
                {
                    case "good":
                        logging.WriteToLog("SUCCESS: Updated the IP address to " + current.IPAddress + ".");
                        break;

                    case "nochg":
                        logging.WriteToLog("SUCCESS: No change was needed. Current IP is " + current.IPAddress);
                        break;

                    case "nohost":
                        logging.WriteToLog("ERROR: Hostname does not exist or Dynamic DNS is not enable in DNS setting.");
                        break;

                    case "badauth":
                        logging.WriteToLog("ERROR: Username:Password combo is incorrect.  Check setting in Google DNS console.");
                        break;

                    case "notfqdn":
                        logging.WriteToLog("ERROR: Hostname is not fully-qualified.  Check DNSURL and Hostname for typos.");
                        break;

                    case "badagent":
                        logging.WriteToLog("ERROR: Bad request. Only IPv4 addresses can be set, check User Agent Header");
                        break;

                    case "abuse":
                        logging.WriteToLog("ERROR: Dynamic DNS has been disable for current hostname due to previous failed requests.");
                        break;

                    case "911":
                        // google error, wait 10 minutes for retry
                        logging.WriteToLog("ERROR: Google error 911. Wait 5 minutes before trying again");
                        break;

                    default:
                        // some other issue
                        logging.WriteToLog("Other failure not due to POST response.");
                        break;
                }
            }

            //Console.ReadLine();
        }
    }
}
