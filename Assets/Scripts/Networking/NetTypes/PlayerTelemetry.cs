using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetTypes
{
    public struct PlayerTelemetry
    {
        public int pid;
        public float px, py, pz, qx, qy, qz, qw;
        public PlayerTelemetry(int pid, float px, float py, float pz, float qx, float qy, float qz, float qw)
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

        public override string ToString()
        {
            return $"player {pid} loc [{px},{py},{pz}] rot [{qx},{qy},{qz},{qw}]";
        }
    }
}
