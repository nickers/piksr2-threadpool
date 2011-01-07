using System;
using System.Threading;
using ThreadsIfaces;
using System.Collections.Generic;

namespace nThreadPool
{
	public class nThreadPool : IThreadPool
	{
		private int poolSize;
		private long threadsNames = 0;
		private long threadsToDestroy = 0;
		
		private List<Thread> threads = null;
		private Queue<WaitCallback> jobs = null;

		public nThreadPool (int size)
		{
			threads = new List<Thread>();
			jobs = new Queue<WaitCallback>();
			SetPoolSize(size);
		}
		
		~nThreadPool()
		{
			Destroy();
		}

		/**
		 * 
		 */
		private void Destroy()
		{
			Monitor.Enter(threads);
			SetPoolSize(0);
			foreach (Thread t in threads) 
			{
				while (t.IsAlive)
				{
					Monitor.Enter(jobs);
					Monitor.PulseAll(jobs);
					Monitor.Exit(jobs); 
				}
			}
			Monitor.Exit(threads);
		}
		
		
		public bool QueueUserWorkItem (WaitCallback callBack)
		{
			Monitor.Enter(jobs);
			//jobs.Add(callBack);
			jobs.Enqueue(callBack);
			Monitor.Pulse(jobs);
			Monitor.Exit(jobs);
			return true;
		}

        public bool SetPoolSize (int size)
		{
//XXX			Console.WriteLine("new_size: " + size);
			Monitor.Enter(threads);
			if (threads.Count < size) 
			{
				for (int i=threads.Count; i<size; i++) 
				{
					Thread t = AddThread();
					t.Name = "IThreadPoolWorker#" + threadsNames++;
					t.Start();
				}
			}
			else if (threads.Count > size)
			{
				Monitor.Enter(jobs);
				threadsToDestroy = threads.Count - size;
				Monitor.PulseAll(jobs);
				Monitor.Exit(jobs);
			}
			Monitor.Exit(threads);
			poolSize = size;
			return true;
		}

		/**
		 *
		 */
        public int PoolSize {  get { return poolSize; } }
		
		private Thread AddThread() 
		{
			Thread th = new Thread(() => {
				while (true) {
					
					try {
						WaitCallback c = null;
						Monitor.Enter(jobs);
						while (jobs.Count <= 0 && threadsToDestroy<=0) Monitor.Wait(jobs);
						
						if (threadsToDestroy > 0) 
						{
///							Console.WriteLine("Exit: " + Thread.CurrentThread.Name);
							threadsToDestroy--;
							Monitor.Exit(jobs);
							Monitor.Enter(threads);
							if (poolSize<threads.Count)
							{
								threads.Remove(Thread.CurrentThread);
								Monitor.Exit(threads);
								return;
							}
							Monitor.Exit(threads);
							Monitor.Enter(jobs);
						}
						if (jobs.Count>0)
						{
							//c = jobs[0];
							//jobs.RemoveAt(0);
							c = jobs.Dequeue();
							Monitor.Pulse(jobs);
							Monitor.Exit(jobs);
							c.Invoke(this);
						}
					} catch (ThreadInterruptedException) {
						return;
					} catch (ThreadAbortException) {
						return;
					} catch (Exception e) {
						Console.Error.WriteLine(e.ToString());
						return;
					}
				}
			});
			
			th.IsBackground = true;
			
			Monitor.Enter(threads);
			threads.Add(th);
			Monitor.Exit(threads);
			return th;
		}
	}
}

