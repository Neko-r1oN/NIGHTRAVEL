using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class SawBladeManager : GimmickBase
{
    Vector2 pos;

    [SerializeField] GameObject sparkObj;
    [SerializeField] SawBlade sawBlade;

    // ���͒l
    public float addPower;

    // �ړ����x
    public float moveSpeed;

    // �d������
    public bool isPowerd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            TurnOnPower();
            isPowerd = true;

        // ���̃Q�[���I�u�W�F�N�g�̃|�W�V�������擾
        pos = this.transform.position;
    }

    /// <summary>
    /// �ۂ̂��ړ��֐�
    /// </summary>
    private void MoveBlade()
    {
        //    //Sequence�̃C���X�^���X���쐬
        var sequence = DOTween.Sequence();

        transform.DOMoveX((pos.x - addPower), 1)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo)
                    .OnStepComplete(() =>
                    {
                        // ���[�v1�������ƂɌĂ΂��
                        Vector3 scale = transform.localScale;
                        scale.x *= -1;
                        transform.localScale = scale;
                    });
    }

    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public override void TurnOnPower()
    {
        sparkObj.SetActive(true);
        MoveBlade();
        sawBlade.StateRotet();
    }
}
