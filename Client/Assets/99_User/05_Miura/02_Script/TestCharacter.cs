using UnityEngine;
using UnityEngine.Audio;

public class TestCharacter : MonoBehaviour
{
    //テスト用キャラクターのスクリプト
    //Aouther:Yuki Miura(y-miura)

    // オブジェクト・コンポーネント参照
    private Rigidbody2D rigidbody2D; // Rigidbody2Dコンポーネントへの参照

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // コンポーネント参照取得
        rigidbody2D = GetComponent<Rigidbody2D>();
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
            float jumpPower = 15.0f;
            // ジャンプ力を適用
            rigidbody2D.linearVelocity = new Vector2(rigidbody2D.linearVelocity.x, jumpPower);
        }
    }
}