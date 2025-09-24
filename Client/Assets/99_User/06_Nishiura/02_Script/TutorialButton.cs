//===================
// チュートリアルUIボタンスクリプト
// Author:Nishiura
// Date:2025/09/24
//===================
using DG.Tweening;
using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<Renderer>().material.color = new Color(255, 255, 255, 0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            this.GetComponent<Renderer>().material.DOFade(1, 0.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            this.GetComponent<Renderer>().material.DOFade(0, 0.5f);
        }
    }
}
