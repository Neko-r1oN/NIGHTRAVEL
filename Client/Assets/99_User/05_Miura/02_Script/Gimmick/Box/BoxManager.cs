//====================
// 足場とする箱を管理するスクリプト
// Aouther:y-miura
// Date:2025/07/20
//====================

using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [SerializeField] GameObject BoxPrefab;  //箱プレハブ取得

    public float spawnTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //SpawnBox関数を繰り返し呼び出して、箱を繰り返し生成する
        InvokeRepeating("SpawnBox", 0.1f, spawnTime);
    }

    /// <summary>
    /// 箱を生成する処理
    /// </summary>
    public void SpawnBox()
    {
        float spawnX = Random.Range(1, 3);
        if (spawnX == 1)
        {
            //箱を生成する
            Instantiate(BoxPrefab, new Vector2(27.09f, 27), Quaternion.identity);
        }
        if (spawnX == 2)
        {
            //箱を生成する
            Instantiate(BoxPrefab, new Vector2(28.9f, 27), Quaternion.identity);
        }
    }
}
