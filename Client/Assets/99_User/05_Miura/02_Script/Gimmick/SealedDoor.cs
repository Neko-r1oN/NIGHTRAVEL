using UnityEngine;

public class SealedDoor : ObjectBase
{
    [SerializeField] GameObject DoorFragment;　//破片エフェクトを取得
    bool isBroken = false;

    public enum Power_ID
    {
        ON = 0,
        OFF
    };

    /// <summary>
    /// 電源オン処理
    /// </summary>
    public override void TurnOnPower()
    {
        // ダメージ付与
        ApplyDamage(DoorFragment, isBroken, new Vector2(0f, -1.5f));
        isBroken = true; // 破壊済みとする
    }
}
