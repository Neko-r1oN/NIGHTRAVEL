//==============================
// データボックスのスクリプト
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
        //SpriteRedererコンポーネントを取得
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーがデータボックスを調べたら
        if (isTouch == true && Input.GetKey(KeyCode.E) && isOpened == false)
        {
            isOpened = true;
            ItemManager.Instance.GetItemRequest(GetComponent<Item>(), CharacterManager.Instance.PlayerObjSelf);
        }
    }

    public override void OnGetItem(bool isSelfAcquired)
    {
        Instantiate(openObj, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isTouch= true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isTouch = false;
        }
    }
}
