//===================
// �G���x�[�^�[���~�{�^���X�N���v�g
// Author:Nishiura
// Date:2025/07/04
//===================
using DG.Tweening;
using UnityEngine;

public class ElevatorButton : GimmickBase
{
    // �ΏۃG���x�[�^�̃v���n�u
    [SerializeField] GameObject targetElevator;
    //�{�^���I�u�W�F�N�g
    [SerializeField] GameObject buttonObj;
    Elevator elevatorScript;
    
    // �{�^���̎��(f:���~t:�㏸)
    public bool buttonType;
    bool isEnterd = false;

    void Start()
    {
        elevatorScript = targetElevator.GetComponent<Elevator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isEnterd && !elevatorScript.isMoving)
        {
            TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �v���C���[���{�^���͈͓��ɓ������ꍇ
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isEnterd = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // �v���C���[���{�^���͈͓�����o���ꍇ
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isEnterd = false;
        }
    }

    void MoveButton() 
    {
        buttonObj.transform.localPosition =  new Vector2(0f,-0.08f);
        //Sequence�̃C���X�^���X���쐬
        var sequence = DOTween.Sequence();

        //Append�œ����ǉ����Ă���
        sequence.Append(buttonObj.transform.DOMoveY(buttonObj.transform.position.y - 0.3f, 0.5f))
                 .AppendInterval(0.1f)
                 .Append(buttonObj.transform.DOMoveY(buttonObj.transform.position.y, 0.5f));
        //Play�Ŏ��s
        sequence.Play();
    }

    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public override void TurnOnPower()
    {
        //�G���x�[�^�[�̃{�^����AudioSource���擾����
        AudioSource elevatorButtonSE=GetComponent<AudioSource>();

        //�G���x�[�^�[�̃{�^����SE���Đ�����
        elevatorButtonSE.Play();

        elevatorScript.MoveButton(buttonType);
        MoveButton();
    }
}
