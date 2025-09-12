//===================
// �n���X�N���v�g
// Author:Nishiura
// Date:2025/07/02
//===================
using UnityEngine;

public class Mine : MonoBehaviour
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
        if (collision.transform.tag == "Player" /*&& collision.gameObject == CharacterManager.Instance.PlayerObjSelf*/)
        {
            Instantiate(boomEffect, pos + new Vector2(0.0f, 0.5f), Quaternion.identity);    // �����G�t�F�N�g�𐶐�
            Destroy(this.gameObject);   // ���g��j��
            Debug.Log("Boomed Mine");
        }
        else if (collision.transform.tag == "Enemy" && !RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            Instantiate(boomEffect, pos, Quaternion.identity);    // �����G�t�F�N�g�𐶐�
            Destroy(this.gameObject);   // ���g��j��
            Debug.Log("Boomed Mine");
        }
    }
}
