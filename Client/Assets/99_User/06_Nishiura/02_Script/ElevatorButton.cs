//===================
// �G���x�[�^�[���~�{�^���X�N���v�g
// Author:Nishiura
// Date:2025/07/04
//===================
using DG.Tweening;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    // �ΏۃG���x�[�^�̃v���n�u
    [SerializeField] GameObject targetElevator;
    //�{�^���I�u�W�F�N�g
    [SerializeField] GameObject buttonObj;
    Elevator elevatorScript;
    
    // �{�^���̎��(f:���~t:�㏸)
    public bool buttonType;

    bool isEnterd = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        elevatorScript = targetElevator.GetComponent<Elevator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isEnterd && !elevatorScript.isMoving)
        {
            elevatorScript.MoveButton(buttonType);
            MoveButton();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �v���C���[���{�^���͈͓��ɓ������ꍇ
        if (collision.transform.tag == "Player")
        {
            isEnterd = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // �v���C���[���{�^���͈͓�����o���ꍇ
        if (collision.transform.tag == "Player")
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
}
