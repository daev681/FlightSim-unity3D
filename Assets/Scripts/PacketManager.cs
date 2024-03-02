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
    private void OnPacketSetting()
    {
        // �� ��Ŷ ������ ���� ó���⸦ ����
        packetHandlers.Add(PacketType.PKT_S_LOGIN, new S_LOGIN_Handler());

    }


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
}

