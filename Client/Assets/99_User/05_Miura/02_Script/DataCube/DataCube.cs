//========================
// �f�[�^�L���[�u�Ɋւ��鏈��
// Aouther:y-miura
// Date:2025/08/06
//========================

using Unity.VisualScripting;
using UnityEngine;

class DataCube : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    //�v���C���[���f�[�^�L���[�u�ɐG�ꂽ��
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Destroy(this.gameObject);
        }
    }
}
