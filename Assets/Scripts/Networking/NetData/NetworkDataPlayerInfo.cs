using poopstory2_server.NetTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataPlayerInfo : NetworkData
    {
        public PlayerData playerData;
        public NetworkDataPlayerInfo() : base(0, NetworkCode.PlayerInfo) { }
        public NetworkDataPlayerInfo(PlayerData playerData) : base(4 + 4 + 4 + playerData.name.Length + 7 * 4, NetworkCode.PlayerInfo)
        {
            this.playerData = playerData;
        }
        protected override void Build()
        {
            AddInt(playerData.id);
            AddInt(playerData.channel);
            AddString(playerData.name);
            AddFloat(playerData.telemetry.px);
            AddFloat(playerData.telemetry.py);
            AddFloat(playerData.telemetry.pz);
            AddFloat(playerData.telemetry.qx);
            AddFloat(playerData.telemetry.qy);
            AddFloat(playerData.telemetry.qz);
            AddFloat(playerData.telemetry.qw);

        }

        protected override NetworkData ParseData(byte[] data)
        {
            PlayerTelemetry pt = new PlayerTelemetry();
            string name;
            int channel;
            int index = ReadInt(data, 0, out pt.pid);
            index = ReadInt(data, index, out channel);
            index = ReadString(data, index, out name);
            index = ReadFloat(data, index, out pt.px);
            index = ReadFloat(data, index, out pt.py);
            index = ReadFloat(data, index, out pt.pz);
            index = ReadFloat(data, index, out pt.qx);
            index = ReadFloat(data, index, out pt.qy);
            index = ReadFloat(data, index, out pt.qz);
            index = ReadFloat(data, index, out pt.qw);
            return new NetworkDataPlayerInfo(new PlayerData() { channel = channel, id = pt.pid, name = name, telemetry = pt});
        }
    }
}
