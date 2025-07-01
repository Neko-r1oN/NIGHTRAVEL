//====================
//�v���X�}�V���̃X�N���v�g
//Aouther:y-miura
//Date:2025/06/20
//====================

using DG.Tweening;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PressMachine : GimmickBase
{
    [SerializeField] GameObject machineFragment;
    PlayerBase player;
    Rigidbody2D rigidbody2d;
    bool isBroken = false;
    public float addPow;
    public float pullPow;
    public bool isPowerd = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MovePress();
    }

    private void Update()
    {

    }

    public async void MovePress()
    {
        //Sequence�̃C���X�^���X���쐬
        var sequence = DOTween.Sequence();

        //Append�œ����ǉ����Ă���
         sequence.Append(this.transform.DOMoveY(-addPow, 1))
                 .AppendInterval(1)
                 .Append(this.transform.DOMoveY(pullPow, 2));
                
        //Play�Ŏ��s
        sequence.Play()
                .AppendInterval(1)
                .SetLoops(-1);
    }

    public override void TurnOnPower()
    {
       isPowerd = false;
    }
}
