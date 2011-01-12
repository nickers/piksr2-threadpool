using System;
using System.Threading;
using ThreadsIfaces;
using System.Collections.Generic;

namespace nThreadPool
{
	/// <summary>
	/// Magic thread pool made by most briliant programmer in th world!
	/// 
	/// Tomasz Wsuł
	/// inf80169
	/// 2nickers@gmail.com
	/// </summary>
	public class nThreadPool : IThreadPool
	{
		private int poolSize;
		private long threadsNames = 0;
		private long threadsToDestroy = 0;
		
		private List<Thread> threads = null;
		private Queue<WaitCallback> jobs = null;

		/// <summary>
		/// Guess what? It is constructor!
		/// </summary>
		/// <param name="size">
		/// Initial number of threads.
		/// </param>
		public nThreadPool (int size)
		{
			threads = new List<Thread>();
			jobs = new Queue<WaitCallback>();
			SetPoolSize(size);
		}
		
		/// <summary>
		/// Destructor.
		/// </summary>
		~nThreadPool()
		{
			Destroy();
		}

		/// <summary>
		/// Waits for all threads to finish and destroys them.
		/// </summary>
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
		
		/// <summary>
		/// Adds job to be executed by thread pool.
		/// </summary>
		/// <param name="callBack">
		/// Job to be executed. <see cref="WaitCallback"/>
		/// </param>
		/// <returns>
		/// True if succeded, False otherwise.<see cref="System.Boolean"/>
		/// </returns>
		public bool QueueUserWorkItem (WaitCallback callBack)
		{
			Monitor.Enter(jobs);
			jobs.Enqueue(callBack);
			Monitor.Pulse(jobs);
			Monitor.Exit(jobs);
			return true;
		}

		
		/// <summary>
		/// Sets desired amount of threads in pool.
		/// 
		/// If current amount of threads in pool is bigger than desired one
		/// some threads are destroyed, but not until they finish current job (if executing any).
		/// If all threads are currently occupied first threads will be killed after they finish 
		/// executing current job.
		/// </summary>
		/// <param name="size">
		/// Desired amount of threads in pool.
		/// </param>
		/// <returns>
		/// True if ok, False if failed(invalid amount of threads passed).
		/// </returns>
        public bool SetPoolSize (int size)
		{
			if (size<0)
				return false;

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

		/// <summary>
		/// Returns desired thread pool size.
		/// 
		/// Actual number of threads may by bigger for some time, if size was just
		/// decreased and not all unneeded threads were killed.
		/// </summary>
        public int PoolSize {  get { return poolSize; } }
		
		/// <summary>
		/// Creates new background thread, adds it to pool and returns it.
		/// </summary>
		/// <returns>
		/// Worker thread already added to pool. <see cref="Thread"/>
		/// </returns>
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
							c = jobs.Dequeue();
							Monitor.Pulse(jobs);
							Monitor.Exit(jobs);
							c.Invoke(this);
						}
					} catch (ThreadInterruptedException) {
						// it is ok, we know what we need to do
						return;
					} catch (ThreadAbortException) {
						// the same as above
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

