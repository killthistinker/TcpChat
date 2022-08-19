using System;
using System.Net.Sockets;
using System.Text;

namespace Classwork
{
    public class Client
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        protected internal string UserName;
        private TcpClient _tcpClient;
        private Server _server;

        public Client(TcpClient tcpClient, Server server)
        {
            _tcpClient = tcpClient;
            this._server = server;
            Id = Guid.NewGuid().ToString();
            server.AddConnection(this);
        }
        
        public void Process()
        {
            try
            {
                Stream = _tcpClient.GetStream();
                string message = GetMessage();
                
                bool validate = _server.TryValidate(message, this);
                while (validate)
                {
                    message = GetMessage();
                    validate = _server.TryValidate(message, this);
                }
                UserName = message;
                message = UserName + " вошел в чат";
                _server.BroadcastMessage(message, Id);
                Console.WriteLine(message);
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        if (message == "personaly")
                        {
                            message = GetMessage();
                            _server.SendPersonally(message, UserName);
                        }
                        else
                        {
                            string nameAndDate = String.Format("{0} - ({1})", UserName, DateTime.Now.ToShortDateString());
                            Console.WriteLine(nameAndDate);
                            _server.BroadcastMessage(nameAndDate, Id);
                            message = String.Format("Текст сообщения : {0}", message);
                            Console.WriteLine(message);
                            _server.BroadcastMessage(message, Id);
                        }
                    }
                    catch
                    {
                        message = $"{UserName} покинул чат";
                        Console.WriteLine(message);
                        _server.BroadcastMessage(message, Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _server.RemoveConnection(Id);
                Close();
            }
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (Stream.DataAvailable);
            return builder.ToString();
        }

        protected internal void Close()
        {
            Stream?.Close();
            _tcpClient?.Close();
        }
    }
}