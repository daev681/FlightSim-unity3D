using System;
using UnityEngine;

public class PacketSession : MonoBehaviour
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

            PacketHeader header = new PacketHeader();
            header.size = BitConverter.ToUInt16(buffer, processLen);
            header.id = BitConverter.ToUInt16(buffer, processLen + sizeof(ushort));

            // ����� ��ϵ� ��Ŷ ũ�⸦ �Ľ��� �� �־�� ��
            if (dataSize < header.size)
                break;

            // ��Ŷ ���� ����
            OnRecvPacket(buffer, processLen, header.size);

            processLen += header.size;
        }
    }

    protected virtual void OnRecvPacket(byte[] buffer, int offset, int len)
    {
        // ���� ���� �ۼ�
        Debug.Log("Received packet data: " + System.Text.Encoding.UTF8.GetString(buffer, offset + sizeof(ushort) * 2, len - sizeof(ushort) * 2));
    }
}

