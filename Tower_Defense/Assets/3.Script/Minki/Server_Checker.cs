
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using LitJson;
using System;
using System.IO;

public enum Type
{
    Empty = 0,
    Server,
    Client
}
public class Item
{
    public string License;
    public string Server_IP;
    public string Port;

    public Item(string L_index, string IPvalue, string port)
    {
        License = L_index;
        Server_IP = IPvalue;
        Port = port;
    }
}

public class Server_Checker : MonoBehaviour
{
    public static Server_Checker instance;

    public Type type;

    private NetworkManager manager;
    private KcpTransport kcp;

    private string Path = string.Empty;
    public string Server_Ip { get; private set; }
    public string Server_Port { get; private set; }

    [Header("자신의 아이피 넣어주기")]
    public string ServerIP = string.Empty;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        if (Path.Equals(string.Empty))
        {
            Path = Application.dataPath + "/License";
        }
        if (!File.Exists(Path)) //폴더 검사
        {
            Directory.CreateDirectory(Path);
        }
        if (!File.Exists(Path + "/License.json")) //파일 검사
        {
            Default_Data(Path);
        }
        manager = GetComponent<NetworkManager>();
        kcp = (KcpTransport)manager.transport;
        type = License_type();
    }
    private void Default_Data(string path)
    {

        List<Item> item = new List<Item>();
        item.Add(new Item("2", ServerIP, "7777"));

        JsonData data = JsonMapper.ToJson(item);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    private Type License_type()
    {
        Type type = Type.Empty;

        try
        {
            string Json_string = File.ReadAllText(Path + "/License.json");
            JsonData itemdata = JsonMapper.ToObject(Json_string);
            string string_type = itemdata[0]["License"].ToString();
            string str_serverIP = itemdata[0]["Server_IP"].ToString();
            string str_Port = itemdata[0]["Port"].ToString();
            Server_Ip = str_serverIP;
            Server_Port = str_Port;
            type = (Type)Enum.Parse(typeof(Type), string_type);

            manager.networkAddress = Server_Ip;
            kcp.port = ushort.Parse(Server_Port);

            return type;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return Type.Empty;
        }
    }

    private void Start()
    {
        if (type.Equals(Type.Empty))
        {
            Debug.Log("타입없음 오류");
            return;
        }

        if (type.Equals(Type.Server))
        {
            Start_Server();
        }
    }

    public void Start_Server()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("WebGL cannot be Server");
        }
        else
        {
            manager.StartServer();
            Debug.Log($"{manager.networkAddress} StartServer....");
            NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"New client Connect : {NetworkConnectionToClient.address}");
            };
            NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"client DisConnect : {NetworkConnectionToClient.address}");
            };
        }
    }


    public void Start_Client()
    {
        Room_Manager.singleton.StartClient();
        Debug.Log($"{manager.networkAddress} : Startclient...");
    }


    private void OnApplicationQuit()
    {
        if (NetworkClient.isConnected)
        {
            manager.StopClient();
        }

        if (NetworkServer.active)
        {
            manager.StopServer();
        }
    }


}
