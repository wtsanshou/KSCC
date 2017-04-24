using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using _20130520MotionAnalysisStudent.Entity;

namespace _20130520MotionAnalysisStudent.SocketConnect
{
    class CreateClient
    {
        /// <summary>
        /// the thread used to receive information
        /// </summary>
        private Thread receiveThread;

        /// <summary>
        /// the client socket
        /// </summary>
        private Socket mainSocket;


        private byte[] receiveBuffer;
        private byte[] sendBuffer;

        private string IpAddr;
        private int Port;


        public CreateClient(string ipAddr, int port)
        {
            this.IpAddr = ipAddr;
            this.Port = port;
        }

        /// <summary>
        /// set up connect to server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool Connect()
        {
            try
            {
                mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ip = IPAddress.Parse(this.IpAddr);

                this.receiveBuffer = new byte[65535];
                this.sendBuffer = new byte[1024];

                int port = Convert.ToInt32(this.Port);

                IPEndPoint ie = new IPEndPoint(ip, port);

                mainSocket.Connect(ie);

                receiveThread = new Thread(receiveMessage);
                receiveThread.Start(mainSocket);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                this.mainSocket.Shutdown(SocketShutdown.Both);
                this.mainSocket.Close();
                this.mainSocket = null;

                receiveThread.Abort();

                return false;
            }
        }

        /// <summary>
        /// the final buffer used to decode to my class
        /// </summary>
        private byte[] myClassBuffer;

        /// <summary>
        /// detect if the buffer is a new information
        /// </summary>
        private bool isFirst = true;

        /// <summary>
        /// the number has been received
        /// </summary>
        private int receiveLength = 0;

        /// <summary>
        /// the length of the buffer of the class
        /// </summary>
        private int myClassLenght = 0;


        /// <summary>
        /// receive message from server
        /// </summary>
        /// <param name="mainsocket"></param>
        public void receiveMessage(object mainsocket)
        {
            Socket mySocket = (Socket)mainsocket;

            try
            {
                if (mySocket != null && mySocket.Connected)
                {
                    while (true)
                    {
                        ///the information is new
                        if (this.isFirst)
                        {
                            ///first 4 byte used to store the length of a object
                            byte[] firstByte = new byte[4];

                            int first4 = mySocket.Receive(firstByte);
                            if (first4 == 0)
                            {
                                continue;
                            }
                            ///receive the value of class lenght
                            this.myClassLenght = BitConverter.ToInt32(firstByte, 0);

                            ///receive the object
                            this.myClassBuffer = new byte[myClassLenght];


                            int firstLength = mySocket.Receive(this.myClassBuffer);

                            this.receiveLength += firstLength;

                            ///a object received 
                            ///decode it
                            if (this.myClassLenght == this.receiveLength)
                            {

                                Decoder(this.myClassBuffer);
                                this.myClassLenght = 0;
                                this.receiveLength = 0;

                                this.isFirst = true;
                                continue;
                            }
                            ///object receiving is not finished
                            else if (this.myClassLenght > this.receiveLength)
                            {
                                this.isFirst = false;
                                continue;
                            }

                        }
                        ///the information is not finished 
                        ///go on receiving
                        else
                        {
                            ///the rest of the information of the object
                            byte[] partBuffer = new byte[this.myClassLenght - this.receiveLength];

                            int partLength = mySocket.Receive(partBuffer);

                            ///combin the rest infomation with the old buffer
                            Buffer.BlockCopy(partBuffer, 0, this.myClassBuffer, this.receiveLength, partLength);


                            this.receiveLength += partLength;

                            ///object receiving is finished
                            if (this.myClassLenght == this.receiveLength)
                            {
                                Decoder(this.myClassBuffer);
                                this.myClassLenght = 0;
                                this.receiveLength = 0;

                                this.isFirst = true;
                                continue;
                            }
                            ///object receiving is not finished
                            else if (this.myClassLenght > this.receiveLength)
                            {
                                this.isFirst = false;
                                continue;
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        /// <summary>
        /// decode the data which received form teacher
        /// </summary>
        /// <param name="classBuffer"> a whole calss bytes</param>
        public void Decoder(byte[] classBuffer)
        {
            MemoryStream stream;

            //Console.WriteLine(Encoding.Unicode.GetString(receiveBuffer,0,receiveNumber));
            ///set the serializer class
            XmlSerializer se = new XmlSerializer(typeof(XmlJointsCollection));

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            ///read the classBuffer into the stream
            stream = new MemoryStream(classBuffer);

            ///let it read form start index
            stream.Position = 0;

            ///deserialize
            XmlJointsCollection data = se.Deserialize(stream) as XmlJointsCollection;
            stream.Flush();
            Console.WriteLine(data.Joints[0].x);

            ///change it to a Position class
            for (int i = 0; i < JOINTNUMBER; i++)
            {
                receivePositions[i] = new Position(data.Joints[i].x, data.Joints[i].y, data.Joints[i].z);
            }
            this.isGetResult = true;
        }

        /// <summary>
        /// the flag of get a data from teacher
        /// </summary>
        private bool isGetResult = false;

        /// <summary>
        /// the number of joints
        /// </summary>
        private const int JOINTNUMBER = 20;

        /// <summary>
        /// store the data received from teacher
        /// </summary>
        private Position[] receivePositions = new Position[JOINTNUMBER];

        /// <summary>
        /// return the data received form teacher
        /// </summary>
        /// <returns></returns>
        public Position[] GetMessage()
        {
            if (this.receivePositions != null && this.isGetResult)
            {
                this.isGetResult = false;
                return this.receivePositions;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// send start time to teacher
        /// </summary>
        /// <param name="time">start time</param>
        public void Send(string time)
        {
            //string sendInfo = "i come";
            this.sendBuffer = Encoding.Unicode.GetBytes(time);

            try
            {
                if (this.mainSocket != null && this.mainSocket.Connected)
                {
                    this.mainSocket.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);

                    Console.WriteLine("send a message");
                    
                    Console.WriteLine(DateTime.Now.ToUniversalTime());
                }
            }
            catch (Exception ex)
            { }
        }

        public void stop()
        {
            try
            {
                if (this.mainSocket != null && this.mainSocket.Connected)
                {
                    this.mainSocket.Shutdown(SocketShutdown.Both);
                    this.mainSocket.Close();
                    this.mainSocket = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }
}
