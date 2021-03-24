using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetTypes
{
    public struct PlayerData
    {
        public string name;
        public int id;
        public int channel;
        public PlayerTelemetry telemetry;
    }
}
