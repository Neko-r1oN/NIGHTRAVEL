using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using Pixeye.Unity;

/// <summary>
/// ボス撃破演出
/// </summary>
public class ColorChanger : MonoBehaviour
{
    private float start_TimeScale = 1.0f;   //進行時間(初期値)

    [SerializeField, Header("進行時間")] float goal_TimeScale = 0.1f;   //進行時間(最小値)

    private float start_fixedDeltaTime = 0.02f;  //演算時間(初期値)
    [SerializeField, Header("演算時間")] float goal_fixedDeltaTime = 0.002f; //演算時間(最小値)

    private float start_Exposure = 0.0f;  //彩度数値(初期値)
    [SerializeField, Header("彩度数値")] float goal_Exposure = 1.4f;  //彩度数値(最大値)

    private float start_Monochrome = 0.0f;  //明暗数値(初期値)
    [SerializeField, Header("明暗数値")] float goal_Monochrome = -100.0f;    //明暗数値(最小値)
    [Space(50)]

    [SerializeField] float changeTime;
    [SerializeField] float stopTime;

    [SerializeField] Volume volume;
    ColorAdjustments colorAdjustments;

    void Start()
    {
        volume.profile.TryGet(out colorAdjustments);
        if (colorAdjustments == null) Debug.Log("モノクロで金");

        //StartCoroutine("ColorChange");
        //StartSlow();
    }

    /// <summary>
    /// カラー変更関数
    /// </summary>
    /// <returns></returns>
    public IEnumerator ColorChange()
    {
        for (float i = 0; i < changeTime; i += 0.1f)
        {
            
            Time.timeScale -= (goal_TimeScale / (changeTime * 10));
            Time.fixedDeltaTime -= (goal_fixedDeltaTime / (changeTime * 10));
            colorAdjustments.postExposure.value = colorAdjustments.postExposure.value + (goal_Exposure / (changeTime * 10));
            colorAdjustments.saturation.value = colorAdjustments.saturation.value + (goal_Monochrome / (changeTime * 10));

            yield return new WaitForSeconds(0.1f);
        }
        MeterSet(0);
        StartCoroutine("ColorBack");
        Debug.Log("for終わった与");
    }

    /// <summary>
    /// カラーリセット関数
    /// </summary>
    /// <returns></returns>
    IEnumerator ColorBack()
    {
        yield return new WaitForSeconds(stopTime);

        for (float i = 0; i < changeTime; i += 0.1f)
        {

            Time.timeScale += (start_TimeScale / (changeTime * 10));
            Time.fixedDeltaTime += (start_fixedDeltaTime / (changeTime * 10));
            colorAdjustments.postExposure.value -= (goal_Exposure / (changeTime * 10));
            colorAdjustments.saturation.value -= (goal_Monochrome / (changeTime * 10));

            yield return new WaitForSeconds(0.1f);
        }

        MeterSet(1);
        Debug.Log("back終わった与");
    }

    /// <summary>
    /// カラーセット関数
    /// </summary>
    /// <param name="i"></param>
    void MeterSet(int i)
    {
        if(i == 0)
        {
            Time.timeScale = goal_TimeScale;
            Time.fixedDeltaTime = goal_fixedDeltaTime;
            colorAdjustments.postExposure.value = goal_Exposure;
            colorAdjustments.saturation.value = goal_Monochrome;
        }else
        {
            Time.timeScale = start_TimeScale;
            Time.fixedDeltaTime = start_fixedDeltaTime;
            colorAdjustments.postExposure.value = start_Exposure;
            colorAdjustments.saturation.value = start_Monochrome;
            colorAdjustments.saturation.value = start_Monochrome;
        }
    }
}
