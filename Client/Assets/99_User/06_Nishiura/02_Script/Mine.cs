//===================
// �n���X�N���v�g
// Author:Nishiura
// Date:2025/07/02
//===================
using UnityEngine;

public class Mine : GimmickBase
{
    [SerializeField] GameObject boomEffect; // �����G�t�F�N�g�v���n�u

    Vector2 pos;

    private void Start()
    {
        // ���̃Q�[���I�u�W�F�N�g�̃|�W�V�������擾
        pos = this.gameObject.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
        }
        else if (collision.transform.tag == "Enemy")
        {
            if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
            }
        }
    }

    public override void TurnOnPower()
    {
        GetComponent<AudioSource>().Play();

        Instantiate(boomEffect, pos, Quaternion.identity);    // �����G�t�F�N�g�𐶐�
        Destroy(this.gameObject);   // ���g��j��
    }
}
