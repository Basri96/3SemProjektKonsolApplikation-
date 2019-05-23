using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UDP3SemConsolReciever
{
    class Program
    {
        // https://msdn.microsoft.com/en-us/library/tst0kwb1(v=vs.110).aspx
        // IMPORTANT Windows firewall must be open on UDP port 7000
        // Use the network EGV5-DMU2 to capture from the local IoT devices
        private const int Port = 7500;
        //private static readonly IPAddress IpAddress = IPAddress.Parse("192.168.5.137"); 
        // Listen for activity on all network interfaces
        // https://msdn.microsoft.com/en-us/library/system.net.ipaddress.ipv6any.aspx

        private const string weightUri = "https://restsmarttrashservice.azurewebsites.net/api/weight/";

        public static async Task<int> AddWeightAsync(weight newWeight)
        {
            using (HttpClient client = new HttpClient())
            {

                var jsonString = JsonConvert.SerializeObject(newWeight);
                Console.WriteLine("JSON: " + jsonString);
                StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(weightUri, content);
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new Exception("Customer already exists. Try another id");
                }
                response.EnsureSuccessStatusCode();
                string str = await response.Content.ReadAsStringAsync();
                int copyOfNewWeight = JsonConvert.DeserializeObject<int>(str);
                return copyOfNewWeight;
            }

        }

        static void Main()
        {
            using (UdpClient socket = new UdpClient(new IPEndPoint(IPAddress.Any, Port)))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(0, 0);

                while (true)
                {
                    Console.WriteLine("Waiting for broadcast {0}", socket.Client.LocalEndPoint);
                    byte[] datagramReceived = socket.Receive(ref remoteEndPoint);

                    string message = Encoding.ASCII.GetString(datagramReceived, 0, datagramReceived.Length);
                    Console.WriteLine("Receives {0} bytes from {1} port {2} message {3}", datagramReceived.Length,
                        remoteEndPoint.Address, remoteEndPoint.Port, message);

                    string[] parts = message.Split(' ');
                    //Console.WriteLine(message);
                    string date = parts[2];
                    string time = parts[3].Substring(0,8);
                    string weight = parts[5];
                    Console.WriteLine(date);
                    string dateTime = date + " " + time;

                    weight weightKilo = new weight();
                    weightKilo.dato = dateTime;
                    weightKilo.weightMeasure = weight;
                    int i = AddWeightAsync(weightKilo).Result;

                    //Parse(message);
                    //}
                }
            }
        }
    }
}