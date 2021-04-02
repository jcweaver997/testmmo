using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace poopstory2_server.NetUtil
{
    public class Locked<T>
    {
        public class LockedValue<TVal> : IDisposable
        {
            private Locked<TVal> instance;
            public TVal Value { private set; get; }
            public LockedValue(Locked<TVal> instance, TVal value)
            {
                this.instance = instance;
                this.Value = value;
            }

            public void Dispose()
            {
                instance.Release();
            }
        }



        private SemaphoreSlim sem;
        private T val;


        public Locked(T val) 
        {
            this.val = val;
            sem = new SemaphoreSlim(1,1);
        }

        public async Task<LockedValue<T>> WaitAsync()
        {
            await sem.WaitAsync();
            return new LockedValue<T>(this, val);
        }

        public LockedValue<T> Wait()
        {
            sem.Wait();
            return new LockedValue<T>(this,val);
        }

        public void Release()
        {
            sem.Release();
        }
    }
}
