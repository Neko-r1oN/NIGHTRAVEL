using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared;

public class CharacterManager : MonoBehaviour
{
    #region プレイヤー関連
    [SerializeField] GameObject charaSwordPrefab;
    #endregion

    #region 敵関連
    Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();
    #endregion

    const float updateSec = 0.1f;
    bool isAwake = false;

    private void Awake()
    {
        if (RoomModel.Instance.ConnectionId == Guid.Empty) return;
        isAwake = true;
        StartCoroutine("UpdateCoroutine");
    }

    /// <summary>
    /// キャラクターの情報更新呼び出し用コルーチン
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateCoroutine()
    {
        if (RoomModel.Instance.IsMaster) UpdateMasterData();
        else UpdatePlayerData();
        yield return new WaitForSeconds(updateSec);
    }

    /// <summary>
    /// マスタークライアント用の情報更新
    /// </summary>
    async void UpdateMasterData()
    {

    }

    /// <summary>
    /// プレイヤーの情報更新
    /// </summary>
    async void UpdatePlayerData()
    {

    }

    /// <summary>
    /// 新たな敵をリストに追加する
    /// </summary>
    /// <param name="newEnemies"></param>
    public void AddEnemies(params GameObject[] newEnemies)
    {
        foreach (GameObject enemy in newEnemies)
        {
            enemies.Add(enemies.Count, enemy);
        }
    }
}
