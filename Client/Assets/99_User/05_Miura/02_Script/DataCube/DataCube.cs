//========================
// データキューブに関する処理
// Aouther:y-miura
// Date:2025/08/06
//========================

using Unity.VisualScripting;
using UnityEngine;

class DataCube : Item
{
    [SerializeField] GameObject seObj;

    //プレイヤーがデータキューブに触れたら
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            Instantiate(seObj, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation); // seObjを生成
            ItemManager.Instance.GetItemRequest(GetComponent<Item>(), collision.gameObject);
        }
    }
}
