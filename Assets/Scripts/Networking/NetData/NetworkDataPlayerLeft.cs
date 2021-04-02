using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataPlayerLeft : NetworkData
    {
        public int pid;
        public NetworkDataPlayerLeft() : base(0, NetworkCode.PlayerLeft) { }

        public NetworkDataPlayerLeft(int pid) : base(4, NetworkCode.PlayerLeft) {
            this.pid = pid;
        }

        protected override void Build()
        {
            AddInt(pid);
        }

        protected override NetworkData ParseData(byte[] data)
        {
            int pid;
            ReadInt(data,0,out pid);
            return new NetworkDataPlayerLeft(pid);
        }
    }
}
