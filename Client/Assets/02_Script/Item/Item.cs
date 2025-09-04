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

    private void Start()
    {
        
    }

    /// <summary>
    /// �A�C�e���l�����̏���
    /// </summary>
    public void OnGetItem()
    {
        // �l�����̃p�[�e�B�N������
        // Instantiate();
        Destroy(gameObject);
    }
}
