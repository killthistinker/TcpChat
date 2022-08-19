using System;
using System.Threading;

namespace Classwork
{
    class Program
    {
        private static Server _server;
        private static Thread _listenThread;
        static void Main(string[] args)
        {
            try
            {
                _server = new Server();
                _listenThread = new Thread(_server.Listen);
                _listenThread.Start();
            }
            catch (Exception e)
            {
                _server.Disconnect();
                Console.WriteLine(e.Message);
            }
        }
    }
}