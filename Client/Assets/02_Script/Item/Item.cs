using KanKikuchi.AudioManager;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Item : MonoBehaviour
{
    // �A�C�e���̎��
    [SerializeField] EnumManager.ITEM_TYPE itemType;
    public EnumManager.ITEM_TYPE ItemType { get { return itemType; } }

    // ���ʗpID
    string uniqueId;
    public string UniqueId { get { return uniqueId; } set { uniqueId = value; } }

    /// <summary>
    /// �A�C�e���l�����̏���
    /// </summary>
    /// <param name="isSelfAcquired">���g����������̂��ǂ���</param>
    public virtual void OnGetItem(bool isSelfAcquired)
    {
        // �l�����̃p�[�e�B�N������
        // Instantiate();
        Destroy(gameObject);

        SEManager.Instance.Play(
               audioPath: SEPath.RERIKKU_GET, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
    }
}
