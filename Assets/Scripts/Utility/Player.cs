using UnityEngine;

public class Player
{
    private static Player instance;

    public Player(int playerId, string playerName)
    {
        this.playerId = playerId;
        this.playerName = playerName;
    }

    public int isLogin = 0;
    public int playerId { get; private set; }
    public string playerName { get; private set; }
    public bool IsLoggedIn { get; set; }
    public Vector3 CurrentPosition { get; private set; }
    public GameObject PlayerObject { get; private set; }
    private Player() { }

    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    // PlayerName �Ӽ��� ���� Getter �� Setter �Լ�
    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Player();
            }
            return instance;
        }
    }

    // �÷��̾��� ��ġ ������Ʈ �޼���
    public void SetPosition(Vector3 newPosition)
    {
        CurrentPosition = newPosition;

    }

    // �÷��̾��� GameObject ���� �޼���
    public void SetPlayerObject(GameObject playerObject)
    {
        PlayerObject = playerObject;
    }

    // �÷��̾ ��ġ�� ����� ������ �������� �޼���
    public GameObject GetPlayerPlane()
    {
        return PlayerObject;
    }
}