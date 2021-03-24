using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class WaitObject<T>
    {

        private EventWaitHandle waitHandle;
        private Mutex objMutex;
        private T obj;
        private int waiting;
        public WaitObject()
        {
            waitHandle = new EventWaitHandle(false,EventResetMode.AutoReset);
            waiting = 0;
            objMutex = new Mutex();
        }

        public T Wait()
        {
            waiting++;
            T obj;
            waitHandle.WaitOne();
            objMutex.WaitOne();
            obj = this.obj;
            waiting--;
            objMutex.ReleaseMutex();
            return obj;
        }

        public bool Give(T t)
        {
            if(waiting>0)
            {
                objMutex.WaitOne();
                obj = t;
                waitHandle.Set();
                objMutex.ReleaseMutex();
                return true;
            }
            return false;
        }



    }
}
