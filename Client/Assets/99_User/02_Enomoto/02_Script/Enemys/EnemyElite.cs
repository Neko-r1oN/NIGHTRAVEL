using Pixeye.Unity;
using System;
using UnityEngine;
using static StatusEffectController;

public class EnemyElite : MonoBehaviour
{
    #region マテリアル
    [Foldout("マテリアル")]
    [SerializeField]
    Material blazeMaterial;

    [Foldout("マテリアル")]
    [SerializeField]
    Material frostMaterial;

    [Foldout("マテリアル")]
    [SerializeField]
    Material thunderMaterial;
    #endregion

    #region プレファブ
    [SerializeField]
    GameObject blazeTrap;
    #endregion

    #region 特殊効果が発動する間隔
    readonly float blazeTickInterval = 0.2f; // ブレイズエリート
    #endregion

    Rigidbody2D rb2D;
    float timer;

    /// <summary>
    /// エリートの種類
    /// </summary>
    public enum ELITE_TYPE
    {
        None,
        Blaze,      // ブレイズエリート
        Frost,      // フロストエリート
        Thunder     // サンダーエリート
    }
    ELITE_TYPE eliteType = ELITE_TYPE.None;
    public ELITE_TYPE EliteType { get { return eliteType; } }

    /// <summary>
    /// 付与する状態異常の種類
    /// </summary>
    public StatusEffectController.EFFECT_TYPE addStatusEffect { get; private set; }

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Init(ELITE_TYPE type)
    {
        eliteType = type;

        // 50%UPしたステータスデータ作成
        CharacterStatusData data = new CharacterStatusData();
        CharacterBase charaBase = GetComponent<CharacterBase>();
        data.hp = charaBase.BaseHP + Mathf.CeilToInt(charaBase.BaseHP * 0.5f);
        data.power = charaBase.BasePower+ Mathf.CeilToInt(charaBase.BasePower * 0.5f);
        data.moveSpeed = charaBase.BaseMoveSpeed + Mathf.CeilToInt(charaBase.BaseMoveSpeed * 0.5f);

        var spriteRenderer = GetComponent<SpriteRenderer>();
        Action action = type switch
        {
            ELITE_TYPE.Blaze => () =>
            {
                spriteRenderer.material = blazeMaterial;
            }
            ,
            ELITE_TYPE.Frost => () =>
            {
                spriteRenderer.material = frostMaterial;
            }
            ,
            ELITE_TYPE.Thunder => () =>
            {
                spriteRenderer.material = thunderMaterial;
                data.moveSpeed = charaBase.BaseMoveSpeed * 2;
            }
            ,
            _ => () => { }
        };
        action();

        // 基礎ステータス&現在のステータスを上書き
        charaBase.OverrideBaseStatus(data, true);
    }

    /// <summary>
    /// 付与させる状態異常のenumを取得する
    /// </summary>
    /// <returns></returns>
    public StatusEffectController.EFFECT_TYPE GetAddStatusEffectEnum()
    {
        return eliteType switch
        {
            ELITE_TYPE.Blaze => StatusEffectController.EFFECT_TYPE.Burn,
            ELITE_TYPE.Frost => StatusEffectController.EFFECT_TYPE.Freeze,
            ELITE_TYPE.Thunder => StatusEffectController.EFFECT_TYPE.Shock,
            _ => StatusEffectController.EFFECT_TYPE.None
        };
    }

    /// <summary>
    /// ブレイズエリートの特殊効果処理
    /// </summary>
    public void ActivateBlazeEffect()
    {
        GameObject trap = Instantiate(blazeTrap, transform.position, Quaternion.identity);
        trap.GetComponent<BlazeTrap>().InitializeBlazeTrap(this.transform);

        // 移動中は一歩先にトラップを移動させる
        //float posX = transform.position.x;
        //if (Mathf.Abs(rb2D.linearVelocityX) > 0)
        //{
        //    var sqSr = GetComponent<SpriteRenderer>();
        //    posX += TransformHelper.GetFacingDirection(transform) * (sqSr.bounds.size.x / 2);
        //    trap.transform.position = new Vector2(posX, trap.transform.position.y);
        //}
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        Action action = eliteType switch
        {
            ELITE_TYPE.Blaze => () => {
                if (timer > blazeTickInterval)
                {
                    timer = 0;
                    ActivateBlazeEffect();
                }
            }
            ,
            _ => () => { }
        };
        action();
    }
}
