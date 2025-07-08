//========================
//壊れるオブジェクトの親クラス
//Author:y-Miura
//date:2025/03/17
//========================

using UnityEngine;

abstract public class ObjectBase : MonoBehaviour
{
    /// <summary>
    /// 壊れるオブジェクトのダメージ関数
    /// </summary>
    abstract public void ApplyDamage();

    /// <summary>
    /// 壊れるオブジェクトの破片をフェードする関数
    /// </summary>
    /// <param name="fragment">破片</param>
    abstract public void FadeFragment(Transform fragment);
}
