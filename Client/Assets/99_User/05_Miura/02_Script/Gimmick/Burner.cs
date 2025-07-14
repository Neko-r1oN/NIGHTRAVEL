//===================
// バーナーに関する処理
// Aouther:y-miura
// Date:2025/07/07
//===================

using System.Collections;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Burner : GimmickBase
{
    PlayerBase player;
    EnemyBase enemy;
    [SerializeField] GameObject flame;
    bool isFlame;

    private void Start()
    {
        //invokerepetingでIgnitionを呼ぶ
        //3秒間隔で点いたり消えたりする
        InvokeRepeating("Ignition", 0.1f, 3);
    }

    private void Update()
    {

    }

    /// <summary>
    /// 触れたオブジェクトに炎上効果を付与する処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = GetComponent<PlayerBase>();

            Debug.Log("プレイヤーに炎上状態を付与");
        }

        if (collision.CompareTag("Enemy"))
        {
            enemy = GetComponent<EnemyBase>();

            Debug.Log("敵に炎上状態を付与");
        }
    }

    /// <summary>
    /// 炎を点けたり消したりする処理
    /// </summary>
    private void Ignition()
    {
        if(isFlame==true)
        {//isFlameがtrueだったら
            //flameを非アクティブ状態にする
            flame.SetActive(false);
            isFlame = false;
        }
        else if(isFlame==false)
        {//isFlameがfalseだったら
            //flameをアクティブ状態にする
            flame.SetActive(true); 
            isFlame = true;
        }
    }

    public override void TurnOnPower(int triggerID)
    {

    }

    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }

}
