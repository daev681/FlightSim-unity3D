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
              
                // 모든 플레이어를 추가한 후에 현재 플레이어 설정
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
        // 각 패킷 유형에 대한 처리기를 매핑
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
            // 최소한 헤더는 파싱할 수 있어야 함
            if (dataSize < sizeof(ushort) * 2)
                break;

            // 헤더 파싱
            ushort packetSize = BitConverter.ToUInt16(buffer, processLen);
            ushort packetId = BitConverter.ToUInt16(buffer, processLen + sizeof(ushort));

            // 헤더에 기록된 패킷 크기를 파싱할 수 있어야 함
            if (dataSize < packetSize)
                break;

            // 바디 파싱
            byte[] packetBody = new byte[packetSize - sizeof(ushort) * 2];
            Buffer.BlockCopy(buffer, processLen + sizeof(ushort) * 2, packetBody, 0, packetBody.Length);

            // 패킷 조립 성공
            OnRecvPacket(packetId, packetBody);

            processLen += packetSize;
        }
    } // 초기화
   

    protected virtual void OnRecvPacket(ushort packetId, byte[] packetBody)
    {

        // 패킷 ID에 해당하는 처리기를 찾아서 실행
        if (packetHandlers.ContainsKey((PacketType)packetId))
        {
            packetHandlers[(PacketType)packetId].HandlePacket(packetBody);
        }
        else
        {
            Debug.LogWarning("Received unknown packet with ID: " + packetId);
        }
    }

    // 패킷을 직렬화하여 전송하기 위한 함수
    public byte[] SerializeWithHeader(IMessage message, PacketType packetId)
    {
        // 메시지를 직렬화하여 데이터를 얻음
        byte[] messageData = message.ToByteArray();

        // 헤더를 생성하고 헤더와 메시지 데이터의 길이를 계산
        PacketHeader header = new PacketHeader
        {
            size = (ushort)(sizeof(ushort) * 2 + messageData.Length), // 패킷 길이 = 헤더 길이(ushort 2개) + 메시지 데이터 길이
            id = (ushort)packetId
        };

        // 헤더와 메시지 데이터를 합쳐서 패킷을 생성
        byte[] packet = new byte[sizeof(ushort) * 2 + messageData.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(header.size), 0, packet, 0, sizeof(ushort));
        Buffer.BlockCopy(BitConverter.GetBytes(header.id), 0, packet, sizeof(ushort), sizeof(ushort));
        Buffer.BlockCopy(messageData, 0, packet, sizeof(ushort) * 2, messageData.Length);

        return packet;
    }

    // 패킷을 서버로 전송하는 메서드
    public void SendToServer(IMessage message, PacketType type)
    {
        // 메시지를 직렬화하여 패킷에 헤더를 추가하고 서버로 전송
        byte[] serializedData = SerializeWithHeader(message, type);
        SendMessage(serializedData);
    }

    // 서버로 메시지를 전송하는 메서드
    private void SendMessage(byte[] data)
    {
        try
        {
            // 비동기 방식으로 데이터를 보냅니다.
            NetworkManager.Instance.getServerSocket().BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send data to server: {e.Message}");
        }
    }


    // 메시지 전송이 완료된 후 호출되는 콜백 메서드
    private void SendCallback(IAsyncResult result)
    {
        try
        {
            // 비동기 작업을 완료하고 전송된 바이트 수를 반환
            int sentBytes = NetworkManager.Instance.getServerSocket().EndSend(result);
            Debug.Log($"Sent {sentBytes} bytes to server");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send data to server: {e.Message}");
        }
    }
}

