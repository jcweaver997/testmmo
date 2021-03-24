using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataError : NetworkData
    {
        public string errMsg;
        public NetworkDataError(string errorText) : base(errorText.Length + 4, NetworkCode.Error)
        {
            this.errMsg = errorText;
        }

        public NetworkDataError() : base(0, NetworkCode.Error)
        {

        }

        protected override void Build()
        {
            AddString(errMsg);
        }

        protected override NetworkData ParseData(byte[] data)
        {
            string s;
            ReadString(data,0,out s);
            return new NetworkDataError(s);
        }
    }
}
