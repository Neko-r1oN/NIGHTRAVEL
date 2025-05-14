using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class TestCharacter : MonoBehaviour
{
    //テスト用キャラクターのスクリプト
    //Aouther:Yuki Miura(y-miura)

    // オブジェクト・コンポーネント参照
    private Rigidbody2D rigidbody2D; // Rigidbody2Dコンポーネントへの参照

    // 仮で変数定義
    public float HP; //キャラクターのHP(奈落などのステージギミックでHPを減らす)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // コンポーネント参照取得
        rigidbody2D = GetComponent<Rigidbody2D>();

        //HP設定
        HP = 5000.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // 仮の移動処理
        if (Input.GetKey(KeyCode.D))
        {// 右方向の移動入力
            Vector2 pos = transform.position;
            pos.x += 0.05f;
            transform.position = pos;
        }
        else if (Input.GetKey(KeyCode.A))
        {// 左方向の移動入力
            Vector2 pos = transform.position;
            pos.x -= 0.05f;
            transform.position = pos;
        }

        // ジャンプ操作
        if (Input.GetKeyDown(KeyCode.Space))
        {// ジャンプ開始
         // ジャンプ力を計算
            float jumpPower = 10.0f;
            // ジャンプ力を適用
            rigidbody2D.linearVelocity = new Vector2(rigidbody2D.linearVelocity.x, jumpPower);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Abyss"))
        {//「Abyss」タグがついたオブジェクトに触れたら

            float HP2 = HP * 0.3f;
            HP -= HP2;

            Debug.Log("HPが" + HP + "になった");
            Debug.Log("HP2は" + HP2);

            if(HP<=1)
            {
                HP = 1;
                Debug.Log("HPが" + HP + "になった");
            }
        }

        if (collision.gameObject.CompareTag("Short circuit"))
        {//「Short circuit」タグが付いたオブジェクトに触れたら
            HP -= 50;
            Debug.Log("HPが" + HP + "になった");

            if(HP < 0)
            {
                HP = 0;
                Debug.Log("死亡");
            }
        }
    }
}