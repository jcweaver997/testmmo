using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poopstory2_server.NetData
{
    public abstract class NetworkData
    {
        private byte[] bytes;
        private int index;
        private int packetSize;
        private NetworkCode code;
        public const int MAX_SIZE = 6000;

        public NetworkData(int packetSize, NetworkCode code)
        {
            this.packetSize = packetSize;
            this.code = code;
        }
        protected void AddInt(int i)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(i), 0, bytes, index, 4);
            index += 4;
        }

        protected void AddCode(NetworkCode c)
        {
            AddInt((int)c);
        }

        protected void AddString(string s)
        {
            byte[] chars = Encoding.UTF8.GetBytes(s);
            AddInt(chars.Length);
            Buffer.BlockCopy(chars, 0, bytes, index, chars.Length);
            index += chars.Length;
        }

        protected void AddFloat(float f)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(f), 0, bytes, index, 4);
            index += 4;
        }

        public byte[] GetBytes()
        {
            if (bytes == null)
            {
                index = 0;
                bytes = new byte[packetSize + 8];
                AddInt(packetSize + 8);
                AddCode(code);
                Build();
            }
            return bytes;
        }

        protected abstract NetworkData ParseData(byte[] data);
        protected abstract void Build();

        public static NetworkData Parse(byte[] data)
        {
            NetworkCode code = (NetworkCode)BitConverter.ToInt32(data, 4);
            NetworkData nd = null;
            Console.WriteLine($"code {code} val {BitConverter.ToInt32(data, 4)}");
            byte[] d = new byte[data.Length - 8];
            Buffer.BlockCopy(data, 8, d, 0, data.Length - 8);
            nd = NetworkDataTypes.GetT(code)?.ParseData(d);
            if (nd is null)
            {
                Console.WriteLine($"Uncategorized net code {code}");
            }
            return nd;
        }

        protected static int ReadString(byte[] bytes, int startIndex, out string val)
        {
            int length;
            startIndex = ReadInt(bytes,startIndex, out length);
            val = Encoding.UTF8.GetString(bytes,startIndex,length);
            return startIndex + length;
        }

        protected static int ReadInt(byte[] bytes, int startIndex, out int val)
        {
            val = BitConverter.ToInt32(bytes, startIndex);
            return startIndex + 4;
        }

        protected static int ReadFloat(byte[] bytes, int startIndex, out float val)
        {
            val = BitConverter.ToSingle(bytes, startIndex);
            return startIndex + 4;
        }

        protected static int ReadCode(byte[] bytes, int startIndex, out NetworkCode val)
        {
            int v;
            startIndex = ReadInt(bytes,startIndex,out v);
            val = (NetworkCode)v;
            return startIndex;
        }




    }
}
