using System;
using System.Net.Sockets;
using System.Text;
using static System.Console;

namespace Client
{
    class Program
    {
        static readonly Encoding encoding = Encoding.UTF8;
        static readonly Int32 port = 10010;
        static readonly String host = "120.79.182.165";
        static void Main(string[] args)
        {
            while (true)
            {
                Connect(host);
            }
        }
        static void Connect(String server)
        {
            String message;
            try
            {
                //下三行顺序不换
                Write($"Sent: ");
                message = ReadLine();
                TcpClient client = new TcpClient(server, port);

                Byte[] data = encoding.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                data = new Byte[256];
                String responseData = String.Empty;
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = encoding.GetString(data, 0, bytes);
                WriteLine($"Received: {responseData}");
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                WriteLine("SocketException: {0}", e);
            }
        }
    }
}
