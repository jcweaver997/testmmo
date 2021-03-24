using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataVersion : NetworkData
    {
        public string version;

        public NetworkDataVersion() : base(0, NetworkCode.Version)
        {

        }

        public NetworkDataVersion(string version) : base(version.Length+4, NetworkCode.Version)
        {
            this.version = version;
        }

        protected override void Build()
        {
            AddString(version);
        }

        protected override NetworkData ParseData(byte[] data)
        {
            string ver;
            ReadString(data,0,out ver);
            return new NetworkDataVersion(ver);
        }
    }
}
