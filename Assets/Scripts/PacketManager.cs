using Google.Protobuf;
using Protocol;
using System;
using System.Collections.Generic;
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
    }
}


public class PacketManager : MonoBehaviour
{
    private readonly Dictionary<PacketType, IPacketHandler> packetHandlers = new Dictionary<PacketType, IPacketHandler>();

    public void OnRecv(byte[] buffer, int len)
    {
        OnPacketSetting();
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
    private void OnPacketSetting()
    {
        // 각 패킷 유형에 대한 처리기를 매핑
        packetHandlers.Add(PacketType.PKT_S_LOGIN, new S_LOGIN_Handler());

    }


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
}

