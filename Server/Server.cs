using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Z.Expressions;

namespace Server
{
    internal class Server
    {
        // Incoming data from the client.
        public static string data = null;
        static void Main(string[] args)
        {
            StartListening();
        }

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }
                    data = data.Replace("<EOF>", "");
                    // Show the data on the console.
                    Console.WriteLine($"Text received : {data}");

                    decimal result = 0;
                    string error = "";
                    try
                    {
                        result = Evaluar(data);
                    }
                    catch(Exception ex)
                    {
                        error = ex.ToString();
                    }

                    string resp = string.IsNullOrEmpty(error) ? result.ToString() : error;       

                    // Echo the data back to the client.
                    byte[] msg = Encoding.UTF8.GetBytes(resp);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        static decimal Evaluar(string expression)
        {
            EvalContext context = new EvalContext
            {
                DefaultNumberType = DefaultNumberType.Double,
                UseCaretForExponent = true
            };
            return context.Execute<decimal>(expression);
        }

    }    
}
