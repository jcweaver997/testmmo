using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{

    

    public enum NetworkCode : uint
    {
        Null, Error, KeepAlive, Version, Login, ChannelList, ChannelSelect, Telemetry, PlayerInfoRequest, PlayerInfo
    }
}
