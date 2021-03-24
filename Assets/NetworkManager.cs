using poopstory2_server.NetData;
using poopstory2_server.NetTypes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public const string VERSION = "1.0";
    public GameObject character;
    public GameObject playerPrefab;
    public delegate void MessageReceivedEventHandler(NetworkData nd);
    public MessageReceivedEventHandler OnMessageReceived;
    public PlayerData myData;
    private const string ip = "127.0.0.1";
    private const int port = 6823;
    private Thread listenThread, mainThread;
    private TcpClient client;
    private ConcurrentQueue<NetworkData> messages;
    private WaitObject<NetworkData> waitMessage;

    private ConcurrentQueue<NetworkData> testMapQueue = new ConcurrentQueue<NetworkData>();
    

    // Start is called before the first frame update
    void Start()
    {
        messages = new ConcurrentQueue<NetworkData>();
        waitMessage = new WaitObject<NetworkData>();
        NetworkDataTypes.Initialize();
        mainThread = new Thread(Connect);
        mainThread.Start();
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        TestMap();
    }



    private void Connect()
    {
        client = new TcpClient();
        client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"),port));
        Debug.Log("Connected to Server");
        listenThread = new Thread(Listen);
        listenThread.Start();
        CheckVersion();
        Authenticate();
    }

    private void Queue(NetworkData nd)
    {
        if (nd == null) return;
        if (nd is NetworkDataKeepAlive keepAlive)
        {
            Send(new NetworkDataKeepAlive());
            return;
        }
        var s = OnMessageReceived;
        if (s != null)
        {
            s(nd);
        }
        if (waitMessage.Give(nd)) return;
        messages.Enqueue(nd);
    }

    private void Authenticate()
    {
        Send(new NetworkDataLogin("testu","testp"));
        var nd = ReadBlock();
        if (nd is NetworkDataError e)
        {
            Debug.Log("Got error "+ e.errMsg);
            Close();
        }else if (nd is NetworkDataChannelList cl)
        {
            SelectChannel(cl);
        }
        else
        {
            Debug.Log("bad channel list");
        }
    }

    private void SelectChannel(NetworkDataChannelList cl)
    {
        for (int i = 0; i < cl.channelInfo.Length; i++)
        {
            Debug.Log($"channel {i} {cl.channelInfo[i]}");
        }
        Debug.Log("Selecting channel 0");
        Send(new NetworkDataChannelSelect(0));
        NetworkData nd = ReadBlock();
        Debug.Log($"got {nd}");
        if (nd is NetworkDataPlayerInfo pi)
        {
            Joined(pi.playerData);
        }
        else
        {
            Debug.Log("error joining channel");
        }

    }

    private void GatherTelemetry()
    {
        Vector3 pos = gameObject.transform.position;
        Quaternion rot = gameObject.transform.rotation;
        myData.telemetry.px = pos.x;
        myData.telemetry.py = pos.y;
        myData.telemetry.pz = pos.z;
        myData.telemetry.qx = rot.x;
        myData.telemetry.qy = rot.y;
        myData.telemetry.qz = rot.z;
        myData.telemetry.qw = rot.w;
    }

    private void TestMap()
    {
        NetworkData nd;
        while (testMapQueue.TryDequeue(out nd))
        {
            if (nd is NetworkDataTelemetry tel)
            {
                var t = tel.Telemetry;
                if (t.pid == myData.id)
                {
                    GatherTelemetry();
                    Send(new NetworkDataTelemetry(myData.telemetry));
                    Debug.Log("sending my telemetry");
                }
                else if(t.pid == -1)
                {
                    myData.telemetry = t;
                    myData.telemetry.pid = myData.id;
                    character.transform.position = new Vector3(myData.telemetry.px, myData.telemetry.py, myData.telemetry.pz);
                    character.transform.rotation = new Quaternion(myData.telemetry.qx, myData.telemetry.qy, myData.telemetry.qz, myData.telemetry.qw);

                }
                else
                {
                    Debug.Log($"other info {t.pid} my id {myData.id}");
                }
            }
        }
    }

    private void TestMapQueue(NetworkData nd)
    {
        testMapQueue.Enqueue(nd);
    }

    private void Joined(PlayerData pd)
    {
        Debug.Log("joined");
        myData = pd;
        pd.telemetry.pid = -1;
        TestMapQueue(new NetworkDataTelemetry(pd.telemetry));
        OnMessageReceived += TestMapQueue;
    }

    private void CheckVersion()
    {
        Send(new NetworkDataVersion(VERSION));
        var nd = ReadBlock();
        if (nd is NetworkDataVersion v)
        {
            if (v.version == VERSION)
            {
                Debug.Log("version match");
                return;
            }
        }
        Close();
    }

    private void Listen()
    {
        byte[] buffer = new byte[6000];
        int index = 0;
        NetworkStream ns = client.GetStream();
        while (true)
        {
            try
            {
                int l = ns.Read(buffer, index, NetworkData.MAX_SIZE - index);
                index += l;
                if (l == 0)
                {
                    Close();
                    return;
                }
                
                if ((int)index > 4)
                {
                    int dataLength = BitConverter.ToInt32(buffer, 0);
                    //Debug.Log($"got some data, {index}/{dataLength}");
                    if (dataLength > NetworkData.MAX_SIZE) { Console.Error.WriteLine("data length longer than MAX_SIZE."); Close(); return; }
                    if ((int)index >= dataLength)
                    {
                        int extraBytes = (int)index - dataLength;
                        byte[] data = new byte[dataLength];
                        Buffer.BlockCopy(buffer, 0, data, 0, dataLength);
                        Buffer.BlockCopy(buffer, dataLength, buffer, 0, extraBytes);
                        index = 0;
                        Queue(NetworkData.Parse(data));
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); Close(); return; }
        }
    }

    public void Send(NetworkData nd)
    {
        byte[] bytes = nd.GetBytes();
        try
        {
            client.GetStream().Write(bytes, 0, bytes.Length);
        }
        catch (Exception) { Close(); };
    }

    public NetworkData ReadBlock()
    {
        NetworkData nd;
        if (messages.TryDequeue(out nd)) { Debug.Log(nd); return nd; }
        return waitMessage.Wait();
    }


    public void Close()
    {
        Debug.Log("closing");
        listenThread.Interrupt();
        client.Close();
    }

}
