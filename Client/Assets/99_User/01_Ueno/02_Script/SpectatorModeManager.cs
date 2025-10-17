using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using Rewired;

public class SpectatorModeManager : MonoBehaviour
{
    Dictionary<int, GameObject> players = new Dictionary<int, GameObject>(); 
    int followKey = 0;

    private static SpectatorModeManager instance;

    [SerializeField] CinemachineCamera camera;

    public static SpectatorModeManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (players.Count == 0)
        {
            int key = 0;
            foreach (var obj in CharacterManager.Instance.PlayerObjs.Values)
            {
                players.Add(key, obj);

                if (obj == CharacterManager.Instance.PlayerObjSelf) followKey = key;

                key++;
            }
        }
    }

    [ContextMenu("FocusCameraOnAlivePlayer")]
    public void FocusCameraOnAlivePlayer()
    {
        foreach (var player in players)
        {
            if(player.Value == null || player.Value.GetComponent<PlayerBase>().IsDead)
            {
                continue;
            }

            if(player.Value != CharacterManager.Instance.PlayerObjSelf && followKey != player.Key)
            {
                followKey = player.Key;

                camera.Target.TrackingTarget = player.Value.transform;

                UIManager.Instance.ChangeStatusToTargetPlayer(player.Value.GetComponent<PlayerBase>());
                break;
            }
        }

        List<GameObject> list = new List<GameObject>(CharacterManager.Instance.PlayerUIObjs.Values);
        foreach (var obj in list)
        {
            Destroy(obj);
        }
        CharacterManager.Instance.PlayerUIObjs = new Dictionary<Guid, GameObject>();
    }
}
