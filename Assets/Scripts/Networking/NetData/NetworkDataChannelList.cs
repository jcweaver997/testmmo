using poopstory2_server.NetTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataChannelList : NetworkData
    {
        public ChannelInfo[] channelInfo;


        public NetworkDataChannelList(ChannelInfo[] channels) : base(4 * 3 * channels.Length+4, NetworkCode.ChannelList)
        {
            channelInfo = channels;
        }

        public NetworkDataChannelList() : base(0, NetworkCode.ChannelList)
        {

        }

        protected override void Build()
        {
            AddInt(channelInfo.Length);
            for(int i = 0; i < channelInfo.Length; i++)
            {
                AddInt(channelInfo[i].players);
                AddInt(channelInfo[i].maxPlayers);
                AddInt((int)channelInfo[i].channelStatus);
            }
        }

        protected override NetworkData ParseData(byte[] data)
        {
            ChannelInfo[] channelInfo;
            int numChannels;
            int index = ReadInt(data,0,out numChannels);
            channelInfo = new ChannelInfo[numChannels];

            for (int i = 0; i < numChannels; i++)
            {
                index = ReadInt(data, index, out channelInfo[i].players);
                index = ReadInt(data, index, out channelInfo[i].maxPlayers);
                int cs;
                index = ReadInt(data, index, out cs);
                channelInfo[i].channelStatus = (ChannelStatus)cs;
            }
            return new NetworkDataChannelList(channelInfo);
        }
    }
}
