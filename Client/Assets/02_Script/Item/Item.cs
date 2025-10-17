using KanKikuchi.AudioManager;
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
    /// <param name="isSelfAcquired">自身が回収したのかどうか</param>
    public virtual void OnGetItem(bool isSelfAcquired)
    {
        // 獲得時のパーティクル生成
        // Instantiate();
        Destroy(gameObject);

        SEManager.Instance.Play(
               audioPath: SEPath.RERIKKU_GET, //再生したいオーディオのパス
               volumeRate: 1.0f,                //音量の倍率
               delay: 0.0f,                //再生されるまでの遅延時間
               pitch: 1.0f,                //ピッチ
               isLoop: false,             //ループ再生するか
               callback: null              //再生終了後の処理
               );
    }
}
