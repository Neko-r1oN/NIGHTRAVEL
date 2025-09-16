//========================
//壊れるオブジェクトの親クラス
//Author:y-Miura
//date:2025/03/17
//========================

using DG.Tweening;
using Rewired;
using UnityEngine;
using UnityEngine.UIElements;

abstract public class ObjectBase : GimmickBase
{
    /// <summary>
    /// 壊れるオブジェクトのダメージ関数
    /// </summary>
    abstract protected void ApplyDamage ();

    abstract public void DestroyFragment(Transform obj);

    /// <summary>
    /// 壊れるオブジェクトの破片をフェードする関数
    /// </summary>
    /// <param name="fragment">破片</param>
    abstract public void FadeFragment(Transform fragment);
}
