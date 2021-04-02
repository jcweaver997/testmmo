using Assets.Scripts.Networking.NetUtil;
using poopstory2_server.NetData;
using poopstory2_server.NetTypes;
using poopstory2_server.NetUtil;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public const string VERSION = "1.0";
    public GameObject character;
    public GameObject playerPrefab;
    public PlayerData myData;
    public Dictionary<int, GameObject> players;
    private Locked<Queue<NetworkData>> messageHandlerQueue;
    private const string ip = "127.0.0.1";
    private const int port = 6823;
    private Task mainTask;
    private NetworkClient client;
    private delegate void HandleMessage(NetworkData nd);
    private Dictionary<Type, HandleMessage> messageHandlers;
    

    // Start is called before the first frame update
    void Start()
    {
        messageHandlers = new Dictionary<Type, HandleMessage>();
        messageHandlers.Add(typeof(NetworkDataTelemetry), HandleTelemetry);
        messageHandlers.Add(typeof(NetworkDataPlayerInfo), HandlePlayerInfo);
        messageHandlers.Add(typeof(NetworkDataPlayerLeft), HandlePlayerLeave);


        NetworkDataTypes.Initialize();
        messageHandlerQueue = new Locked<Queue<NetworkData>>(new Queue<NetworkData>());
        players = new Dictionary<int, GameObject>();
        mainTask = Task.Run(Connect);
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        MessageHandler();
    }



    private async Task Connect()
    {
        client = await NetworkClient.Connect(IPAddress.Parse("127.0.0.1"), port);
        if (client == null)
        {
            Console.WriteLine("connection timed out");
            return;
        }
        await CheckVersion();
        await Authenticate();
    }
    private async Task Authenticate()
    {
        await client.SendAsync(new NetworkDataLogin("testu","testp"));
        var nd = await client.ReadAsync();
        if (nd is NetworkDataError e)
        {
            Debug.Log("Got error "+ e.errMsg);
            Close();
        }else if (nd is NetworkDataChannelList cl)
        {
            await SelectChannel(cl);
        }
        else
        {
            Debug.Log("bad channel list");
        }
    }

    private async Task SelectChannel(NetworkDataChannelList cl)
    {
        for (int i = 0; i < cl.channelInfo.Length; i++)
        {
            Debug.Log($"channel {i} {cl.channelInfo[i]}");
        }
        Debug.Log("Selecting channel 0");
        await client.SendAsync(new NetworkDataChannelSelect(0));
        NetworkData nd = await client.ReadAsync();
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
        Vector3 pos = character.transform.position;
        Quaternion rot = character.transform.rotation;
        myData.telemetry.px = pos.x;
        myData.telemetry.py = pos.y;
        myData.telemetry.pz = pos.z;
        myData.telemetry.qx = rot.x;
        myData.telemetry.qy = rot.y;
        myData.telemetry.qz = rot.z;
        myData.telemetry.qw = rot.w;
    }

    private void MessageHandler()
    {
        using (var messageHandlerQueue = this.messageHandlerQueue.Wait())
        {
            NetworkData nd;

            while (messageHandlerQueue.Value.TryDequeue(out nd))
            {
                if (messageHandlers.ContainsKey(nd.GetType()))
                {
                    messageHandlers[nd.GetType()](nd);
                }
            }

        }
    }

    private void HandleTelemetry(NetworkData nd)
    {
        
        var tel = (NetworkDataTelemetry)nd;
        var t = tel.Telemetry;
        if (t.pid == myData.id)
        {
            GatherTelemetry();
            client.QueueSend(new NetworkDataTelemetry(myData.telemetry));
        }
        else if (t.pid == -1)
        {
            myData.telemetry = t;
            myData.telemetry.pid = myData.id;
            character.transform.position = new Vector3(myData.telemetry.px, myData.telemetry.py, myData.telemetry.pz);
            character.transform.rotation = new Quaternion(myData.telemetry.qx, myData.telemetry.qy, myData.telemetry.qz, myData.telemetry.qw);

        }
        else
        {
            if (players.ContainsKey(t.pid))
            {
                players[t.pid].GetComponent<NetInterpolator>().Set(new Vector3(t.px, t.py, t.pz), new Quaternion(t.qx, t.qy, t.qz, t.qw),1f/8f);
            }
            else
            {
                client.QueueSend(new NetworkDataPlayerInfoRequest(t.pid));
            }
        }
    }

    private void HandlePlayerInfo(NetworkData nd)
    {
        var pi = (NetworkDataPlayerInfo)nd;
        if (!players.ContainsKey(pi.playerData.id))
        {
            CreatePlayerAt(pi.playerData);
        }
    }

    private void HandlePlayerLeave(NetworkData nd)
    {
        var pl = (NetworkDataPlayerLeft)nd;
        if (players.ContainsKey(pl.pid))
        {
            GameObject.Destroy(players[pl.pid]);
            players.Remove(pl.pid);
        }
    }



    private void CreatePlayerAt(PlayerData t)
    {
        GameObject go = GameObject.Instantiate(playerPrefab);
        go.transform.position = new Vector3(t.telemetry.px, t.telemetry.py, t.telemetry.pz);
        go.transform.rotation = new Quaternion(t.telemetry.qx, t.telemetry.qy, t.telemetry.qz, t.telemetry.qw);
        players.Add(t.id,go);
    }

    private async Task TestMapQueue(NetworkClient nc, NetworkData nd, CancellationToken token)
    {
        using (var messageHandlerQueue = await this.messageHandlerQueue.WaitAsync()) {

            messageHandlerQueue.Value.Enqueue(nd);
        }
        
    }

    private void Joined(PlayerData pd)
    {
        Debug.Log("joined");
        myData = pd;
        pd.telemetry.pid = -1;
        players.Add(myData.id, character);
        Task.WaitAll(TestMapQueue(client, new NetworkDataTelemetry(pd.telemetry), CancellationToken.None));
        client.OnMessageReceived += TestMapQueue;
    }

    private async Task CheckVersion()
    {

        var nd = await client.ReadAsync();
        await client.SendAsync(new NetworkDataVersion(VERSION));
        if (nd is NetworkDataVersion v)
        {
            if (v.version == VERSION)
            {
                return;
            }
        }
        Close();
    }

    public void Close()
    {
        Debug.Log("left");
        client?.Close();
    }

}
