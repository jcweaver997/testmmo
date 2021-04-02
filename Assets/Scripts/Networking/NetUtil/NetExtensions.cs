using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.NetUtil
{
    public static class QueueHelper
    {

        public static bool TryDequeue<T>(this Queue<T> q, out T t)
        {
            try
            {
                t = q.Dequeue();
                return true;
            }
            catch (InvalidOperationException)
            {
                t = default(T);
                return false;
            }

        }

    }
}
