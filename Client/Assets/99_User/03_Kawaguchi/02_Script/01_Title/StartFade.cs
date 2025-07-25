using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;  //DOTweenを使うときはこのusingを入れる


public class StartFade : MonoBehaviour
{
    [SerializeField] Renderer fade;

    [Header("フェード関連")]
    [SerializeField] float startFade;
    [SerializeField] float fadeNum;
    float time = 0;

    [Header("ループ開始時の色")]
    [SerializeField]
    Color32 startColor = new Color32(255, 255, 255, 255);
    

    // Start is called before the first frame update
    void Start()
    {
        fade = GetComponent<Renderer>();

        fade.material.color = startColor;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (startFade < time) fade.material.color = Color.Lerp(fade.material.color, new Color(1, 1, 1.0f, 0), fadeNum * Time.deltaTime);
    }
}
