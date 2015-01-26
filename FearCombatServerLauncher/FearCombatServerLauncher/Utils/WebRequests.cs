using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FearCombatServerLauncher.Utils
{
    public static class WebRequests
    {
        public static IPAddress GetExternalIP()
        {
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (var response = request.GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream()))
                {
                    var value = stream.ReadToEnd();
                    var start = value.IndexOf("Address: ") + 9;
                    var end = value.IndexOf("</body>");
                    var address = value.Substring(start, end - start);
                    return IPAddress.Parse(address);
                }
            }
        }

        public static string GetServerList()
        {
            WebRequest request = WebRequest.Create("http://master.fear-combat.org/api/serverlist-raw.php");
            using (var response = request.GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream()))
                {
                    var value = stream.ReadToEnd();
                    return value;
                }
            }
        }
    }
}