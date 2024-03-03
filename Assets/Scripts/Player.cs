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
        IsLoggedIn = false; // 플레이어가 초기에 로그인되지 않은 상태로 설정
    }

    // 플레이어의 위치 업데이트 메서드
    public void SetPosition(GameObject newPosition)
    {
        CurrentPosition = newPosition.transform.position;
        PlayerObject = newPosition;
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