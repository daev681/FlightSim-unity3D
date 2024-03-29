using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Protocol;
using PimDeWitte.UnityMainThreadDispatcher;
public class NetworkManager
{
    private static NetworkManager instance;
    private Socket serverSocket { get; set; }
    private bool serverConnected { get; set; }

    private NetworkManager() { }

    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NetworkManager();

            }
            return instance;
        }
    }


    public Socket getServerSocket()
    {
        return serverSocket;
    }



    public void ConnectToServer()
    {
        if (serverConnected)
        {
            Debug.LogWarning("Already connected to server.");
            return;
        }

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        IPEndPoint serverEndPoint = new IPEndPoint(serverIP, 7777);

        try
        {
            serverSocket.BeginConnect(serverEndPoint, ConnectCallback, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to server: {e.Message}");
        }
    }

    private void ConnectCallback(IAsyncResult result)
    {
        try
        {
            serverSocket.EndConnect(result);
            Debug.Log("Connected to server");
            serverConnected = true;
            // 로그인 요청 보내기
            for (int i = 0; i < 1; i++)
            {
                var loginMessage = new C_LOGIN();
                PacketManager.Instance.SendToServer(loginMessage, PacketType.PKT_C_LOGIN);
            }

            StartReceive();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to server: {e.Message}");
        }
    }

    private void StartReceive()
    {
        if (!serverConnected)
        {
            Debug.LogWarning("Cannot start receiving data. Server is not connected.");
            return;
        }

        byte[] buffer = new byte[1024]; 
        serverSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, buffer);
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int bytesRead = serverSocket.EndReceive(result);

            if (bytesRead > 0)
            {
                byte[] receivedData = (byte[])result.AsyncState;
           
                PacketManager.Instance.OnRecv(receivedData, bytesRead);
                serverSocket.BeginReceive(receivedData, 0, receivedData.Length, SocketFlags.None, ReceiveCallback, receivedData);

            }
            else
            {
                Debug.Log("Connection closed or no data received from server.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to receive data: {e.Message}");
        }
    }

    public void OnDestroy(int playerId)
    {
        if (serverSocket != null && serverSocket.Connected && PlayerManager.Instance.IsMainPlayer(playerId))
        {
            
            var loginMessage = new C_DESTROY();
            PacketManager.Instance.SendToServer(loginMessage, PacketType.PKT_C_DESTORY);
            PlayerManager.Instance.ClearPlayers();
            serverSocket.Shutdown(SocketShutdown.Both);
            serverSocket.Close();

        }
    }
}