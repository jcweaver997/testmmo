using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetTypes
{
    public enum ChannelStatus : int
    {
        Open, Full, Maintenance
    }

    public struct ChannelInfo
    {
        public int players;
        public int maxPlayers;
        public ChannelStatus channelStatus;

        public override string ToString()
        {
            return $"{channelStatus} with {players}/{maxPlayers}";
        }
    }
}
