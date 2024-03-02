using System.Collections.Generic;

public class PlayerManager
{
    private static PlayerManager instance;
    private Dictionary<int, Player> players = new Dictionary<int, Player>();

    private PlayerManager() { }

    public static PlayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PlayerManager();
            }
            return instance;
        }
    }

    public void AddPlayer(int playerId , string playerName)
    {
        // �̹� �����ϴ� �÷��̾����� Ȯ���ϰ� �߰�
        if (!players.ContainsKey(playerId))
        {
            Player newPlayer = new Player(playerId, playerName);
            players.Add(playerId, newPlayer);
        }
        else
        {
    
        }
    }

    public Player GetPlayer(int playerId)
    {
        if (players.ContainsKey(playerId))
        {
            return players[playerId];
        }
        else
        {
            return null; // �÷��̾ �������� ���� ��� null ��ȯ
        }
    }

}