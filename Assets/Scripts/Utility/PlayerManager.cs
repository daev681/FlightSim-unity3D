using System.Collections.Generic;
using UnityEngine;
using Protocol;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEditor;

public class PlayerManager
{
    private static PlayerManager instance;
    private static Dictionary<int, Player> players = new Dictionary<int, Player>();
    private PlayerManager() { }
    private Player mainPlayer; // 주체 플레이어를 저장할 변수 추가


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

    // 딕셔너리를 초기화하는 메서드
    public void ClearPlayers()
    {
        players.Clear();
    }

    public bool SyncPlayers(int playerId, Player player)
    {
        // 이미 존재하는 플레이어인지 확인
        if (players.ContainsKey(playerId))
        {
            // 이미 존재하는 플레이어인 경우 업데이트
            players[playerId] = player;
            return true;
        }
        else
        {
            // 존재하지 않는 플레이어인 경우 추가
            players.Add(playerId, player);
            return false;

        }
    }

    public void SpawnF15(Player player)
    {
        // 프리팹 경로 설정
        string prefabPath = "Assets/Prefabs/F15.prefab";

        // 프리팹 로드
        GameObject f15Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);


        if (f15Prefab != null)
        {
            // 프리팹 복제
            GameObject newF15 = UnityEngine.Object.Instantiate(f15Prefab);
            Plane playerObject = newF15.GetComponent<Plane>();
            AIController script = newF15.GetComponent<AIController>();
            if (script != null)
            {
                script.enabled = false;
            }

            // 복제된 오브젝트 이름 변경
            newF15.name = player.PlayerId.ToString();

            // 새로운 위치 및 회전으로 이동

            newF15.transform.position = player.CurrentPosition;
            newF15.transform.rotation = player.CurrentRotation;
            player.PlayerObject = playerObject;

            bool isSync = Instance.SyncPlayers(player.PlayerId, player);


        }
        else
        {
            Debug.LogError("F15Prefab을 로드할 수 없습니다.");
        }
    }


    public bool DeletePlayer(int playerId)
    {
        // 플레이어가 있는지 확인하고 있다면 삭제
        if (players.ContainsKey(playerId))
        {

            players.Remove(playerId);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject objToDelete = GameObject.Find(playerId.ToString());

            // 객체가 존재하는지 확인 후 삭제합니다.
            if (objToDelete != null)
            {
                // Destroy 함수를 사용하여 객체를 삭제합니다.
                UnityEngine.Object.Destroy(objToDelete);
            }
            else
            {
                // 객체가 존재하지 않는 경우에 대한 처리를 추가할 수 있습니다.
                Debug.LogWarning("Object with name '7번' not found.");
                }
            });
            var messasge = new C_CHAT()
            {
                Msg = playerId + " 님이 파괴되었습니다",
            };
            PacketManager.Instance.SendToServer(messasge, PacketType.PKT_C_CHAT);
            return true;
        }
        else
        {

            return false;

        }
    }

    public bool IsExistPlayer(int playerId)
    {

        return players.ContainsKey(playerId);
    }

    // 플레이어가 주체 플레이어인지 여부를 확인하는 메서드
    public bool IsMainPlayer(int playerId)
    {
        Player player;
        if (players.TryGetValue(playerId, out player))
        {
            return player.IsMainPlayer;
        }
        return false; // 해당 playerId에 해당하는 플레이어가 없는 경우
    }

    // 플레이어가 주체 플레이어인지 여부를 확인하는 메서드
    public Player getMainPlayer()
    {
        foreach (var player in players.Values)
        {
            if (player.IsMainPlayer)
            {
                return player;
            }
        }
        return null; // 주체 플레이어를 찾지 못한 경우
    }



    public bool isPlayerById(int playerId)
    {
        return players.ContainsKey(playerId);
    }


    public void UpdatePlayerPosition(int playerId, Vector3 position , Quaternion quaternion)
    {

        if (Instance.IsExistPlayer(playerId) && IsMainPlayer(playerId))
        {
            // 패킷 생성
            var playerPositionMessage = new C_POSITION()
            {
                Px = position.x,
                Py = position.y,
                Pz = position.z,
                Rx = quaternion.x,
                Ry = quaternion.y,
                Rz = quaternion.z,

            };
            // 패킷 서버로 전송

            PacketManager.Instance.SendToServer(playerPositionMessage, PacketType.PKT_C_POSITION);
        }
        else
        {

        }
    }

    public void SyncFireMissile(Vector3 position, Quaternion rotation)
    {
            var FireMessage = new C_MISSILE()
            {
                Px = position.x,
                Py = position.y,
                Pz = position.z,
                Rx = rotation.x,
                Ry = rotation.y,
                Rz = rotation.z,
            };
            PacketManager.Instance.SendToServer(FireMessage, PacketType.PKT_C_MISSILE);
    }

    // 오픈월드 타겟팅을 위한 메서드
    public void UpdateTargetForMainPlayer(int playerId)
    {
        if (Instance.IsExistPlayer(playerId) && IsMainPlayer(playerId)  && players.Count >= 2)
        {
            Player mainPlayer = players[playerId];

            if (mainPlayer == null || !mainPlayer.IsMainPlayer)
            {
                Debug.LogWarning("No main player found or current player is not main player.");
                return;
            }

            // 현재 메인 플레이어의 위치
            Vector3 mainPlayerPosition = mainPlayer.CurrentPosition;

            // 모든 플레이어들 중에서 메인 플레이어와 가장 가까운 플레이어를 찾음
            Player nearestPlayer = null;
            float nearestDistance = float.MaxValue;
            foreach (var player in players.Values)
            {
                // 메인 플레이어는 타겟으로 선택하지 않음
                if (player.IsMainPlayer)
                    continue;

                // 플레이어의 위치와 메인 플레이어와의 거리 계산
                float distance = Vector3.Distance(mainPlayerPosition, player.CurrentPosition);
                if (distance < nearestDistance)
                {
                    nearestPlayer = player;
                    nearestDistance = distance;
                }
            }

            // 가장 가까운 플레이어를 타겟으로 설정
            if (nearestPlayer != null)
            {
                mainPlayer.Target = nearestPlayer.PlayerObject.GetComponent<Target>();
                Debug.Log("Main player's target updated: " + nearestPlayer.PlayerName);
            }
            else
            {
                mainPlayer.Target = null;
                Debug.LogWarning("No target found for main player.");
            }
        }
           
    }


}