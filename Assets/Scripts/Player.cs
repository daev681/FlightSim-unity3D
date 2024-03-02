public class Player
{
    public int PlayerId { get; private set; }
    public string PlayerName { get; private set; }
    public bool IsLoggedIn { get; set; }

    public Player(int playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        IsLoggedIn = false; // �÷��̾ �ʱ⿡ �α��ε��� ���� ���·� ����
    }
}