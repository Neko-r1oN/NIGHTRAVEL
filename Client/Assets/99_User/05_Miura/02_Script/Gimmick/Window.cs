//====================
// 送風ファンのスクリプト
// Aouther:y-miura
// Date:2025/07/08
//====================

using System.Collections;
using UnityEngine;

public class Window : GimmickBase
{
    [SerializeField] GameObject windObj;
    bool isWind;

    public enum Power_ID
    {
        ON = 0,
        OFF
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnOnPower(1);
        InvokeRepeating("SendWind", 0.1f, 5);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 風を流したり止めたりする処理
    /// </summary>
    public void SendWind()
    {
        if(isWind==true)
        {
            windObj.SetActive(false);
            isWind=false;
        }
        else if(isWind==false)
        { 
            windObj.SetActive(true);
            isWind=true;
        }
    }

    public override void TurnOnPower(int triggerID)
    {
        switch (triggerID)
        {
            case 0:
                windObj.SetActive(true);
                break;

            case 1:
                windObj.SetActive(false);
                break;

            default:
                break;
        }
    }

    public override void TruggerRequest()
    {

    }
}
