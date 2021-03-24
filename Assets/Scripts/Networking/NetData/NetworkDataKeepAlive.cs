using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataKeepAlive : NetworkData
    {
        public NetworkDataKeepAlive() : base(0,NetworkCode.KeepAlive)
        {

        }


        protected override void Build()
        {
            
        }

        protected override NetworkData ParseData(byte[] data)
        {
            return new NetworkDataKeepAlive();
        }
    }
}
