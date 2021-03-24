using poopstory2_server.NetTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataPlayerInfoRequest : NetworkData
    {
        public int pid;

        public NetworkDataPlayerInfoRequest() : base(0, NetworkCode.PlayerInfoRequest) { }
        public NetworkDataPlayerInfoRequest(int pid) : base(4, NetworkCode.PlayerInfoRequest) {
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
            return new NetworkDataPlayerInfoRequest(pid);
        }
    }
}
