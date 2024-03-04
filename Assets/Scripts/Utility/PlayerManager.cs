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
        // �̹� �����ϴ� �÷��̾����� Ȯ���ϰ� �߰�
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

    // ���� �÷��̾ �����ϴ� �޼���
    public void SetCurrentPlayer(Player player)
    {
        player.isLogin = 1;
        currentPlayer = player;
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
    public bool IsPlayerAlreadySpawned(int playerId)
    {
        return currentPlayer != null && currentPlayer.playerId == playerId;
    }

  


    public void UpdatePlayerPosition(Vector3 position) 
    {
        if (players.ContainsKey(currentPlayer.playerId) && currentPlayer.isLogin == 1)
        {
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
}