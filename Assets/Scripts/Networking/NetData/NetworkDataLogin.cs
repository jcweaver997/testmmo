using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    class NetworkDataLogin : NetworkData
    {
        public string username, password;
        public NetworkDataLogin(string username, string password): base(username.Length+password.Length+8,NetworkCode.Login)
        {

            this.username = username;
            this.password = password;
        }

        public NetworkDataLogin() : base (0,NetworkCode.Login){}

        protected override NetworkData ParseData(byte[] data)
        {
            string username, password;
            int index = 0;
            index = ReadString(data, index, out username);
            index = ReadString(data, index, out password);
            return new NetworkDataLogin(username,password);
        }

        protected override void Build()
        {
            AddString(username);
            AddString(password);
        }
    }
}
