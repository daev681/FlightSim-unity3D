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
    private Player mainPlayer; // ��ü �÷��̾ ������ ���� �߰�


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

    public bool SyncPlayers(int playerId, Player player)
    {
        // �̹� �����ϴ� �÷��̾����� Ȯ��
        if (players.ContainsKey(playerId))
        {
            // �̹� �����ϴ� �÷��̾��� ��� ������Ʈ
            players[playerId] = player;
            return true;
        }
        else
        {
            // �������� �ʴ� �÷��̾��� ��� �߰�
            players.Add(playerId, player);
            return false;

        }
    }

    public void SpawnF15(Player player)
    {
        // ������ ��� ����
        string prefabPath = "Assets/Prefabs/F15.prefab";

        // ������ �ε�
        GameObject f15Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);


        if (f15Prefab != null)
        {
            // ������ ����
            GameObject newF15 = UnityEngine.Object.Instantiate(f15Prefab);
            Plane playerObject = newF15.GetComponent<Plane>();
            AIController script = newF15.GetComponent<AIController>();
            if (script != null)
            {
                script.enabled = false;
            }

            // ������ ������Ʈ �̸� ����
            newF15.name = player.PlayerId.ToString();

            // ���ο� ��ġ �� ȸ������ �̵�

            newF15.transform.position = player.CurrentPosition;
            newF15.transform.rotation = player.CurrentRotation;
            player.PlayerObject = playerObject;

            bool isSync = Instance.SyncPlayers(player.PlayerId, player);


        }
        else
        {
            Debug.LogError("F15Prefab�� �ε��� �� �����ϴ�.");
        }
    }


    public bool DeletePlayer(int playerId)
    {
        // �÷��̾ �ִ��� Ȯ���ϰ� �ִٸ� ����
        if (players.ContainsKey(playerId))
        {

            players.Remove(playerId);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject objToDelete = GameObject.Find(playerId.ToString());

            // ��ü�� �����ϴ��� Ȯ�� �� �����մϴ�.
            if (objToDelete != null)
            {
                // Destroy �Լ��� ����Ͽ� ��ü�� �����մϴ�.
                UnityEngine.Object.Destroy(objToDelete);
            }
            else
            {
                // ��ü�� �������� �ʴ� ��쿡 ���� ó���� �߰��� �� �ֽ��ϴ�.
                Debug.LogWarning("Object with name '7��' not found.");
                }
            });
            var messasge = new C_CHAT()
            {
                Msg = playerId + " ���� �ı��Ǿ����ϴ�",
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

    // �÷��̾ ��ü �÷��̾����� ���θ� Ȯ���ϴ� �޼���
    public bool IsMainPlayer(int playerId)
    {
        Player player;
        if (players.TryGetValue(playerId, out player))
        {
            return player.IsMainPlayer;
        }
        return false; // �ش� playerId�� �ش��ϴ� �÷��̾ ���� ���
    }

    // �÷��̾ ��ü �÷��̾����� ���θ� Ȯ���ϴ� �޼���
    public Player getMainPlayer()
    {
        foreach (var player in players.Values)
        {
            if (player.IsMainPlayer)
            {
                return player;
            }
        }
        return null; // ��ü �÷��̾ ã�� ���� ���
    }



    public bool isPlayerById(int playerId)
    {
        return players.ContainsKey(playerId);
    }


    public void UpdatePlayerPosition(int playerId, Vector3 position , Quaternion quaternion)
    {

        if (Instance.IsExistPlayer(playerId) && IsMainPlayer(playerId))
        {
            // ��Ŷ ����
            var playerPositionMessage = new C_POSITION()
            {
                Px = position.x,
                Py = position.y,
                Pz = position.z,
                Rx = quaternion.x,
                Ry = quaternion.y,
                Rz = quaternion.z,

            };
            // ��Ŷ ������ ����

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

    // ���¿��� Ÿ������ ���� �޼���
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

            // ���� ���� �÷��̾��� ��ġ
            Vector3 mainPlayerPosition = mainPlayer.CurrentPosition;

            // ��� �÷��̾�� �߿��� ���� �÷��̾�� ���� ����� �÷��̾ ã��
            Player nearestPlayer = null;
            float nearestDistance = float.MaxValue;
            foreach (var player in players.Values)
            {
                // ���� �÷��̾�� Ÿ������ �������� ����
                if (player.IsMainPlayer)
                    continue;

                // �÷��̾��� ��ġ�� ���� �÷��̾���� �Ÿ� ���
                float distance = Vector3.Distance(mainPlayerPosition, player.CurrentPosition);
                if (distance < nearestDistance)
                {
                    nearestPlayer = player;
                    nearestDistance = distance;
                }
            }

            // ���� ����� �÷��̾ Ÿ������ ����
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