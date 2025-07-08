//
//壊れるオブジェクトの親クラス
//Author:y-Miura
//date:2025/03/17
//

using UnityEngine;

abstract public class ObjectBase : GimmickBase
{
    /// <summary>
    /// 壊れるオブジェクトのダメージ関数
    /// </summary>
    abstract public void ApplyDamage();

    abstract public void FadeFragment(Transform fragment);
}
