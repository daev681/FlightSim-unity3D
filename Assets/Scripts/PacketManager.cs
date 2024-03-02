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
    }


    protected virtual void OnRecvPacket(ushort packetId, byte[] packetBody)
    {
        // ��Ŷ ID�� ���� ó���� �۾��� ����
        switch ((PacketType)packetId)
        {
            case PacketType.PKT_C_LOGIN:
                // PKT_C_LOGIN ��Ŷ ó��
                Debug.Log("Received PKT_C_LOGIN packet");
                break;
            case PacketType.PKT_S_LOGIN:
                // PKT_S_LOGIN ��Ŷ ó��
                Debug.Log("Received PKT_S_LOGIN packet");
                break;
            case PacketType.PKT_C_ENTER_GAME:
                // PKT_C_ENTER_GAME ��Ŷ ó��
                Debug.Log("Received PKT_C_ENTER_GAME packet");
                break;
            case PacketType.PKT_S_ENTER_GAME:
                // PKT_S_ENTER_GAME ��Ŷ ó��
                Debug.Log("Received PKT_S_ENTER_GAME packet");
                break;
            case PacketType.PKT_C_CHAT:
                // PKT_C_CHAT ��Ŷ ó��
                Debug.Log("Received PKT_C_CHAT packet");
                break;
            case PacketType.PKT_S_CHAT:
                // PKT_S_CHAT ��Ŷ ó��
                Debug.Log("Received PKT_S_CHAT packet");
                break;
            default:
                // ���ǵ��� ���� ��Ŷ ó��
                Debug.LogWarning("Received unknown packet with ID: " + packetId);
                break;
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

