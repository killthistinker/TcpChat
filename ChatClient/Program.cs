using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatClient
{
    class Program
    {
        private const string Personaly = "лично";
        private static string _userName;
        private const string Host = "127.0.0.1";
        private const int Port = 8888;
        private static TcpClient _client;
        private static NetworkStream _stream;
        private static bool _authorization = true;
        
        static void Main(string[] args)
        {
            ConnectionToChat();
            _client = new TcpClient();
            try
            {
                _client.Connect(Host, Port);
                _stream = _client.GetStream();
                Authorization();
                Console.WriteLine($"Добро пожаловать в чат {_userName}");
                SendMessage();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                DIsconnect();
            }
        }
        
        private static void SendMessage()
        {
            while (true)
            {
                byte[] data = new byte[64];
                Console.WriteLine("Введите \"лично\" для отправки сообщения конкретному пользователю");
                Console.WriteLine("Введите сообщение: ");
                string message = Console.ReadLine();
                if (message == Personaly)
                {
                    data = Encoding.Unicode.GetBytes("personaly");
                    _stream.Write(data, 0, data.Length);
                    SendPersonally(ref message);
                }
                else
                {
                    data = Encoding.Unicode.GetBytes(message);
                    _stream.Write(data, 0, data.Length);
                }
            }
        }

        private static void Authorization()
        {
            do
            {
                Console.WriteLine("Введите своё имя: ");
                _userName = Console.ReadLine();
                string message = _userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                _stream.Write(data, 0, data.Length);
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();
                Thread.Sleep(100);
            } while (_authorization);
        }

        private static void SendPersonally(ref string message)
        {
            Console.WriteLine("Введите сообщение в формате: ");
            Console.WriteLine("\"имя пользователя:\" \"сообщение \"");
            Console.WriteLine("для отправки сообщения нескольким пользователям перечесляйте их через запятую");
            byte[] data = new byte[64];
            message = Console.ReadLine();
            data = Encoding.Unicode.GetBytes(message);
            _stream.Write(data, 0 , data.Length);
            Thread.Sleep(5);
        }

        private static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = _stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data,0, bytes));
                    } while (_stream.DataAvailable);

                    string message = builder.ToString();
                    if (message.Contains("false"))
                    {
                        Console.WriteLine($"Пользователь с именем {_userName} существует, введите другое имя");
                        return;
                    }
                    _authorization = false;
                    Console.WriteLine(message);
                }
                catch
                {
                    Console.WriteLine("Подключение прервано");
                    DIsconnect();
                } 
            }
        }
        
        private static void DIsconnect()
        {
            if(_stream != null)
                _stream.Close();
            if (_client != null)
                _client.Close();
            Environment.Exit(0);
        }

        private static void ConnectionToChat()
        {
            while (true)
            {
                int numberPort;
                Console.WriteLine("Введите ip");
                string host = Console.ReadLine();
                if (host == "выйти") Environment.Exit(0);
                Console.WriteLine("Введите порт");
                string port = Console.ReadLine();
                if (port == "выйти") Environment.Exit(0);
                int.TryParse(port, out numberPort);
                if (host == Host && numberPort == Port)
                    break;
                Console.WriteLine("Введены неверные данные, повторите попытку");
            }
        }
    }
}