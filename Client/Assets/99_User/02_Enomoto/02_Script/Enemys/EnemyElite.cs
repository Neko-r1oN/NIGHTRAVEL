using Pixeye.Unity;
using System;
using UnityEngine;
using static StatusEffectController;

public class EnemyElite : MonoBehaviour
{
    #region �}�e���A��
    [Foldout("�}�e���A��")]
    [SerializeField]
    Material blazeMaterial;

    [Foldout("�}�e���A��")]
    [SerializeField]
    Material frostMaterial;

    [Foldout("�}�e���A��")]
    [SerializeField]
    Material thunderMaterial;
    #endregion

    #region �v���t�@�u
    [SerializeField]
    GameObject blazeTrap;
    #endregion

    #region ������ʂ���������Ԋu
    readonly float blazeTickInterval = 0.2f; // �u���C�Y�G���[�g
    #endregion

    Rigidbody2D rb2D;
    float timer;

    /// <summary>
    /// �G���[�g�̎��
    /// </summary>
    public enum ELITE_TYPE
    {
        None,
        Blaze,      // �u���C�Y�G���[�g
        Frost,      // �t���X�g�G���[�g
        Thunder     // �T���_�[�G���[�g
    }
    ELITE_TYPE eliteType = ELITE_TYPE.None;
    public ELITE_TYPE EliteType { get { return eliteType; } }

    /// <summary>
    /// �t�^�����Ԉُ�̎��
    /// </summary>
    public StatusEffectController.EFFECT_TYPE addStatusEffect { get; private set; }

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// ����������
    /// </summary>
    public void Init(ELITE_TYPE type)
    {
        eliteType = type;

        // 50%UP�����X�e�[�^�X�f�[�^�쐬
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

        // ��b�X�e�[�^�X&���݂̃X�e�[�^�X���㏑��
        charaBase.OverrideBaseStatus(data, true);
    }

    /// <summary>
    /// �t�^�������Ԉُ��enum���擾����
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
    /// �u���C�Y�G���[�g�̓�����ʏ���
    /// </summary>
    public void ActivateBlazeEffect()
    {
        GameObject trap = Instantiate(blazeTrap, transform.position, Quaternion.identity);
        trap.GetComponent<BlazeTrap>().InitializeBlazeTrap(this.transform);

        // �ړ����͈����Ƀg���b�v���ړ�������
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
