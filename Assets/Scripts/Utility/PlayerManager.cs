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

    public bool isPlayerById(int playerId)
    {
        return players.ContainsKey(playerId);
    }

    // ���� �÷��̾ �����ϴ� �޼���
    public void SetCurrentPlayer(Player player)
    {
        currentPlayer = player;
    }

    public void SetCurrentPlayerPosition(Vector3 position)
    {
        currentPlayer.CurrentPosition = position;
    }

    // ���� �÷��̾ �����ϴ� �޼���
    public int GetCurrentPlayerId()
    {
        return currentPlayer.PlayerId;
    }

    // ���� �÷��̾ �������� �޼���
    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }


    // �ش� �÷��̾��� ����Ⱑ �̹� �����Ǿ� �ִ��� ���θ� Ȯ���ϴ� �޼���
    public bool IsPlayerAlreadySpawned()
    {
        return currentPlayer != null;
    }

    public void UpdatePlayerPosition(Vector3 position)
    {
        if (Instance.IsPlayerAlreadySpawned())
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
    }

    public void UpdateOtherPlayerPosition(int playerId, Vector3 position)
    {
        if (players.ContainsKey(playerId))
        {
            Player player = players[playerId];
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
            Debug.LogWarning("�÷��̾ �������� �ʽ��ϴ�.");
        }
    }
}