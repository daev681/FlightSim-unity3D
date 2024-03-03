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

    // PlayerName 속성에 대한 Getter 및 Setter 함수
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

    // 플레이어의 위치 업데이트 메서드
    public void SetPosition(Vector3 newPosition)
    {
        CurrentPosition = newPosition;

    }

    // 플레이어의 GameObject 설정 메서드
    public void SetPlayerObject(GameObject playerObject)
    {
        PlayerObject = playerObject;
    }

    // 플레이어가 위치한 평면의 정보를 가져오는 메서드
    public GameObject GetPlayerPlane()
    {
        return PlayerObject;
    }
}