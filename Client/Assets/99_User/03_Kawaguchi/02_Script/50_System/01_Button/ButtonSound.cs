using UnityEngine;
using KanKikuchi.AudioManager;

public class ButtonSound : MonoBehaviour
{
    /// <summary>
    /// クリック音再生関数
    /// </summary>
    public void ForcusButton()
    {
        SEManager.Instance.Play(
            audioPath: SEPath.BUTTON_FOCUS, //再生したいオーディオのパス
            volumeRate: 1,                //音量の倍率
            delay: 0,                //再生されるまでの遅延時間
            pitch: 1,                //ピッチ
            isLoop: false,             //ループ再生するか
            callback: null              //再生終了後の処理
        );
    }

    /// <summary>
    /// クリック音再生関数
    /// </summary>
    public void PushButton()
    {
        SEManager.Instance.Play(
            audioPath: SEPath.BUTTON_PUSH, //再生したいオーディオのパス
            volumeRate: 1,                //音量の倍率
            delay: 0,                //再生されるまでの遅延時間
            pitch: 1,                //ピッチ
            isLoop: false,             //ループ再生するか
            callback: null              //再生終了後の処理
        );
    }


    public void ForcusPowerUpButton()
    {
        SEManager.Instance.Play(
            audioPath: SEPath.CURSOR_POWER_UP, //再生したいオーディオのパス
            volumeRate: 1,                //音量の倍率
            delay: 0,                //再生されるまでの遅延時間
            pitch: 1,                //ピッチ
            isLoop: false,             //ループ再生するか
            callback: null              //再生終了後の処理
        );
    }
    /// <summary>
    /// クリック音再生関数
    /// </summary>
    public void PushPowerUpButton()
    {
        SEManager.Instance.Play(
            audioPath: SEPath.BUTTON_PUSH, //再生したいオーディオのパス
            volumeRate: 1,                //音量の倍率
            delay: 0,                //再生されるまでの遅延時間
            pitch: 1,                //ピッチ
            isLoop: false,             //ループ再生するか
            callback: null              //再生終了後の処理
        );
        SEManager.Instance.Play(
            audioPath: SEPath.POWER_UP, //再生したいオーディオのパス
            volumeRate: 1,                //音量の倍率
            delay: 0.2f,                //再生されるまでの遅延時間
            pitch: 1,                //ピッチ
            isLoop: false,             //ループ再生するか
            callback: null              //再生終了後の処理
        );
    }
}
