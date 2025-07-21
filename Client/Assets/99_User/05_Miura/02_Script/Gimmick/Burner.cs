//===================
// バーナーに関する処理
// Aouther:y-miura
// Date:2025/07/07
//===================
using UnityEngine;

public class Burner : GimmickBase
{
    PlayerBase player;
    EnemyBase enemy;
    StatusEffectController statusEffectController;
    [SerializeField] GameObject flame;
    bool isFlame;

    //オブジェクトの起動状態のID
    //triggerIDが起動状態のIDになる
    public enum Power_ID
    {
        ON = 0,
        OFF
    };

    private void Start()
    {
        //3秒間隔で点火と消火を繰り返す
        InvokeRepeating("Ignition", 0.1f, 3);
    }

    private void Update()
    {

    }

    /// <summary>
    /// 触れたプレイヤー/敵に炎上効果を付与する処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//触れたオブジェクトのタグが「Player」だったら
            player = GetComponent<PlayerBase>();
            statusEffectController = collision.gameObject.GetComponent<StatusEffectController>(); //触れたオブジェクトのStatusEffectControllerをGetComponentする

            statusEffectController.ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Burn); //プレイヤーに炎上状態を付与
            Debug.Log("プレイヤーに炎上状態を付与");
        }

        if (collision.CompareTag("Enemy"))
        {//触れたオブジェクトのタグが「Enemy」だったら
            enemy = GetComponent<EnemyBase>();
            statusEffectController = collision.gameObject.GetComponent<StatusEffectController>(); //触れたオブジェクトのStatusEffectControllerを取得する

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
