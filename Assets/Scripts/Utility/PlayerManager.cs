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




    public void AddPlayer(int playerId, Vector3 currentPosition)
    {
        // �̹� �����ϴ� �÷��̾����� Ȯ���ϰ� �߰�
        if (!players.ContainsKey(playerId))
        {
            Player newPlayer = new Player(playerId, currentPosition);
            players.Add(playerId, newPlayer);
        }
        else
        {

        }
    }

    public bool isPlayerById(int playerId)
    {
        if (players.ContainsKey(playerId))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // ���� �÷��̾ �����ϴ� �޼���
    public void SetCurrentPlayer(Player player)
    {
        currentPlayer = player;
    }

    public void SetCurrentPlayerPosition(Vector3 transform)
    {
        currentPlayer.CurrentPosition = transform;
    }

    // ���� �÷��̾ �����ϴ� �޼���
    public int GetCurrentPlayerId()
    {

        return currentPlayer.playerId;
    }

    // ���� �÷��̾ �������� �޼���
    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    // ���� �÷��̾ �������� �޼���
    public int GetCurrentisLogin()
    {
        return currentPlayer.isLogin;
    }

    // �ش� �÷��̾��� ����Ⱑ �̹� �����Ǿ� �ִ��� ���θ� Ȯ���ϴ� �޼���
    public bool IsPlayerAlreadySpawned()
    {
        return currentPlayer == null ? false : true;
    }



    public void UpdatePlayerPosition(Vector3 position) 
    {
        if (players.ContainsKey(currentPlayer.playerId) && Instance.IsPlayerAlreadySpawned())
        {
            Instance.SetCurrentPlayerPosition(position);
            Player player = players[currentPlayer.playerId];
            // ��Ŷ ����
            var playerPositionMessage = new C_POSITION()
            {
                PlayerId = (ulong)player.playerId,
                X = position.x,
                Y = position.y,
                Z = position.z
            };

            // ��Ŷ ������ ����
            PacketManager.Instance.SendToServer(playerPositionMessage, PacketType.PKT_C_POSITION);
        }
        else if (currentPlayer.isLogin == 0)
        {

           Debug.LogWarning("���� �α��� �� ���°� �ƴմϴ�.");
        }
        else
        {
            Debug.LogWarning("���ǿ� ���� ����� ���� �ʾҽ��ϴ�");
        }
    }

    public void UpdateOtherPlayerPosition(int PlayerId, Vector3 position)
    {
        if (players.ContainsKey(PlayerId))
        {
            Player player = players[PlayerId];
            // ��Ŷ ����
            var playerPositionMessage = new C_POSITION()
            {
                PlayerId = (ulong)player.playerId,
                X = position.x,
                Y = position.y,
                Z = position.z
            };

            // ��Ŷ ������ ����
            PacketManager.Instance.SendToServer(playerPositionMessage, PacketType.PKT_C_POSITION);
        }
        else if (currentPlayer.isLogin == 0)
        {

            Debug.LogWarning("���� �α��� �� ���°� �ƴմϴ�.");
        }
        else
        {
            Debug.LogWarning("���ǿ� ���� ����� ���� �ʾҽ��ϴ�");
        }
    }
}