//==============================
// �f�[�^�{�b�N�X�̃X�N���v�g
// Aouther:y-miura
// Date:2025/08/06
//==============================

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DataBox : Item
{
    private bool isOpened = false;
    private bool isTouch = false;
    public GameObject openObj;
    private Image image;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //SpriteRederer�R���|�[�l���g���擾
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTouch == true && isOpened == false)
        {//�f�[�^�{�b�N�X�ɐG�ꂽ&&�f�[�^�{�b�N�X���J���Ă��Ȃ�������
            isOpened = true; //�f�[�^�{�b�N�X���J�������Ƃɂ���
            ItemManager.Instance.GetItemRequest(GetComponent<Item>(), CharacterManager.Instance.PlayerObjSelf);
        }
    }

    public override void OnGetItem(bool isSelfAcquired)
    {
        Instantiate(openObj, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);//openObj�𐶐�
        
        Destroy(gameObject); //�f�[�^�{�b�N�X������
    }

    /// <summary>
    /// �f�[�^�{�b�N�X�J������
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {//�v���C���[���f�[�^�{�b�N�X�ɐG�ꂽ��
            ItemManager.Instance.GetItemRequest(GetComponent<Item>(), collision.gameObject);
            isTouch = true; //�f�[�^�{�b�N�X�ɐG�ꂽ���Ƃɂ���
        }
    }
}
