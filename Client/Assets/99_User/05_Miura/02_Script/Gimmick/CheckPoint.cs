using UnityEngine;

//======================================
//�ޗ��ɗ��������ɁA�����O�̈ʒu�ɕ������邽�߂�
//�`�F�b�N�|�C���g�̃X�N���v�g
//�쐬��:�O�Y�L�H
//�X�V:2025/06/05
//=======================================

public class CheckPoint : MonoBehaviour
{
    public bool isCheck=false;
    GameObject spawnObject=new GameObject();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnObject = this.gameObject.transform.GetChild(0).GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            isCheck = true;
        }
    }
}
