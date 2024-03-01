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
            // 최소한 헤더는 파싱할 수 있어야 함
            if (dataSize < sizeof(ushort) * 2)
                break;

            PacketHeader header = new PacketHeader();
            header.size = BitConverter.ToUInt16(buffer, processLen);
            header.id = BitConverter.ToUInt16(buffer, processLen + sizeof(ushort));

            // 헤더에 기록된 패킷 크기를 파싱할 수 있어야 함
            if (dataSize < header.size)
                break;

            // 패킷 조립 성공
            OnRecvPacket(buffer, processLen, header.size);

            processLen += header.size;
        }
    }

    protected virtual void OnRecvPacket(byte[] buffer, int offset, int len)
    {
        // 구현 로직 작성
        Debug.Log("Received packet data: " + System.Text.Encoding.UTF8.GetString(buffer, offset + sizeof(ushort) * 2, len - sizeof(ushort) * 2));
    }
}

