using Google.Protobuf;
using System;
using UnityEngine;

public class PacketManager : MonoBehaviour
{
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
    }


    protected virtual void OnRecvPacket(ushort packetId, byte[] packetBody)
    {
        // 패킷 ID에 따라서 처리할 작업을 수행
        switch ((PacketType)packetId)
        {
            case PacketType.PKT_C_LOGIN:
                // PKT_C_LOGIN 패킷 처리
                Debug.Log("Received PKT_C_LOGIN packet");
                break;
            case PacketType.PKT_S_LOGIN:
                // PKT_S_LOGIN 패킷 처리
                Debug.Log("Received PKT_S_LOGIN packet");
                break;
            case PacketType.PKT_C_ENTER_GAME:
                // PKT_C_ENTER_GAME 패킷 처리
                Debug.Log("Received PKT_C_ENTER_GAME packet");
                break;
            case PacketType.PKT_S_ENTER_GAME:
                // PKT_S_ENTER_GAME 패킷 처리
                Debug.Log("Received PKT_S_ENTER_GAME packet");
                break;
            case PacketType.PKT_C_CHAT:
                // PKT_C_CHAT 패킷 처리
                Debug.Log("Received PKT_C_CHAT packet");
                break;
            case PacketType.PKT_S_CHAT:
                // PKT_S_CHAT 패킷 처리
                Debug.Log("Received PKT_S_CHAT packet");
                break;
            default:
                // 정의되지 않은 패킷 처리
                Debug.LogWarning("Received unknown packet with ID: " + packetId);
                break;
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
}

