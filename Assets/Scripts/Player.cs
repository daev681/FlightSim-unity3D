using UnityEngine;

public class Player
{
    public int PlayerId { get; private set; }
    public string PlayerName { get; private set; }
    public bool IsLoggedIn { get; set; }
    public Vector3 CurrentPosition { get; private set; }
    public GameObject PlayerObject { get; private set; }



    public Player(int playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        IsLoggedIn = false; // �÷��̾ �ʱ⿡ �α��ε��� ���� ���·� ����
    }

    // �÷��̾��� ��ġ ������Ʈ �޼���
    public void SetPosition(GameObject newPosition)
    {
        CurrentPosition = newPosition.transform.position;
        PlayerObject = newPosition;
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