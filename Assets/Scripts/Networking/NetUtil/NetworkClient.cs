using Assets.Scripts.Networking.NetUtil;
using poopstory2_server.NetData;
using poopstory2_server.NetUtil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace poopstory2_server.NetUtil
{


    public class NetworkClient
    {
        public delegate Task MessageReceivedHandler(NetworkClient nc, NetworkData nd, CancellationToken token);
        public delegate void NetworkClientEvent();
        public event NetworkClientEvent OnClientClose;
        public event MessageReceivedHandler OnMessageReceived;

        public TcpClient tcpClient;
        public Locked<Queue<NetworkData>> messageQueue;
        public Locked<Queue<NetworkData>> sendQueue;

        private Waiter waitMessage;
        private Task listenTask, keepAliveTask, sendTask;
        private NetworkStream ns;
        public CancellationTokenSource cancel;
        public NetworkClient(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            ns = tcpClient.GetStream();
            messageQueue = new Locked<Queue<NetworkData>>(new Queue<NetworkData>());
            sendQueue = new Locked<Queue<NetworkData>>(new Queue<NetworkData>());
            cancel = new CancellationTokenSource();
            waitMessage = new Waiter();
            cancel.Token.Register(FullClose);
            listenTask = Task.Run(Listen);
            keepAliveTask = Task.Run(KeepAlive);
            sendTask = Task.Run(SendLoop);
        }

        public static async Task<NetworkClient> Connect(IPAddress ip, int port)
        {
            TcpClient client = new TcpClient();
            Task connectTask = client.ConnectAsync(ip, port);
            Task timeoutTask = Task.Delay(15000);
            await Task.WhenAny(connectTask, timeoutTask);
            if (!connectTask.IsCompleted)
            {
                Console.WriteLine("connection timed out");
                return null;
            }
            else
            {
                return new NetworkClient(client);
            }
        }
        private async Task SendLoop()
        {
            byte[] b = new byte[6000];
            NetworkData nd;
            int index = 0;
            while (true)
            {
                await Task.Delay(100, cancel.Token);
                using (var l = await sendQueue.WaitAsync())
                {
                    while (l.Value.TryDequeue(out nd) && index < 64000)
                    {
                        byte[] buf = nd.GetBytes();
                        if (buf.Length>b.Length-index)
                        {
                            byte[] c = new byte[buf.Length*4+b.Length];
                            Buffer.BlockCopy(b,0,c,0,index);
                            b = c;
                        }
                        Buffer.BlockCopy(buf,0,b,index,buf.Length);
                        index += buf.Length;
                    }

                }

                if (index>0)
                {
                    try
                    {
                        await ns.WriteAsync(b, 0, index, cancel.Token);
                        index = 0;
                    }
                    catch (IOException) { Close(); };
                }
            }
        }

        private async Task Listen()
        {
            byte[] buffer = new byte[6000];
            int index = 0;
            while (true)
            {
                try
                {
                    int l = await ns.ReadAsync(buffer, index, NetworkData.MAX_SIZE - index);
                    index += l;
                    if (l == 0)
                    {
                        Close();
                        return;
                    }
                    if (index >4)
                    {
                        int dataLength = BitConverter.ToInt32(buffer,0);
                        if (dataLength > NetworkData.MAX_SIZE) { Console.Error.WriteLine("data length longer than MAX_SIZE."); Close(); return; }
                        while (index >= dataLength)
                        {
                            int extraBytes = index - dataLength;
                            byte[] data = new byte[dataLength];
                            Buffer.BlockCopy(buffer,0,data,0,dataLength);
                            Buffer.BlockCopy(buffer,dataLength,buffer,0,extraBytes);
                            index = extraBytes;
                            _ = Queue(NetworkData.Parse(data));
                            if (index > 4)
                            {
                                dataLength = BitConverter.ToInt32(buffer, 0);
                                if (dataLength > NetworkData.MAX_SIZE) { Console.Error.WriteLine("data length longer than MAX_SIZE."); Close(); return; }
                            }

                        }
                    }
                }
                catch (IOException) { Close(); return; }
            }
        }

        private async Task KeepAlive()
        {
            while (true)
            {
                await Task.Delay(5000, cancel.Token);
                await SendAsync(new NetworkDataKeepAlive());
            }
        }


        private async Task Queue(NetworkData nd)
        {
            if (nd == null) return;
            if(nd is NetworkDataKeepAlive keepAlive)
            {
                return;
            }

            using (var messages = await messageQueue.WaitAsync())
            {
                if (waitMessage.IsWaiting())
                {
                    messages.Value.Enqueue(nd);
                    waitMessage.Signal();
                }
                else
                {
                    var onMessageReceived = OnMessageReceived;
                    if (onMessageReceived != null)
                    {
                        _ = onMessageReceived(this, nd, cancel.Token);
                    }
                    else
                    {
                        messages.Value.Enqueue(nd);
                    }
                }

            }
        }

        public async Task<NetworkData> ReadAsync()
        {
            NetworkData nd;

            while (true)
            {
                using (var l = await messageQueue.WaitAsync())
                {
                    if (l.Value.TryDequeue(out nd)){return nd;}
                }
                await waitMessage.WaitAsync(cancel.Token);
            }
            
        }

        public async Task SendAsync(NetworkData nd)
        {
            if (nd == null) { return; }

            byte[] b = nd.GetBytes();
            try
            {
                await ns.WriteAsync(b, 0, b.Length, cancel.Token);
            }
            catch (IOException) { Close(); };
        }

        public void QueueSend(NetworkData nd)
        {
            if (nd == null) { return; }
            using (var l = sendQueue.Wait())
            {
                l.Value.Enqueue(nd);

            }
        }

        public async Task QueueSendAsync(NetworkData nd)
        {
            if (nd == null) { return; }
            using (var l = await sendQueue.WaitAsync())
            {
                l.Value.Enqueue(nd);

            }
        }

        public void Close()
        {
            cancel.CancelAfter(1);
        }

        private void FullClose()
        {
            var s = OnClientClose;
            if (s != null)
            {
                s();
            }
        }
    }
}
