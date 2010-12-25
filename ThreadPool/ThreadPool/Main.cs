using System;
using ThreadsIfaces;
using System.Threading;
using Mono.Unix.Native;

namespace nThreadPool
{
	class MainClass
	{
		public static long TEST_SIZE = 10000000;
		public static long MOD = TEST_SIZE/100;
		
		public static Object sync = new Object();
		public static Int64 cnt = 0;
		
		public static void ddd()
		{
			long tm=0;
			Mono.Unix.Native.Syscall.time(out tm);
			nThreadPool tp = new nThreadPool(3);
			for (int i=0; i<TEST_SIZE; i++) {
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
//			Thread.Sleep(30);
			
			while (MainClass.cnt<TEST_SIZE) {
				Thread.Sleep(100);
				Console.WriteLine(MainClass.cnt);
			}
			
			
			Console.WriteLine("koniec");
		}
		
		/**
		 * 
		 */
		public static void ddd2()
		{
			int i1,i2;
			MainClass.cnt=0;
			long tm=0;
			Mono.Unix.Native.Syscall.time(out tm);
//			nThreadPool tp = new nThreadPool(3);
			ThreadPool.GetMaxThreads(out i1, out i2);
			ThreadPool.SetMaxThreads(3,i2);
			ThreadPool.SetMinThreads(3,i2);
			                         
			for (int i=0; i<TEST_SIZE; i++) {
				ThreadPool.QueueUserWorkItem((Object o) => {
					Monitor.Enter(MainClass.sync);
					MainClass.cnt++;
					if (MainClass.cnt%MOD==0)
					{
						//IThreadPool p = (IThreadPool)o;
						//long i1,i2;
						ThreadPool.GetMaxThreads(out i1,out i2);
						if (i1==10)
						{
							ThreadPool.SetMaxThreads(100,i2);
							ThreadPool.SetMinThreads(100,i2);
						}
						else
						{
							ThreadPool.SetMaxThreads(10,i2);
							ThreadPool.SetMinThreads(10,i2);
						}
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
			
			/*
			Thread.Sleep(5);
			ThreadPool.SetMaxThreads(10,i2);
			ThreadPool.SetMinThreads(10,i2);
			Thread.Sleep(11);
			ThreadPool.SetMaxThreads(2,i2);
			ThreadPool.SetMinThreads(2,i2);
			Thread.Sleep(11);
			ThreadPool.SetMaxThreads(20,i2);
			ThreadPool.SetMinThreads(20,i2);
			
			ThreadPool.SetMaxThreads(12,i2);
			ThreadPool.SetMinThreads(12,i2);
			
			ThreadPool.SetMaxThreads(30,i2);
			ThreadPool.SetMinThreads(30,i2);
			
			ThreadPool.SetMaxThreads(3,i2);
			ThreadPool.SetMinThreads(3,i2);
			
			ThreadPool.SetMaxThreads(1,i2);
			ThreadPool.SetMinThreads(1,i2);
			*/
			
//			Thread.Sleep(30);
			
			while (MainClass.cnt<TEST_SIZE) {
				Thread.Sleep(100);
				Console.WriteLine(MainClass.cnt);
			}
			
			
			Console.WriteLine("koniec");
		}
		
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			
			for (int i=0; i<args.Length; i++) 
			{
				Console.WriteLine(i + ") " + args[i]);
			}
			
			//if (args.Length>0 && args[0]=="my")
			{
				MainClass.ddd();
				Console.WriteLine("Main finish");
			}
//			Thread.Sleep(3000);

			//if (args.Length==0 || args[0]=="tp") 
			{
			//	MainClass.ddd2();
			}
//			Console.WriteLine("Main finish");
//			Thread.Sleep(3000);
			
			Console.WriteLine(" >> koniec << ");
		}
	}
}

