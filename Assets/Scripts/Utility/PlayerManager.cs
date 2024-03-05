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

    public bool DeletePlayer(int playerId)
    {
        // 플레이어가 있는지 확인하고 있다면 삭제
        if (players.ContainsKey(playerId))
        {
            players.Remove(playerId);
            var messasge = new C_CHAT()
            {
                Msg = playerId + " 님이 파괴되었습니다",
            };
            PacketManager.Instance.SendToServer(messasge, PacketType.PKT_C_CHAT);
            return true;
        }
        else
        {

            return false;

        }
    }

    public bool IsExistPlayer(int playerId)
    {

        return players.ContainsKey(playerId);
    }

    // 플레이어가 주체 플레이어인지 여부를 확인하는 메서드
    public bool IsMainPlayer(int playerId)
    {
        Player player;
        if (players.TryGetValue(playerId, out player))
        {
            return player.IsMainPlayer;
        }
        return false; // 해당 playerId에 해당하는 플레이어가 없는 경우
    }


    public bool isPlayerById(int playerId)
    {
        return players.ContainsKey(playerId);
    }


    public void UpdatePlayerPosition(int playerId, Vector3 position)
    {

        if (Instance.IsExistPlayer(playerId) && IsMainPlayer(playerId))
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
        else
        {

        }
    }

    public void SyncFireMissile(Vector3 position, Quaternion rotation)
    {
            var FireMessage = new C_MISSILE()
            {
                Px = position.x,
                Py = position.y,
                Pz = position.z,
                Rx = rotation.x,
                Ry = rotation.y,
                Rz = rotation.z,
            };
            PacketManager.Instance.SendToServer(FireMessage, PacketType.PKT_C_MISSILE);
        }
  
}