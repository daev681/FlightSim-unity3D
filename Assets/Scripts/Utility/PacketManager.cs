using Google.Protobuf;
using PimDeWitte.UnityMainThreadDispatcher;
using Protocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public interface IPacketHandler
{
    void HandlePacket(byte[] packetBody);
}

public class S_LOGIN_Handler : IPacketHandler
{
    public void HandlePacket(byte[] packetBody)
    {
        S_LOGIN loginPacket = S_LOGIN.Parser.ParseFrom(packetBody);
        Debug.Log("Received PKT_S_LOGIN packet: " + loginPacket.ToString());
        if (loginPacket.Success)
        {
            foreach (var player in loginPacket.Players)
            {
                // PlayerManager.Instance.SyncPlayerInfo((int)player.Id, player.Name);
              
                // ��� �÷��̾ �߰��� �Ŀ� ���� �÷��̾� ����
                if (loginPacket.Players.Count > 0)
                {
                    var me = loginPacket.Players[0];
                    if (!PlayerManager.Instance.IsExistPlayer((int)me.Id))
                    {
                        Vector3 currentPosition = new Vector3(me.X, me.Y, me.Z);
                        Player myPlayer = new Player();
                        myPlayer.CurrentPosition = currentPosition;
                        myPlayer.PlayerId = (int)me.Id;
                        myPlayer.PlayerName = me.Name;
                        myPlayer.IsMainPlayer = true;
                        //PlayerManager.Instance.SetCurrentPlayer(myPlayer);
                        PlayerManager.Instance.SyncPlayers((int)me.Id, myPlayer);              
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            GameObject playerObject = GameObject.Find("F15");
                            playerObject.name = me.Id.ToString();
                        });
                        var enterMessage = new C_ENTER_GAME() { Playerindex = 0 };
                        PacketManager.Instance.SendToServer(enterMessage, PacketType.PKT_C_ENTER_GAME);
                    }
                }
            }
        }
    
    }
}

public class S_ENTER_GAME_Handler : IPacketHandler
{
    public void HandlePacket(byte[] packetBody)
    {

        S_ENTER_GAME enterPacket = S_ENTER_GAME.Parser.ParseFrom(packetBody);
        Debug.Log("Received S_ENTER_GAME_Handler packet: " + enterPacket.ToString());
     
        if (enterPacket.Success)
        {

            foreach (var player in enterPacket.CurrentAllplayers)
            {
                if (!PlayerManager.Instance.IsExistPlayer((int)player.Id)) 
                {
                    Vector3 currentPosition = new Vector3(player.X, player.Y, player.Z);
                    Player myPlayer = Player.Instance;
                    myPlayer.CurrentPosition = currentPosition;
                    myPlayer.PlayerId = (int)player.Id;
                    myPlayer.IsMainPlayer = false;
                    bool isSync = PlayerManager.Instance.SyncPlayers((int)player.Id, myPlayer);
                    if (isSync)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            GameObject playerObject = GameObject.Find(player.Id.ToString());
                            if (playerObject != null)
                            {

                                playerObject.transform.position = currentPosition;
                            }
                            else
                            {
                                Debug.LogWarning("Player object not found for player ID: " + player.Id.ToString());
                            }
                        });
                    }
                    else
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            Plane.Instance.SpawnF15((int)player.Id, currentPosition);
                        });
                    }
                }
            }
        }
    }
}

public class S_POSITION_Handler : IPacketHandler
{
    public void HandlePacket(byte[] packetBody)
    {
        S_POSITION positionPacket = S_POSITION.Parser.ParseFrom(packetBody);
        Debug.Log("Received S_POSITION_Handler packet: " + positionPacket.ToString());

        foreach (var player in positionPacket.CurrentAllplayers)
        {
            if (PlayerManager.Instance.IsExistPlayer((int)player.Id) && PlayerManager.Instance.IsMainPlayer((int)player.Id))
            {
                Vector3 currentPosition = new Vector3(player.X, player.Y, player.Z);
                Player myPlayer = Player.Instance;
                myPlayer.CurrentPosition = currentPosition;
                myPlayer.PlayerId = (int)player.Id;
                myPlayer.IsMainPlayer = false;
                bool isSync = PlayerManager.Instance.SyncPlayers((int)player.Id, myPlayer);
                if (isSync)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        GameObject playerObject = GameObject.Find(player.Id.ToString());
                        if (playerObject != null)
                        {

                            playerObject.transform.position = currentPosition;
                        }
                        else
                        {
                            Debug.LogWarning("Player object not found for player ID: " + player.Id.ToString());
                        }
                    });
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        Plane.Instance.SpawnF15((int)player.Id, currentPosition);
                    });
                }
            }
        }
    }
}


public class PacketManager
{
    private readonly Dictionary<PacketType, IPacketHandler> packetHandlers = new Dictionary<PacketType, IPacketHandler>();
    private static PacketManager instance;

    private void OnPacketSetting()
    {
        // �� ��Ŷ ������ ���� ó���⸦ ����
        packetHandlers.Add(PacketType.PKT_S_LOGIN, new S_LOGIN_Handler());
        packetHandlers.Add(PacketType.PKT_S_ENTER_GAME, new S_ENTER_GAME_Handler());
        packetHandlers.Add(PacketType.PKT_S_POSITION, new S_POSITION_Handler());
    }


    public static PacketManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PacketManager();
                instance.OnPacketSetting();
            }
            return instance;
        }
    }


    public void OnRecv(byte[] buffer, int len)
    {
   
        int processLen = 0;
        while (true)
        {
            int dataSize = len - processLen;
            // �ּ��� ����� �Ľ��� �� �־�� ��
            if (dataSize < sizeof(ushort) * 2)
                break;

            // ��� �Ľ�
            ushort packetSize = BitConverter.ToUInt16(buffer, processLen);
            ushort packetId = BitConverter.ToUInt16(buffer, processLen + sizeof(ushort));

            // ����� ��ϵ� ��Ŷ ũ�⸦ �Ľ��� �� �־�� ��
            if (dataSize < packetSize)
                break;

            // �ٵ� �Ľ�
            byte[] packetBody = new byte[packetSize - sizeof(ushort) * 2];
            Buffer.BlockCopy(buffer, processLen + sizeof(ushort) * 2, packetBody, 0, packetBody.Length);

            // ��Ŷ ���� ����
            OnRecvPacket(packetId, packetBody);

            processLen += packetSize;
        }
    } // �ʱ�ȭ
   

    protected virtual void OnRecvPacket(ushort packetId, byte[] packetBody)
    {

        // ��Ŷ ID�� �ش��ϴ� ó���⸦ ã�Ƽ� ����
        if (packetHandlers.ContainsKey((PacketType)packetId))
        {
            packetHandlers[(PacketType)packetId].HandlePacket(packetBody);
        }
        else
        {
            Debug.LogWarning("Received unknown packet with ID: " + packetId);
        }
    }

    // ��Ŷ�� ����ȭ�Ͽ� �����ϱ� ���� �Լ�
    public byte[] SerializeWithHeader(IMessage message, PacketType packetId)
    {
        // �޽����� ����ȭ�Ͽ� �����͸� ����
        byte[] messageData = message.ToByteArray();

        // ����� �����ϰ� ����� �޽��� �������� ���̸� ���
        PacketHeader header = new PacketHeader
        {
            size = (ushort)(sizeof(ushort) * 2 + messageData.Length), // ��Ŷ ���� = ��� ����(ushort 2��) + �޽��� ������ ����
            id = (ushort)packetId
        };

        // ����� �޽��� �����͸� ���ļ� ��Ŷ�� ����
        byte[] packet = new byte[sizeof(ushort) * 2 + messageData.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(header.size), 0, packet, 0, sizeof(ushort));
        Buffer.BlockCopy(BitConverter.GetBytes(header.id), 0, packet, sizeof(ushort), sizeof(ushort));
        Buffer.BlockCopy(messageData, 0, packet, sizeof(ushort) * 2, messageData.Length);

        return packet;
    }

    // ��Ŷ�� ������ �����ϴ� �޼���
    public void SendToServer(IMessage message, PacketType type)
    {
        // �޽����� ����ȭ�Ͽ� ��Ŷ�� ����� �߰��ϰ� ������ ����
        byte[] serializedData = SerializeWithHeader(message, type);
        SendMessage(serializedData);
    }

    // ������ �޽����� �����ϴ� �޼���
    private void SendMessage(byte[] data)
    {
        try
        {
            // �񵿱� ������� �����͸� �����ϴ�.
            NetworkManager.Instance.getServerSocket().BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send data to server: {e.Message}");
        }
    }


    // �޽��� ������ �Ϸ�� �� ȣ��Ǵ� �ݹ� �޼���
    private void SendCallback(IAsyncResult result)
    {
        try
        {
            // �񵿱� �۾��� �Ϸ��ϰ� ���۵� ����Ʈ ���� ��ȯ
            int sentBytes = NetworkManager.Instance.getServerSocket().EndSend(result);
            Debug.Log($"Sent {sentBytes} bytes to server");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send data to server: {e.Message}");
        }
    }
}

