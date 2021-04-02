using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataTypes
    {
        private static Dictionary<NetworkCode, NetworkData> codeToType = new Dictionary<NetworkCode, NetworkData>();
        private static bool initialized = false;
        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            Add(new NetworkDataError(), NetworkCode.Error);
            Add(new NetworkDataLogin(), NetworkCode.Login);
            Add(new NetworkDataChannelList(), NetworkCode.ChannelList);
            Add(new NetworkDataVersion(), NetworkCode.Version);
            Add(new NetworkDataKeepAlive(), NetworkCode.KeepAlive);
            Add(new NetworkDataChannelSelect(), NetworkCode.ChannelSelect);
            Add(new NetworkDataTelemetry(), NetworkCode.Telemetry);
            Add(new NetworkDataPlayerInfo(), NetworkCode.PlayerInfo);
            Add(new NetworkDataPlayerInfoRequest(), NetworkCode.PlayerInfoRequest);
            Add(new NetworkDataPlayerLeft(), NetworkCode.PlayerLeft);

        }

        private static void Add(NetworkData t, NetworkCode code)
        {
            codeToType.Add(code,t);
        }

        public static NetworkData GetT(NetworkCode i)
        {
            if (codeToType.ContainsKey(i))
            {
                return codeToType[i];
            }
            return null;
        }
    }
}
