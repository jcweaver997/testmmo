using poopstory2_server.NetTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataTelemetry : NetworkData
    {
        int pid;
        float px, py, pz, qx, qy, qz, qw;
        public PlayerTelemetry Telemetry { private set { } get {
                return new PlayerTelemetry(pid, px, py, pz, qx, qy, qz, qw);
            } }

        public NetworkDataTelemetry() : base(0, NetworkCode.Telemetry) { }

        public NetworkDataTelemetry(int pid,float px, float py, float pz, float qx, float qy, float qz, float qw) : base(8 * 4, NetworkCode.Telemetry)
        {
            this.pid = pid;
            this.px = px;
            this.py = py;
            this.pz = pz;
            this.qx = qx;
            this.qy = qy;
            this.qz = qz;
            this.qw = qw;
        }

        public NetworkDataTelemetry(PlayerTelemetry pt) : base(8 * 4, NetworkCode.Telemetry)
        {
            this.pid = pt.pid;
            this.px = pt.px;
            this.py = pt.py;
            this.pz = pt.pz;
            this.qx = pt.qx;
            this.qy = pt.qy;
            this.qz = pt.qz;
            this.qw = pt.qw;
        }

        protected override void Build()
        {
            AddInt(pid);
            AddFloat(px);
            AddFloat(py);
            AddFloat(pz);
            AddFloat(qx);
            AddFloat(qy);
            AddFloat(qz);
            AddFloat(qw);
        }

        protected override NetworkData ParseData(byte[] data)
        {
            float px, py, pz, qx, qy, qz, qw;
            int pid;
            int index = ReadInt(data, 0, out pid);
            index = ReadFloat(data, index, out px);
            index = ReadFloat(data, index, out py);
            index = ReadFloat(data, index, out pz);
            index = ReadFloat(data, index, out qx);
            index = ReadFloat(data, index, out qy);
            index = ReadFloat(data, index, out qz);
            index = ReadFloat(data, index, out qw);
            return new NetworkDataTelemetry(pid, px, py, pz, qx, qy, qz, qw);
        }


    }
}
