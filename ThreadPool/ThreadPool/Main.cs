using System;
using ThreadsIfaces;
using System.Threading;
using Mono.Unix.Native;


namespace nThreadPool
{
	/// <summary>
	/// Tomasz Wsuł
	/// inf80169
	/// 2nickers@gmail.com
	/// </summary>
	class MainClass
	{
		/// <summary>
		/// Must be dividable by 4
		/// </summary>
		public static long TEST_SIZE = 10000000;
		public static long MOD = TEST_SIZE/100;
		
		public static Object sync = new Object();
		public static Int64 cnt = 0;
		
		public static void SuperTest()
		{
			long tm=0;
			Mono.Unix.Native.Syscall.time(out tm);
			nThreadPool tp = new nThreadPool(3);
			for (int th=0; th<4; th++) 
			{
				Thread _th = new Thread(() => {
					for (int i=0; i<TEST_SIZE/4; i++) 
					{
						tp.QueueUserWorkItem((Object o) => {
							Monitor.Enter(MainClass.sync);
							MainClass.cnt++;
							if (MainClass.cnt%MOD==0)
							{
								IThreadPool p = (IThreadPool)o;
								if (p.PoolSize==10)
									p.SetPoolSize(100);
								else
									p.SetPoolSize(10);
								Console.WriteLine(Thread.CurrentThread.Name);
							}
							if (MainClass.cnt>=TEST_SIZE)
							{
								long tm2=0;
								Mono.Unix.Native.Syscall.time(out tm2);
								Console.WriteLine("Time: " + (tm2-tm));
							}
							Monitor.Exit(MainClass.sync);
						});
					}
				});
				_th.Start();
			}		
			Console.WriteLine("rozpoczęto dodawanie");

/*
			Thread.Sleep(5);
			tp.SetPoolSize(10);
			Thread.Sleep(11);
			tp.SetPoolSize(2);
			Thread.Sleep(11);
			tp.SetPoolSize(20);
			tp.SetPoolSize(12);
			tp.SetPoolSize(30);
			tp.SetPoolSize(3);
			tp.SetPoolSize(1);
*/
			while (MainClass.cnt<TEST_SIZE) {
				Thread.Sleep(300);
				Console.WriteLine(MainClass.cnt);
			}
			
			Console.WriteLine("koniec");
		}

		public static void Main (string[] args)
		{
			Console.WriteLine (" >> Hello World! << ");
			MainClass.SuperTest();
			Console.WriteLine(" >> koniec << ");
		}
	}
}

