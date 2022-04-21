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
        static void Main(string[] args)
        {
            StartListening();
        }

        public static void StartListening()
        {

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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
                    Task.Run(() => Procesar(handler));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        static void Procesar(Socket handler)
        {
            byte[] bytes = new byte[99999];
            int bytesRec = handler.Receive(bytes);
            string data = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            // Show the data on the console.
            Console.WriteLine($"Text received : {data}");

            decimal result = 0;
            string error = "";
            try
            {
                result = Evaluar(data);
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }

            string resp = string.IsNullOrEmpty(error) ? result.ToString() : error;

            // Echo the data back to the client.
            handler.Send(Encoding.UTF8.GetBytes(resp));
            // Release the socket.
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
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
