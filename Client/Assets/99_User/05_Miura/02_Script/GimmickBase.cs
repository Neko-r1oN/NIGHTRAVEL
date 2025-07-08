using UnityEngine;

abstract public class GimmickBase : MonoBehaviour
{
    // ギミックの起動
    abstract public void TurnOnPower(int triggerID);
    // サーバーに対して、ギミックの起動をリクエストする
    abstract public void TruggerRequest();

}
