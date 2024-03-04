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

    // 현재 플레이어를 가져오는 메서드
    public int GetCurrentisLogin()
    {
        return currentPlayer.isLogin;
    }

    // 해당 플레이어의 비행기가 이미 생성되어 있는지 여부를 확인하는 메서드
    public bool IsPlayerAlreadySpawned(int playerId)
    {
        return currentPlayer != null && currentPlayer.playerId == playerId;
    }

  


    public void UpdatePlayerPosition(Vector3 position) 
    {
        if (players.ContainsKey(currentPlayer.playerId) && currentPlayer.isLogin == 1)
        {
            Player player = players[currentPlayer.playerId];
            // 패킷 생성
            var playerPositionMessage = new C_POSITION()
            {
                PlayerId = (ulong)player.playerId,
                X = position.x,
                Y = position.y,
                Z = position.z
            };

            // 패킷 서버로 전송
            PacketManager.Instance.SendToServer(playerPositionMessage, PacketType.PKT_C_POSITION);
        }
        else if (currentPlayer.isLogin == 0)
        {

           Debug.LogWarning("현재 로그인 된 상태가 아닙니다.");
        }
        else
        {
            Debug.LogWarning("세션에 정상 등록이 되지 않았습니다");
        }
    }
}