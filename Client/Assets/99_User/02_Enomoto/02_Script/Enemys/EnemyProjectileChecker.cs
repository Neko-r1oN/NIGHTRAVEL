//**************************************************
//  エネミーの発射物に関する判定を担当するクラス
//  Author:r-enomoto
//**************************************************
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class EnemyProjectileChecker : MonoBehaviour
{
    [SerializeField] LayerMask targetLayerMask;     // 視認するLayer [プレイヤー、地面、壁]
    [SerializeField] Transform aimTransform;
    [SerializeField] Transform leftFireRayPoint;
    [SerializeField] Transform rightFireRayPoint;

    [Range(0f, 180f)]
    [SerializeField] float maxAddAngleLeft;    // 回転角度の可動域(左回り)
    [Range(0f, 180f)]
    [SerializeField] float maxAddAngleRight;   // 回転角度の可動域(右回り)
    [Range(0f, 360f)]
    [SerializeField] float initialAngle;   // 可動域を加算する基準値

    private void Start()
    {
        aimTransform.eulerAngles = new Vector3(0, 0, initialAngle);
    }

    /// <summary>
    /// ターゲット方向への回転角度を、ある制限範囲内で計算して返す
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    float ClampAngleToTarget(float initialAngle, Vector3 direction)
    {
        // テクスチャが反転していたら、角度も反転させる
        float directionMultiplier = Mathf.Clamp(transform.localScale.x, -1, 1);
        float maxAddAngleLeft = directionMultiplier == 1 ? this.maxAddAngleLeft : this.maxAddAngleRight;
        float maxAddAngleRight = directionMultiplier == 1 ? this.maxAddAngleRight : this.maxAddAngleLeft;
        initialAngle *= directionMultiplier;

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
    (RaycastHit2D canterHit2D, RaycastHit2D leftHit2D, RaycastHit2D rightHit2D) GetProjectileRaycastHit(Transform target, GameObject projectile, bool followTargetRotation, float? angle)
    {
        float attackDist = GetComponent<EnemyController>().AttackDist;
        float projectileHeight = projectile.GetComponent<SpriteRenderer>().bounds.size.y / 2;

        if (followTargetRotation)
        {
            // ターゲットのいる方向
            Vector2 direction = (target.transform.position - aimTransform.position).normalized;
            aimTransform.rotation = Quaternion.Euler(0f, 0f, ClampAngleToTarget(this.initialAngle, direction));
        }
        else if (angle != null)
        {
            // 指定された角度に設定する
            aimTransform.localRotation = Quaternion.Euler(0, 0, (float)angle);
        }
        Vector2 rayDirection = aimTransform.up; // aimTransformが向いている方向

        // 発射物の幅に合わせて、各Rayの始点を調整する
        leftFireRayPoint.localPosition = Vector2.left * projectileHeight;
        rightFireRayPoint.localPosition = Vector2.right * projectileHeight;

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
    public bool CanFireProjectile(GameObject projectile, bool followTargetRotation = false)
    {
        return IsProjectileFireable(projectile, followTargetRotation);
    }

    /// <summary>
    /// [角度の指定] 発射物を飛ばすことができるかどうか
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public bool CanFireProjectile(GameObject projectile, float? angle = null)
    {
        return IsProjectileFireable(projectile, angle: angle);
    }

    /// <summary>
    /// 発射物を飛ばすことができるか評価
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    bool IsProjectileFireable(GameObject projectile, bool followTargetRotation = false, float? angle = null)
    {
        GameObject target = GetComponent<EnemyController>().Target;
        if (!target || !projectile) return false;

        var projectileRays = GetProjectileRaycastHit(target.transform, projectile, followTargetRotation, angle);

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
    public void DrawProjectileRayGizmo(GameObject projectile, bool followTargetRotation = false)
    {
        DrawProjectileRay(projectile, followTargetRotation);
    }

    /// <summary>
    /// [デバック用:角度の指定] 発射物の射線を描画する
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(GameObject projectile, float? angle = null)
    {
        DrawProjectileRay(projectile, angle: angle);
    }

    /// <summary>
    /// 発射物の射線を描画する
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    void DrawProjectileRay(GameObject projectile, bool followTargetRotation = false, float? angle = null)
    {
        float projectileHeight = projectile.GetComponent<SpriteRenderer>().bounds.size.y / 2;
        GameObject target = GetComponent<EnemyController>().Target;
        if (!target) return;

        var projectileRays = GetProjectileRaycastHit(target.transform, projectile, followTargetRotation, angle);

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
        float attackDist = GetComponent<EnemyController>().AttackDist;
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
        float directionMultiplier = Mathf.Clamp(transform.localScale.x, -1, 1);
        float maxAngleLeft = directionMultiplier == 1 ? this.maxAddAngleLeft : this.maxAddAngleRight;
        float maxAngleRight = directionMultiplier == 1 ? this.maxAddAngleRight : this.maxAddAngleLeft;
        initialAngle *= directionMultiplier;

        float attackDist = GetComponent<EnemyController>().AttackDist;
        Debug.DrawRay(aimTransform.position, GetDirection(initialAngle, -maxAngleLeft).normalized * attackDist, Color.red);
        Debug.DrawRay(aimTransform.position, GetDirection(initialAngle, maxAngleRight).normalized * attackDist, Color.red);
    }
}
