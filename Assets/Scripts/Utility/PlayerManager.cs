using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class PlayerManager
{
    private static PlayerManager instance;
    private static Dictionary<int, Player> players = new Dictionary<int, Player>();
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

    // 딕셔너리를 초기화하는 메서드
    public void ClearPlayers()
    {
        players.Clear();
    }

    public bool SyncPlayers(int playerId, Player player)
    {
        // 이미 존재하는 플레이어인지 확인
        if (players.ContainsKey(playerId))
        {
            // 이미 존재하는 플레이어인 경우 업데이트
            players[playerId] = player;
            return true;
        }
        else
        {
            // 존재하지 않는 플레이어인 경우 추가
            players.Add(playerId, player);
            return false;

        }
    }

    public bool isPlayerById(int playerId)
    {
        return players.ContainsKey(playerId);
    }

    // 현재 플레이어를 설정하는 메서드
    public void SetCurrentPlayer(Player player)
    {
        currentPlayer = player;
    }

    public void SetCurrentPlayerPosition(Vector3 position)
    {
        currentPlayer.CurrentPosition = position;
    }

    // 현재 플레이어를 설정하는 메서드
    public int GetCurrentPlayerId()
    {
        return currentPlayer.PlayerId;
    }

    // 현재 플레이어를 가져오는 메서드
    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }


    // 해당 플레이어의 비행기가 이미 생성되어 있는지 여부를 확인하는 메서드
    public bool IsPlayerAlreadySpawned()
    {
        return currentPlayer != null;
    }

    public void UpdatePlayerPosition(Vector3 position)
    {
        if (Instance.IsPlayerAlreadySpawned())
        {
            // 패킷 생성
            var playerPositionMessage = new C_POSITION()
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            };
            // 패킷 서버로 전송
            PacketManager.Instance.SendToServer(playerPositionMessage, PacketType.PKT_C_POSITION);
        }
    }

    public void UpdateOtherPlayerPosition(int playerId, Vector3 position)
    {
        if (players.ContainsKey(playerId))
        {
            Player player = players[playerId];
            // 패킷 생성
            var playerPositionMessage = new C_POSITION()
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            };

            // 패킷 서버로 전송
            PacketManager.Instance.SendToServer(playerPositionMessage, PacketType.PKT_C_POSITION);
        }
        else
        {
            Debug.LogWarning("플레이어가 존재하지 않습니다.");
        }
    }
}