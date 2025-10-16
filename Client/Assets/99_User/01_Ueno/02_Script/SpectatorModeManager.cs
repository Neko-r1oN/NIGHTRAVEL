using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;

public class SpectatorModeManager : MonoBehaviour
{
    Dictionary<int, GameObject> players = new Dictionary<int, GameObject>(); 
    int followKey = 0;

    private static SpectatorModeManager instance;

    GameObject camera;

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
        int key = 0;
        foreach (var obj in CharacterManager.Instance.PlayerObjs.Values)
        {
            players.Add(key, obj);

            key++;
        }

        camera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("FocusCameraOnAlivePlayer")]
    public void FocusCameraOnAlivePlayer()
    {
        foreach(var player in players)
        {
            if(player.Value == null || player.Value.GetComponent<PlayerBase>().IsDead)
            {
                continue;
            }

            if(player.Value != CharacterManager.Instance.PlayerObjSelf && followKey != player.Key)
            {
                followKey = player.Key;

                camera.GetComponent<CinemachineCamera>().Target.TrackingTarget 
                    = player.Value.transform;

                UIManager.Instance.ChangeStatusToTargetPlayer(player.Value.GetComponent<PlayerBase>());
                break;
            }
        }

        List<GameObject> list = new List<GameObject>();
        foreach (var obj in CharacterManager.Instance.PlayerUIObjs)
        {
            list.Add(obj.Value);
        }

        foreach (var obj in list)
        {
            Destroy(obj);
        }
    }
}
