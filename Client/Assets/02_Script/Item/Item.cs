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
    }
}
