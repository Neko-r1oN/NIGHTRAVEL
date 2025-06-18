using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    PlayerBase playerBase;
    SteelDoor steelDoor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//プレイヤーが鋼鉄製ドアのセンサーに触れたら
            steelDoor = GameObject.FindWithTag("SteelDoor").GetComponent<SteelDoor>();
            playerBase = collision.GetComponent<PlayerBase>();

            //SteelDoorクラスのUp関数を呼び出す(ドアが上に移動する)
            steelDoor.Up();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {//プレイヤーが鋼鉄製ドアのセンサーから離れたら
            steelDoor = GameObject.FindWithTag("SteelDoor").GetComponent<SteelDoor>();
            playerBase = collision.GetComponent<PlayerBase>();

            //SteelDoorクラスのDown関数を呼び出す(ドアの位置をもとに戻す)
            steelDoor.Down();
        }
    }
}
