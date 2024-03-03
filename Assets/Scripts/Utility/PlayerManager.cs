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

    public void UpdatePlayerPosition(int playerId)
    {
        if (players.ContainsKey(playerId))
        {
            Player player = players[playerId];
            // ��Ŷ ����
            var playerPositionMessage = new C_POSITION()
            {
                PlayerId = (ulong)playerId,
                X = (ulong)player.CurrentPosition.x,
                Y = (ulong)player.CurrentPosition.y,
                Z = (ulong)player.CurrentPosition.z
            };

            // ��Ŷ ������ ����
            PacketManager.Instance.SendToServer(playerPositionMessage, PacketType.PKT_C_POSITION);
        }
        else
        {
            // �ش� playerId�� ���� �÷��̾ �������� ������ �˸��� �޽��� ��� �Ǵ� �ٸ� ó�� ����
            // ����: Debug.LogWarning("�������� �ʴ� �÷��̾� ID�Դϴ�.");
        }
    }
}