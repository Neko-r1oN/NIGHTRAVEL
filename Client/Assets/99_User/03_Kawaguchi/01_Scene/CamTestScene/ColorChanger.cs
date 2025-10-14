using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using Pixeye.Unity;

/// <summary>
/// �{�X���j���o
/// </summary>
public class ColorChanger : MonoBehaviour
{
    private float start_TimeScale = 1.0f;   //�i�s����(�����l)

    [SerializeField, Header("�i�s����")] float goal_TimeScale = 0.1f;   //�i�s����(�ŏ��l)

    private float start_fixedDeltaTime = 0.02f;  //���Z����(�����l)
    [SerializeField, Header("���Z����")] float goal_fixedDeltaTime = 0.002f; //���Z����(�ŏ��l)

    private float start_Exposure = 0.0f;  //�ʓx���l(�����l)
    [SerializeField, Header("�ʓx���l")] float goal_Exposure = 1.4f;  //�ʓx���l(�ő�l)

    private float start_Monochrome = 0.0f;  //���Ð��l(�����l)
    [SerializeField, Header("���Ð��l")] float goal_Monochrome = -100.0f;    //���Ð��l(�ŏ��l)
    [Space(50)]

    [SerializeField] float changeTime;
    [SerializeField] float stopTime;

    [SerializeField] Volume volume;
    ColorAdjustments colorAdjustments;

    void Start()
    {
        volume.profile.TryGet(out colorAdjustments);
        if (colorAdjustments == null) Debug.Log("���m�N���ŋ�");

        //StartCoroutine("ColorChange");
        //StartSlow();
    }

    /// <summary>
    /// �J���[�ύX�֐�
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
        Debug.Log("for�I������^");
    }

    /// <summary>
    /// �J���[���Z�b�g�֐�
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
        Debug.Log("back�I������^");
    }

    /// <summary>
    /// �J���[�Z�b�g�֐�
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
