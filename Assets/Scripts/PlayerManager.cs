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
        // 이미 존재하는 플레이어인지 확인하고 추가
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
            return null; // 플레이어가 존재하지 않을 경우 null 반환
        }
    }

}