using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataChannelSelect : NetworkData
    {
        public int channel;
        public NetworkDataChannelSelect():base(0,NetworkCode.ChannelSelect){}

        public NetworkDataChannelSelect(int channel):base(4, NetworkCode.ChannelSelect)
        {
            this.channel = channel;
        }
        protected override void Build()
        {
            AddInt(channel);
        }

        protected override NetworkData ParseData(byte[] data)
        {
            int c;
            ReadInt(data,0,out c);
            return new NetworkDataChannelSelect(c);
        }
    }
}
