public class Player
{
    public int PlayerId { get; private set; }
    public string PlayerName { get; private set; }
    public bool IsLoggedIn { get; set; }

    public Player(int playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        IsLoggedIn = false; // 플레이어가 초기에 로그인되지 않은 상태로 설정
    }
}