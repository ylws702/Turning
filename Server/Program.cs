using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Server
{
    class Program
    {
        readonly static Encoding encoding = Encoding.UTF8;
        readonly static Int32 port = 10010;
        readonly static IPAddress address = IPAddress.Parse("0.0.0.0");
        static void Main(String[] args)
        {
            while (true)
            {
                try
                {
                    Listen();
                }
                catch (SocketException e)
                {
                    WriteLog(e);
                }
            }
        }
        static void Listen()
        {
            //TODO:
            var userid = "123456";
            TcpListener server = new TcpListener(address, port);
            server.Start();
            Byte[] bytes = new byte[256];
            String received, sent;
            while (true)
            {
                WriteLine("Waiting for a connection...");
                TcpClient client = server.AcceptTcpClient();
                WriteLine("Connected!");
                received = null;
                sent = null;
                NetworkStream stream = client.GetStream();
                Int32 i;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    received = encoding.GetString(bytes, 0, i);
                    WriteLine($"Received: {received}");
                    var getMessageTask = GetTurning(received, userid);
                    getMessageTask.Wait();
                    sent = getMessageTask.Result;
                    byte[] message = encoding.GetBytes(sent);
                    stream.Write(message, 0, message.Length);
                    WriteLine($"Sent: {sent}");
                    WriteLog(received, sent);
                }
                client.Close();
            }
        }
        static readonly String APIkey = "bc0c925d46074df687549e9af6362c48";
        static async Task<String> GetTurning(String str, String userid)
        {
            HttpClient client = new HttpClient();
            String request = $"{"http://"}www.tuling123.com/openapi/api?key={ APIkey }&info={str}&userid={userid}";
            try
            {
                HttpResponseMessage response = await client.GetAsync(request);
                response.EnsureSuccessStatusCode();
                String responseBody = await response.Content.ReadAsStringAsync();
                var responseText = Json.GetVals(responseBody, "text");
                if (responseText.Length > 0)
                {
                    return responseText[0];
                }
                else
                {
                    return "emmm";
                }
            }
            catch (HttpRequestException e)
            {
                WriteLog(e);
                return $"HttpRequestException: {e}";
            }
        }
        public static void WriteLog(String received, String sent)
        {
            String filePath = AppDomain.CurrentDomain.BaseDirectory + "Log";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            String logPath = AppDomain.CurrentDomain.BaseDirectory + "Log/" + DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                using (StreamWriter sw = File.AppendText(logPath))
                {
                    sw.WriteLine("**************************************************");
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sw.WriteLine($"Received: {received}");
                    sw.WriteLine($"Sent: {sent}");
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (IOException e)
            {
                WriteLog(e);
            }
        }
        public static void WriteLog(Exception e)
        {
            WriteLine($"{e.GetType()}: {e}");
            String filePath = AppDomain.CurrentDomain.BaseDirectory + "Log";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            String logPath = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                using (StreamWriter sw = File.AppendText(logPath))
                {
                    sw.WriteLine("**************************************************");
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sw.WriteLine($"{e.GetType()}: {e}");
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (IOException ex)
            {
                WriteLine($"IOException: {ex}");
            }
        }
    }
    /// <summary> 
    /// 解析json字符串 
    /// </summary> 
    public static class Json
    {
        /// <summary> 
        /// 获取json中key对应的value 
        /// </summary> 
        /// <param name="str">json字符串</param> 
        /// <param name="key">查找的key值</param> 
        /// <returns></returns> 
        public static String[] GetVals(String str, String key)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Char ch;
            for (int i = 0; i < str.Length; i++)
            {
                ch = str[i];
                if (' ' == ch || '\t' == ch || '\n' == ch || '\r' == ch || '{' == ch || '}' == ch)
                {
                    continue;
                }
                stringBuilder.Append(str[i]);
            }
            var splitArr = stringBuilder.ToString().Split('\"');
            var strList = new List<String>();
            for (int i = 0; i < splitArr.Length; i++)
            {
                if (splitArr[i] == key)
                {
                    if (splitArr[i + 1] == ":")
                    {
                        strList.Add(splitArr[i + 2]);
                    }
                    else
                    {
                        strList.Add(splitArr[i + 1].Substring(1, splitArr[i + 1].Length - 2));
                    }
                }
            }
            return strList.ToArray();
        }
    }
}
