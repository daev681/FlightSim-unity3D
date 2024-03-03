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
        // �̹� �����ϴ� �÷��̾����� Ȯ���ϰ� �߰�
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
            return null; // �÷��̾ �������� ���� ��� null ��ȯ
        }
    }

    public void UpdatePlayerPosition(int playerId, GameObject playerObject)
    {
        if (players.ContainsKey(playerId))
        {
            Player player = players[playerId];
            player.SetPosition(playerObject);

            // ��Ŷ ����
            var playerPositionMessage = new C_POSITION()
            {
                PlayerId = 0,
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