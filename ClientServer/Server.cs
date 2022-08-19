using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Classwork
{
    public class Server
    {
        private static TcpListener _tcpListener;
        private List<Client> _clients = new List<Client>();
        private const string Host = "127.0.0.1";

        protected internal void AddConnection(Client client)
        {
            _clients.Add(client);
        }

        protected internal void RemoveConnection(string id)
        {
            Client client = _clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                _clients.Remove(client);
        }

        protected internal void Listen()
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, 8888);
                _tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключения");
                while (true)
                {
                    TcpClient tcpClient = _tcpListener.AcceptTcpClient();
                    Client client = new Client(tcpClient, this);
                    Thread clientThread = new Thread(client.Process);
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Disconnect();
            }
        }

        protected internal bool TryValidate(string message, Client newClient)
        {
            byte[] data = new byte[64];
            foreach (var client in _clients)
            {
                if (client.UserName == message)
                {
                    message = $"false";
                    data = Encoding.Unicode.GetBytes(message);
                    newClient.Stream.Write(data, 0 , data.Length);
                    return true;
                }
            }
            ValidationSuccesfully(data,newClient);
            return false;
        }

        protected internal void SendPersonally(string personalyesMessage, string userName)
        {
            byte[] data = new byte[64];
            string[] personalyes = personalyesMessage.Split(',', ':');
            int found = personalyesMessage.IndexOf(":");
            personalyesMessage = personalyesMessage.Substring(found + 0);
            for (int i = 0, j = 0; i < _clients.Count; i++)
            {
                if (_clients[i].UserName == personalyes[j])
                {
                    data = Encoding.Unicode.GetBytes(userName + personalyesMessage);
                    _clients[i].Stream.Write(data, 0, data.Length);
                    Thread.Sleep(10);
                    j++;
                }
            }
        }

        protected internal void ValidationSuccesfully(byte[] data, Client newClient)
        {
            string[] infromation = {"Авторизация прошла успешно ",
                "Список пользователей в сети:"};
            for (int i = 0; i < infromation.Length; i++)
            {
                data = Encoding.Unicode.GetBytes(infromation[i]);
                newClient.Stream.Write(data, 0, data.Length);
                Thread.Sleep(3);
            }
            if(_clients.Count > 1)
                for (int i = 0; i < _clients.Count - 1; i++)
                {
                    infromation[0] = $"{i + 1}){_clients[i].UserName} ";
                    data = Encoding.Unicode.GetBytes(infromation[0]);
                    newClient.Stream.Write(data, 0, data.Length);
                }
        }

        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].Id != id)
                    _clients[i].Stream.Write(data, 0, data.Length);
            }
        }

        protected internal void Disconnect()
        {
            _tcpListener.Stop();
            foreach (var client in _clients)
            {
                client.Close();
                Environment.Exit(0);
            }
        }
    }
}