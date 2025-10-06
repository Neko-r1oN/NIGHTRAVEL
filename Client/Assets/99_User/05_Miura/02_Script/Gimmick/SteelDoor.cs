//=================
//�|�S�������h�A�̃X�N���v�g
//Aouther:y-miura
//Date:20025/06/23
//=================

using DG.Tweening;
using System.Configuration;
using UnityEngine;

public class SteelDoor : GimmickBase
{
    [SerializeField] GameObject doorObj;

    [SerializeField] AudioClip openDoorSE;
    [SerializeField] AudioClip closeDoorSE;
    AudioSource audioSource;

    Vector2 initPos = Vector2.zero;//�����ʒu
    public bool isPowerd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //�����ʒu��ݒ�
        Debug.Log(transform.name);
        initPos = doorObj.transform.position;
    }

    /// <summary>
    /// �|�S�������h�A���J������
    /// </summary>
    /// <param name="collision">�G��Ă���I�u�W�F�N�g</param>
    private void OnTriggerStay2D(Collider2D collision)
    {//�Z���T�[�����ɂ��̂��G��Ă����
        if (collision.CompareTag("Player") && isPowerd == true)
        {//�uPlayer�v�^�O���t�������̂��G��Ă����
            //�h�A���J��
            doorObj.transform.DOMoveY(this.transform.position.y+5, 0.5f);
        }
    }

    /// <summary>
    /// �����h�A���J�����Ƃ���SE�Đ�����
    /// �J�������̒����ƒx������������̂ŕʂ̊֐�
    /// </summary>
    /// <param name="collision">�G�ꂽ�I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {//�Z���T�[�����ɂ��̂��G�ꂽ��
        if (collision.CompareTag("Player") && isPowerd == true && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {//�uPlayer�v�^�O���t�������̂��G�ꂽ��
            //�h�A���J��SE���Đ�����
            audioSource.PlayOneShot(openDoorSE);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isPowerd == true && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {//�uPlayer�v�^�O���t�������̂����ꂽ��
         //�h�A�����
            doorObj.transform.DOMoveY(initPos.y, 0.5f);

            //�h�A������SE���Đ�����
            audioSource.PlayOneShot(closeDoorSE);
        }
    }

    public override void TurnOnPower()
    {
        if (isPowerd) return;   // ���łɋN�����Ă���ꍇ�͏������Ȃ�
        isPowerd = true;
    }

}