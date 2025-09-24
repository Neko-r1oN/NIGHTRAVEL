//===================
// バーナーに関する処理
// Aouther:y-miura
// Date:2025/07/07
//===================
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Burner : GimmickBase
{
    PlayerBase player;
    EnemyBase enemy;
    DebuffController statusEffectController;
    [SerializeField] GameObject flame;
    [SerializeField] AudioSource flameSE;

    NavMeshObstacle navMeshObstacle;
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider;

    bool isFlame;
    const float repeatRate = 3f;
    float timer = 0;        // マスタクライアントが自身に切り替わったとき用

    private void Start()
    {
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        // オフライン or マルチプレイ時にマスタクライアントの場合
        if(!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            //3秒間隔で点火と消火を繰り返す
            InvokeRepeating("RequestActivateGimmick", 0.1f, repeatRate);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    /// <summary>
    /// 触れたプレイヤー/敵に炎上効果を付与する処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//触れたオブジェクトのタグが「Player」だったら

            // ダメージを適用させる対象が自分の操作キャラの場合
            if(collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
            {
                player = GetComponent<PlayerBase>();
                statusEffectController = collision.gameObject.GetComponent<DebuffController>(); //触れたオブジェクトのStatusEffectControllerをGetComponentする

                statusEffectController.ApplyStatusEffect(DEBUFF_TYPE.Burn); //プレイヤーに炎上状態を付与
                Debug.Log("プレイヤーに炎上状態を付与");
            }
        }

        if (collision.CompareTag("Enemy"))
        {//触れたオブジェクトのタグが「Enemy」だったら

            // オフライン時 or マルチプレイ時でマスタクライアントの場合
            if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                enemy = GetComponent<EnemyBase>();
                statusEffectController = collision.gameObject.GetComponent<DebuffController>(); //触れたオブジェクトのStatusEffectControllerを取得する

                Debug.Log("敵に炎上状態を付与");
            }
        }
    }

    /// <summary>
    /// 炎を点けたり消したりする処理
    /// </summary>
    private void Ignition()
    {
        timer = 0;
        if (isFlame==true)
        {
            flameSE.Stop();

            // 起動停止
            navMeshObstacle.enabled = false;
            spriteRenderer.enabled = false;
            boxCollider.enabled = false;
            isFlame = false;
        }
        else if(isFlame==false)
        {
            flameSE.Play();

            // 起動開始
            navMeshObstacle.enabled = true;
            spriteRenderer.enabled = true;
            boxCollider.enabled = true;
            isFlame = true;
        }
    }

    /// <summary>
    /// ギミック起動リクエスト
    /// </summary>
    void RequestActivateGimmick()
    {
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
    }

    /// <summary>
    /// ギミック起動処理
    /// </summary>
    public override void TurnOnPower()
    {
        Ignition();
    }

    /// <summary>
    /// ギミック再起動処理
    /// </summary>
    public override void Reactivate()
    {
        if (timer > repeatRate) timer = repeatRate;
        InvokeRepeating("RequestActivateGimmick", repeatRate - timer, repeatRate);
    }
}
