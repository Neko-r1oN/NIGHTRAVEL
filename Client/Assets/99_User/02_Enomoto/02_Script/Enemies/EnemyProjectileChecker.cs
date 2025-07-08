//**************************************************
//  エネミーの発射物に関する判定を担当するクラス
//  Author:r-enomoto
//**************************************************
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class EnemyProjectileChecker : MonoBehaviour
{
    [SerializeField] Transform aimTransform;
    [SerializeField] Transform leftFireRayPoint;
    [SerializeField] Transform rightFireRayPoint;
    LayerMask targetLayerMask;

    [Range(0f, 180f)]
    [SerializeField] float maxAddAngleLeft;    // 回転角度の可動域(左回り)
    [Range(0f, 180f)]
    [SerializeField] float maxAddAngleRight;   // 回転角度の可動域(右回り)
    [Range(0f, 360f)]
    [SerializeField] float initialAngle;   // 可動域を加算する基準値

    private void Start()
    {
        // 視認するLayer [プレイヤー、地面・壁]
        targetLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player");
        aimTransform.eulerAngles = new Vector3(0, 0, initialAngle);
    }

    /// <summary>
    /// ターゲット方向への回転角度を、ある制限範囲内で計算して返す
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float ClampAngleToTarget(Vector3 direction)
    {
        // テクスチャが反転していたら、角度も反転させる
        float directionMultiplier = TransformUtils.GetFacingDirection(transform);
        float maxAddAngleLeft = directionMultiplier == 1 ? this.maxAddAngleLeft : this.maxAddAngleRight;
        float maxAddAngleRight = directionMultiplier == 1 ? this.maxAddAngleRight : this.maxAddAngleLeft;
        float initialAngle = this.initialAngle * directionMultiplier;

        // 方向からターゲットのラジアンを計算して度に変換する
        float targetAngle = Mathf.Atan2(direction.y, direction.x);
        // 角度が真上のときに0度とした角度に修正し(Unityは上向きが0度となるため)、-90で反時計回りで角度が増加するようにする
        targetAngle = targetAngle * Mathf.Rad2Deg - 90;

        // 初期角度との角度差を求め、制限角度内に収める
        float deltaAngle = Mathf.DeltaAngle(initialAngle, targetAngle);
        float clampedDelta = Mathf.Clamp(deltaAngle, -maxAddAngleLeft, maxAddAngleRight);
        return initialAngle + clampedDelta;
    }

    /// <summary>
    /// 発射物のRaycastHit2Dを生成する
    /// </summary>
    /// <returns></returns>
    (RaycastHit2D canterHit2D, RaycastHit2D leftHit2D, RaycastHit2D rightHit2D) GetProjectileRaycastHit(Transform target, float gunBulletWidth, bool followTargetRotation, float? angle, bool canUpdateRotation = true)
    {
        float attackDist = GetComponent<EnemyBase>().AttackDist;

        if (followTargetRotation && canUpdateRotation)
        {
            // ターゲットのいる方向
            Vector2 direction = (target.transform.position - aimTransform.position).normalized;
            aimTransform.rotation = Quaternion.Euler(0f, 0f, ClampAngleToTarget(direction));
        }
        else if (angle != null && canUpdateRotation)
        {
            // 指定された角度に設定する
            aimTransform.localRotation = Quaternion.Euler(0, 0, (float)angle);
        }
        Vector2 rayDirection = aimTransform.up; // aimTransformが向いている方向

        if (canUpdateRotation)
        {
            // 発射物の幅に合わせて、各Rayの始点を調整する
            leftFireRayPoint.localPosition = Vector2.left * gunBulletWidth;
            rightFireRayPoint.localPosition = Vector2.right * gunBulletWidth;
        }

        // RaycastHit2Dをターゲットのいる方向に伸ばす
        RaycastHit2D canterHit2D = Physics2D.Raycast(aimTransform.position, rayDirection, attackDist, targetLayerMask);
        RaycastHit2D leftHit2D = Physics2D.Raycast(leftFireRayPoint.position, rayDirection, attackDist, targetLayerMask);
        RaycastHit2D rightHit2D = Physics2D.Raycast(rightFireRayPoint.position, rayDirection, attackDist, targetLayerMask);
        return (canterHit2D, leftHit2D, rightHit2D);
    }

    /// <summary>
    /// [角度の追従] 発射物を飛ばすことができるかどうか
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public bool CanFireProjectile(float gunBulletWidth, bool followTargetRotation = false)
    {
        return IsProjectileFireable(gunBulletWidth, followTargetRotation);
    }

    /// <summary>
    /// [角度の指定] 発射物を飛ばすことができるかどうか
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public bool CanFireProjectile(float gunBulletWidth, float? angle = null)
    {
        return IsProjectileFireable(gunBulletWidth, angle: angle);
    }

    /// <summary>
    /// 発射物を飛ばすことができるか評価
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    bool IsProjectileFireable(float gunBulletWidth, bool followTargetRotation = false, float? angle = null)
    {
        GameObject target = GetComponent<EnemyBase>().Target;
        if (!target) return false;

        var projectileRays = GetProjectileRaycastHit(target.transform, gunBulletWidth, followTargetRotation, angle);

        // プレイヤーにRayが当たっているか、何も検知ができなかった場合はtrue
        bool resultCenter = projectileRays.canterHit2D && projectileRays.canterHit2D.collider.gameObject.CompareTag("Player");
        bool resultLeft = projectileRays.leftHit2D && projectileRays.leftHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.leftHit2D;
        bool resultRight = projectileRays.rightHit2D && projectileRays.rightHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.rightHit2D;
        return resultCenter && resultLeft && resultRight;
    }

    /// <summary>
    /// [デバック用:角度の追従] 発射物の射線を描画する
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(float gunBulletWidth, bool followTargetRotation = false)
    {
        DrawProjectileRay(gunBulletWidth, followTargetRotation);
    }

    /// <summary>
    /// [デバック用:角度の指定] 発射物の射線を描画する
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(float gunBulletWidth, float? angle = null)
    {
        DrawProjectileRay(gunBulletWidth, angle: angle);
    }

    /// <summary>
    /// [Debug用] 発射物の射線を描画する
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    void DrawProjectileRay(float gunBulletWidth, bool followTargetRotation = false, float? angle = null)
    {
        GameObject target = GetComponent<EnemyBase>().Target;
        if (!target) return;

        var projectileRays = GetProjectileRaycastHit(target.transform, gunBulletWidth, followTargetRotation, angle, false);

        // デバック用
        Color rayUpColor = Color.red;
        Color rayDownColor = Color.red;
        if (projectileRays.leftHit2D && !projectileRays.leftHit2D.collider.gameObject.CompareTag("Player"))
        {
            rayUpColor = Color.gray;
        }
        if (projectileRays.rightHit2D && !projectileRays.rightHit2D.collider.gameObject.CompareTag("Player"))
        {
            rayDownColor = Color.gray;
        }

        Vector3 rayDirection = aimTransform.up;
        float attackDist = GetComponent<EnemyBase>().AttackDist;
        Debug.DrawRay(aimTransform.position, rayDirection * attackDist, rayUpColor);
        Debug.DrawRay(leftFireRayPoint.position, rayDirection * attackDist, rayUpColor);
        Debug.DrawRay(rightFireRayPoint.position, rayDirection * attackDist, rayDownColor);
    }

    public Vector2 GetDirection(float initialAngle, float angle)
    {
        // 真上を0度にし、基準の値を加算
        float angleInRadians = (angle + 90 + initialAngle) * Mathf.Deg2Rad;
        // 角度を使って向きを生成
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }

    /// <summary>
    /// [デバック用] 射線の可動域を描画する
    /// </summary>
    private void OnDrawGizmos()
    {
        float initialAngle;
        initialAngle = this.initialAngle;

        // テクスチャが反転していたら、角度も反転させる
        float directionMultiplier = TransformUtils.GetFacingDirection(transform);
        float maxAngleLeft = directionMultiplier == 1 ? this.maxAddAngleLeft : this.maxAddAngleRight;
        float maxAngleRight = directionMultiplier == 1 ? this.maxAddAngleRight : this.maxAddAngleLeft;
        initialAngle *= directionMultiplier;

        float attackDist = GetComponent<EnemyBase>().AttackDist;
        Debug.DrawRay(aimTransform.position, GetDirection(initialAngle, -maxAngleLeft).normalized * attackDist, Color.red);
        Debug.DrawRay(aimTransform.position, GetDirection(initialAngle, maxAngleRight).normalized * attackDist, Color.red);
    }
}
