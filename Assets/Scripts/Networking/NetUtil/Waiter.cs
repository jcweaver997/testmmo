using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace poopstory2_server.NetUtil
{
    class Waiter
    {
        EventWaitHandle wh;
        int waiting = 0;
        public Waiter()
        {
            wh = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        public async Task WaitAsync(CancellationToken token)
        {
            waiting++;
            await Task.Run(() => WaitHandle.WaitAny(new[] { wh, token.WaitHandle }));
            waiting--;
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }

        public void Wait()
        {
            waiting++;
            wh.WaitOne();
            waiting--;
        }

        public bool IsWaiting()
        {
            return waiting > 0;
        }

        public void Signal()
        {
            wh.Set();
        }
    }
}
