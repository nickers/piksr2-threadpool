using System.Threading;

namespace ThreadsIfaces
{
    interface IThreadPool
    {
        bool QueueUserWorkItem (WaitCallback callBack);
        bool SetPoolSize (int size);
        int PoolSize { get; }
    }
}
