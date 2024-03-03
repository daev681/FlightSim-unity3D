using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class PlayerManager
{
    private static PlayerManager instance;
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private PlayerManager() { }
    private Player currentPlayer;

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
            newPlayer.isLogin = 1;
            players.Add(playerId, newPlayer);
        }
        else
        {

        }
    }

    // 현재 플레이어를 설정하는 메서드
    public void SetCurrentPlayer(Player player)
    {
        player.isLogin = 1;
        currentPlayer = player;
    }

    // 현재 플레이어를 가져오는 메서드
    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void UpdatePlayerPosition(int playerId)
    {
        if (players.ContainsKey(playerId))
        {
            Player player = players[playerId];
            // 패킷 생성
            var playerPositionMessage = new C_POSITION()
            {
                PlayerId = (ulong)playerId,
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