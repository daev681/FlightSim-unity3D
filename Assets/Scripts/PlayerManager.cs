using System.Collections.Generic;
using UnityEngine;
using Protocol;

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



    public void AddPlayer(int playerId, string playerName)
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

    public void UpdatePlayerPosition(int playerId, GameObject playerObject)
    {
        if (players.ContainsKey(playerId))
        {
            Player player = players[playerId];
            player.SetPosition(playerObject);

            // 패킷 생성
            var playerPositionMessage = new C_POSITION()
            {
                PlayerId = 0,
                X = (ulong)player.CurrentPosition.x,
                Y = (ulong)player.CurrentPosition.y,
                Z = (ulong)player.CurrentPosition.z
            };

            // 패킷 서버로 전송
            PacketManager.Instance.SendToServer(playerPositionMessage, PacketType.PKT_C_POSITION);
        }
        else
        {
            // 해당 playerId를 가진 플레이어가 존재하지 않음을 알리는 메시지 출력 또는 다른 처리 수행
            // 예시: Debug.LogWarning("존재하지 않는 플레이어 ID입니다.");
        }
    }
}