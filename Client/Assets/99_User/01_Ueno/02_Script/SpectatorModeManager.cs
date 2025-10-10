using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpectatorModeManager : MonoBehaviour
{
    Dictionary<int, GameObject> players = new Dictionary<int, GameObject>(); 
    int followKey = 0;

    private static SpectatorModeManager instance;

    public static SpectatorModeManager Instance
    {
        get { return instance; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int key = 0;
        foreach(var obj in CharacterManager.Instance.PlayerObjs.Values)
        {
            players.Add(key, obj);

            key++;
        }
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

                //Camera.main.gameObject.GetComponent<CameraFollow>().Target = player.Value.transform;

                UIManager.Instance.ChangeStatusToTargetPlayer(player.Value.GetComponent<PlayerBase>());
                break;
            }
        }
    }
}
