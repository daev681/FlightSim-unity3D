using UnityEngine;

public class Player
{
    private static Player instance;

    private int isLogin;
    private int playerId;
    private string playerName;
    private bool isLoggedIn;
    private Vector3 currentPosition;
    private Quaternion currentRotation;
    private GameObject playerObject;
    private bool isMainPlayer = false; // �÷��̾ ��ü���� ����

    public Player() { }

    public bool IsMainPlayer
    {
        get { return isMainPlayer; }
        set { isMainPlayer = value; }
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

    public int IsLogin
    {
        get { return isLogin; }
        set { isLogin = value; }
    }

    public int PlayerId
    {
        get { return playerId; }
        set { playerId = value; }
    }

    public string PlayerName
    {
        get { return playerName; }
        set { playerName = value; }
    }

    public bool IsLoggedIn
    {
        get { return isLoggedIn; }
        set { isLoggedIn = value; }
    }

    public Vector3 CurrentPosition
    {
        get { return currentPosition; }
        set { currentPosition = value; }
    }

    public Quaternion CurrentRotation
    {
        get { return currentRotation; }
        set { currentRotation = value; }
    }


    public GameObject PlayerObject
    {
        get { return playerObject; }
        set { playerObject = value; }
    }

    // �÷��̾ ��ġ�� ����� ������ �������� �޼���
    public GameObject GetPlayerPlane()
    {
        return playerObject;
    }
}