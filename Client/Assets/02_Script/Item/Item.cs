using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Item : MonoBehaviour
{
    // アイテムの種類
    [SerializeField] EnumManager.ITEM_TYPE itemType;
    public EnumManager.ITEM_TYPE ItemType { get { return itemType; } }

    // 識別用ID
    string uniqueId;
    public string UniqueId { get { return uniqueId; } set { uniqueId = value; } }

    /// <summary>
    /// アイテム獲得時の処理
    /// </summary>
    public virtual void OnGetItem()
    {
        // 獲得時のパーティクル生成
        // Instantiate();
        Destroy(gameObject);
    }
}
