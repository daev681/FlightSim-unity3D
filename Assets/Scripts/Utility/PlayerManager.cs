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

    // ��ųʸ��� �ʱ�ȭ�ϴ� �޼���
    public void ClearPlayers()
    {
        players.Clear();
    }

    public bool SyncPlayers(int playerId, Player player)
    {
        // �̹� �����ϴ� �÷��̾����� Ȯ��
        if (players.ContainsKey(playerId))
        {
            // �̹� �����ϴ� �÷��̾��� ��� ������Ʈ
            players[playerId] = player;
            return true;
        }
        else
        {
            // �������� �ʴ� �÷��̾��� ��� �߰�
            players.Add(playerId, player);
            return false;

        }
    }

    public bool DeletePlayer(int playerId)
    {
        // �÷��̾ �ִ��� Ȯ���ϰ� �ִٸ� ����
        if (players.ContainsKey(playerId))
        {
            players.Remove(playerId);
            var messasge = new C_CHAT()
            {
                Msg = playerId + " ���� �ı��Ǿ����ϴ�",
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

    // �÷��̾ ��ü �÷��̾����� ���θ� Ȯ���ϴ� �޼���
    public bool IsMainPlayer(int playerId)
    {
        Player player;
        if (players.TryGetValue(playerId, out player))
        {
            return player.IsMainPlayer;
        }
        return false; // �ش� playerId�� �ش��ϴ� �÷��̾ ���� ���
    }


    public bool isPlayerById(int playerId)
    {
        return players.ContainsKey(playerId);
    }


    public void UpdatePlayerPosition(int playerId, Vector3 position)
    {

        if (Instance.IsExistPlayer(playerId) && IsMainPlayer(playerId))
        {
            // ��Ŷ ����
            var playerPositionMessage = new C_POSITION()
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            };
            // ��Ŷ ������ ����

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