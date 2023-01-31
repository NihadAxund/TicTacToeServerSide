using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TicTacToeServerSide.Help;

namespace TicTacToeServerSide.Services
{
    public class NetworkService
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 1000000;
        private const int PORT = 27001;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private static bool GameStart = false;
        private static List<UserLogin> ULLIST { get; set; } = new List<UserLogin>();
        public static bool IsFirst { get; private set; } = false;
        public static char[,] Points = new char[3, 3] { { '1', '2', '3' }, { '4', '5', '6' }, { '7', '8', '9' } };

        public static void Start()
        {
            Console.Title = "Server";
           
                SetupServer();
            Console.ReadLine();
            CloseAllSockets();
        }
        private static bool XOBole3(char t)
        {
       
            int num = (Points.Length / 3)-1;
            for (int i = 0; i < Points.Length/3; i++)
            {
                if (Points[i, num] == t) num--;
                else return false;
            }
            return true;

        }

        private static bool XOBole2(char t)
        {
            int num = (Points.Length / 3)-1; //2
            for (int i = (Points.Length/3)-1; i >= 0; i--) //2  
            {
              //  Console.WriteLine(i);
                if (Points[i, num] == t) num--;
                else return false;
            }
            return true;
        }
        private static bool XOBole1(char t)
        {
            bool isok = false;
            for (int i = 0; i < Points.Length / 3; i++) //0 //0 //1
            {
                for (int j = 0; j < Points.Length / 3; j++)//0 //1
                {
                    if (Points[j, i] == t) isok = true;
                    else
                    {
                        isok = false;
                        break;
                    }
                }
                if (!isok)
                {
                    continue;
                }
                else
                {
                    //Console.ForegroundColor = ConsoleColor.Green;
                    //Console.WriteLine(t + "Salam");
                    return isok;
                }
            }
            return false;

        }
        private static bool XOBole(char t)
        {
            bool isok = false;
            for (int i = 0; i < Points.Length/3; i++)
            {
                for (int j = 0; j < Points.Length/3; j++)
                {
                    if (Points[i,j]==t)isok = true;
                    else
                    {
                        isok = false;
                        break;
                    }
                }
                if (!isok)
                {
                    continue;
                }
                else
                {
                    //Console.ForegroundColor = ConsoleColor.Green;
                    //Console.WriteLine(t + "Salam");
                    return isok;
                }
            }
            return false;

        }
        private static void CloseAllSockets()
        {
            foreach (var socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            serverSocket.Close();
        }

        private static void SetupServer()
        {
            Task.Run(() =>
            {
                Console.WriteLine("Setting up server . . . ");
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
                serverSocket.Listen(2);
                while (true)
                {
                    try
                    {
                        serverSocket.BeginAccept(AcceptCallBack, null);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                }
            });
        }

        private static void AcceptCallBack(IAsyncResult ar)
        {
            Socket socket = null;
            try
            {
                socket = serverSocket.EndAccept(ar);
            }
            catch (Exception)
            {
                return;
            }

            clientSockets.Add(socket);
            Console.WriteLine($"{socket.RemoteEndPoint} connected");
            string t = "";
            if (!IsFirst)
            {
                IsFirst = true;
                t = "X";
            }
            else
            {
                IsFirst = false;
                t = "O";
               // GameStart = true;
                
            }
            byte[] data = Encoding.ASCII.GetBytes(t+"\t"+IsFirst.ToString());
            socket.Send(data);
            //if (!IsFirst)
                socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            //{
            //}
        }

        private static bool Boolresult(char symbol)
        {
            if (XOBole3(symbol) || XOBole2(symbol))
                return true;
            else if (XOBole(symbol) || XOBole1(symbol))
                return true;
           return false;
        }

        static bool IsOkay = false;
        private static  void ReceiveCallback(IAsyncResult ar)
        {
            lock(clientSockets)
            {
                bool isok1 = true;
                Socket current = (Socket)ar.AsyncState;
                int received;

                try { received = current.EndReceive(ar); }
                catch (Exception)
                {
                    Console.WriteLine("Client forcefully disconnected");
                    current.Close();
                    clientSockets.Remove(current);
                    return;
                }
                byte[] recBuf = new byte[received];
                Array.Copy(buffer, recBuf, received);
                string text = Encoding.ASCII.GetString(recBuf);
                if (isok1 && !GameStart)
                {
                    isok1 = false;
                    if (ULLIST.Count > 0)
                    {
                        GameStart = true;
                    }
                    Console.WriteLine("Dusdu buraya");
                    var USer = JsonSerializer.Deserialize<UserLogin>(text);
                    ULLIST.Add(USer);
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(USer.UserName);
                    Console.BackgroundColor = ConsoleColor.Black;
                    current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
                    return;
                }
                if (!GameStart)
                {
                    current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
                    return;
                }
                try
                {
                    var no = text[0];
                    var symbol = text[1];
                    Console.WriteLine("Budu: " + no + "||" + symbol);
                    var number = Convert.ToInt32(no) - 49;
                    if (number >= 0 && number <= 2)
                        Points[0, number] = symbol;
                    else if (number >= 3 && number <= 5)
                        Points[1, number - 3] = symbol;
                    else if (number >= 6 && number <= 8)
                        Points[2, number - 6] = symbol;
                    if (Boolresult(symbol))
                    {
                        IsOkay = true;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("WINER: " + symbol);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }



                if (text != String.Empty)
                {
                    var mydata = ConvertString(Points);
                    foreach (var item in clientSockets)
                    {
                        if (item == current)
                        {
                            if (IsOkay)
                            {
                                byte[] data = Encoding.ASCII.GetBytes(mydata + "True\nTrue");
                                item.Send(data);
                            }
                            else
                            {
                                byte[] data = Encoding.ASCII.GetBytes(mydata + "True");
                                item.Send(data);
                            }
                        }
                        else
                        {
                            if (IsOkay)
                            {
                                byte[] data = Encoding.ASCII.GetBytes(mydata + "True\nFalse");
                                item.Send(data);
                            }
                            else
                            {
                                byte[] data = Encoding.ASCII.GetBytes(mydata + "False");
                                item.Send(data);
                            }
                        }

                        Console.WriteLine($"Data sent to {item.RemoteEndPoint}");
                    }
                }
                else if (text == "exit")
                {
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    clientSockets.Remove(current);
                    Console.WriteLine($"{current.RemoteEndPoint} disconnected");
                    return;
                }
                else
                {
                    Console.WriteLine("Text is an invalid request");
                    byte[] data = Encoding.ASCII.GetBytes("Invalid Request");
                    current.Send(data);
                    Console.WriteLine("Warning Sent");
                }
                try
                {
                    current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
                }
                catch (Exception)
                {
                    Console.WriteLine($"{current.RemoteEndPoint} disconnected");
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    clientSockets.Remove(current);
                }
            }
        }
          

        private static string ConvertString(char[,] points)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < points.Length/3; i++)
            {
                for (int k = 0; k < points.Length / 3; k++)
                {
                    sb.Append(points[i, k]);
                    sb.Append('\t');
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
