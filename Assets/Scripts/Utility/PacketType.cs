// PacketType.cs ����

public enum PacketType : ushort
{
    PKT_C_LOGIN = 1000,
    PKT_S_LOGIN = 1001,
    PKT_C_ENTER_GAME = 1002,
    PKT_S_ENTER_GAME = 1003,
    PKT_C_CHAT = 1004,
    PKT_S_CHAT = 1005,
    PKT_C_POSITION = 1006,
    PKT_S_POSITION = 1007,
    PKT_C_MISSILE = 1008,
    PKT_S_MISSILE = 1009,
    PKT_C_DESTORY = 1010,
    PKT_S_DESTORY = 1011,
}