using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using _20130514MotionAnalysisTeacher.Entity;
using System.IO;
using System.Xml.Serialization;

namespace _20130514MotionAnalysisTeacher.SocketSetup
{
    class CreateServer
    {

        ////////////////////////////// Socket ////////////////////////////////////
        private IPEndPoint ServerInfo;//存放服务器的IP和端口信息
        private Socket ServerSocket;//服务端运行的SOCKET
        private Thread ServerThread;//服务端运行的线程
        private Socket[] ClientSocket;//为客户端建立的SOCKET连接
        private int ClientNumb;//存放客户端数量
        private Byte[] receiveBuffer;//存放消息数据
        private Byte[] sendBuffer;

        private int TPort = 6600;

        const int JOINTNUMBER = 20;


        public CreateServer(int port)
        {
            this.TPort = port;
            Setup();
        }
        /*
    public void sendOutPositions(GatherData data)
    {
        MemoryStream stream = new MemoryStream();
        XmlSerializer se = new XmlSerializer(typeof(GatherData));
        se.Serialize(stream, data);
        stream.Flush();

        byte[] buffer = new byte[stream.Length];//一次性发送（如果设定为固定值要循环多次发送）
        stream.Position = 0;  //将流的当前位置重新归0，否则Read方法将读取不到任何数据\
        try
        {
            for (int i = 0; i <= ClientNumb; i++)
            {
                if (ClientSocket[i].Connected)
                {
                    while (stream.Read(buffer, 0, buffer.Length) > 0)
                    {

                        ClientSocket[i].Send(buffer, 0, buffer.Length, SocketFlags.None); //从内存中读取二进制流，并发送
                    }
                }
                else
                {
                    Console.WriteLine("_________________");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
  */

        public void sendOutPositions(XmlJointsCollection tSkeleton)
        {
            MemoryStream stream = new MemoryStream();
            XmlSerializer se = new XmlSerializer(typeof(XmlJointsCollection));

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            se.Serialize(stream, tSkeleton, ns);
            
            stream.Flush();

            sendBuffer = new byte[stream.Length];

            try
            {
                for (int i = 0; i <= ClientNumb; i++)
                {
                    if (ClientSocket[i]!=null && ClientSocket[i].Connected)
                    {
                        stream.Position = 0;  //将流的当前位置重新归0，否则Read方法将读取不到任何数据\

                        byte[] part = new byte[4];
                        part = BitConverter.GetBytes(sendBuffer.Length);
                        ClientSocket[i].Send(part, 0, part.Length, SocketFlags.None);

                        while (stream.Read(sendBuffer, 0, sendBuffer.Length) > 0)
                        {
                            ClientSocket[i].Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None); //从内存中读取二进制流，并发送


                            //Console.WriteLine("send a data!" + sendBuffer.Length);
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
  
        private void Setup()
        {
            try
            {
                IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
                IPAddress _IP = null;
                foreach (IPAddress ip in ips)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        //ipAddress.Content = ip.ToString();
                        _IP = ip;
                    }
                }

                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //提供一个 IP 地址，指示服务器应侦听所有网络接口上的客户端活动
                ServerInfo = new IPEndPoint(IPAddress.Any, this.GetPort());
                ServerSocket.Bind(ServerInfo);//将SOCKET接口和IP端口绑定
                ServerSocket.Listen(10);//开始监听，并且挂起数为10
                ClientSocket = new Socket[1024];//为客户端提供连接个数
                receiveBuffer = new Byte[65535];//消息数据大小
                //sendBuffer = new Byte[65535];
                ClientNumb = 0;//数量从0开始统计
                ServerThread = new Thread(new ThreadStart(RecieveAccept));//将接受客户端连接的方法委托给线程
                ServerThread.Start();//线程开始运行

                Console.WriteLine("Server is running...");
                Console.WriteLine("IP:" + _IP + "\t Port：" + this.GetPort().ToString());
                Console.WriteLine("Server started at " + DateTime.Now.ToString() + ".");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private int GetPort()
        {
            try
            {
                return TPort;
            }
            catch { return 6600; }//默认是6600
        }

        //接受客户端连接的方法
        private void RecieveAccept()
        {

            while (true)
            {
                ClientSocket[ClientNumb] = ServerSocket.Accept();

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(ClientSocket[ClientNumb]);
                Console.WriteLine("client number: "+ ClientNumb);
                ClientNumb++;
            }
        }

        private double startTime = 0;

        public double GetStartTime()
        {
            return startTime;
        }

        private void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;

            try
            {
                while (true)
                {
                    if (myClientSocket!=null && myClientSocket.Connected)
                    {
                        int receiveNum = myClientSocket.Receive(receiveBuffer);

                        if (receiveNum == 0)
                        {
                            break;
                        }

                        string receiveStr = Encoding.Unicode.GetString(receiveBuffer, 0, receiveNum);

                        this.startTime = Convert.ToDouble(receiveStr);

                        Console.WriteLine(DateTime.Now.ToUniversalTime());
                        Console.WriteLine("teacher: " + DateTime.Now.ToOADate().ToString());

                        Console.WriteLine(receiveStr);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                myClientSocket.Shutdown(SocketShutdown.Both);
                myClientSocket.Close();
            }
        }
    }
}
