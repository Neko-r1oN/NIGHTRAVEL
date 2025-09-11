using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LoadingText : MonoBehaviour
{
    private float _repeatSpan;    //繰り返す間隔
    private float _timeElapsed;   //経過時間

    // Start is called before the first frame update
    void Start()
    {
        //表示切り替え時間を指定
        _repeatSpan = 0.5f;
        _timeElapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _timeElapsed += Time.deltaTime;     //時間をカウントする

        if (_timeElapsed >= _repeatSpan)
        {//時間経過でテキスト表示
            GetComponent<Text>().text = "Now Loading";
        }
        if (_timeElapsed >= _repeatSpan + 0.5f)
        {//時間経過でテキスト表示(役職)
            GetComponent<Text>().text = "Now Loading.";
        }
        if (_timeElapsed >= _repeatSpan + 1.0f)
        {//時間経過でテキスト表示(役職)
            GetComponent<Text>().text = "Now Loading..";
        }
        if (_timeElapsed >= _repeatSpan + 1.5f)
        {//時間経過でテキスト表示(役職)
            GetComponent<Text>().text = "Now Loading...";
            _timeElapsed = 0;   //経過時間をリセットする
        }

    }
}
