//**************************************************
//  エネミーの発射物に関する判定を管理するクラス
//  Author:r-enomoto
//**************************************************
using System.Collections.Generic;
using UnityEngine;


public class EnemyProjectileChecker : MonoBehaviour
{
    [SerializeField] Transform enemy;   // 本体

    [SerializeField] Transform aimTransform;
    [SerializeField] Transform leftFireRayPoint;
    [SerializeField] Transform rightFireRayPoint;
    [SerializeField] float gunBulletWidth;
    [SerializeField] float rotetionSpeed;
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
    /// AIMの回転を更新
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="rotetionSpeed"></param>
    public void RotateAimTransform(Vector3 direction, float rotetionSpeed = 0)
    {
        if (rotetionSpeed == 0) rotetionSpeed = this.rotetionSpeed;
        Quaternion quaternion = Quaternion.Euler(0, 0, ClampAngleToTarget(direction));
        aimTransform.rotation = Quaternion.RotateTowards(aimTransform.rotation, quaternion, rotetionSpeed);

        // 発射物の幅に合わせて、各Rayの始点を調整する
        leftFireRayPoint.localPosition = Vector2.left * gunBulletWidth;
        rightFireRayPoint.localPosition = Vector2.right * gunBulletWidth;
    }

    /// <summary>
    /// ターゲット方向への回転角度を、ある制限範囲内で計算して返す
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float ClampAngleToTarget(Vector3 direction)
    {
        var angleRange = GetAdjustedAngleRangeByFacing();

        // 方向からターゲットのラジアンを計算して度に変換する
        float targetAngle = Mathf.Atan2(direction.y, direction.x);
        // 角度が真上のときに0度とした角度に修正し(Unityは上向きが0度となるため)、-90で反時計回りで角度が増加するようにする
        targetAngle = targetAngle * Mathf.Rad2Deg - 90;

        // 初期角度との角度差を求め、制限角度内に収める
        float deltaAngle = Mathf.DeltaAngle(angleRange.initialAngle, targetAngle);
        float clampedDelta = Mathf.Clamp(deltaAngle, -angleRange.maxAddAngleLeft, angleRange.maxAddAngleRight);
        return angleRange.initialAngle + clampedDelta;
    }

    /// <summary>
    /// 発射物のRaycastHit2Dを生成する
    /// </summary>
    /// <param name="target"></param>
    /// <param name="gunBulletWidth"></param>
    /// <param name="canFollowTargetSimulation">rayの向きを、targetに向けてシュミレーションした角度にするかどうか</param>
    /// <param name="angle"></param>
    /// <param name="canUpdateRotation"></param>
    /// <returns></returns>
    (RaycastHit2D canterHit2D, RaycastHit2D leftHit2D, RaycastHit2D rightHit2D) GetProjectileRaycastHit(Transform target, bool canFollowTargetSimulation, float? angle)
    {
        float attackDist = enemy.GetComponent<EnemyBase>().AttackDist;

        Quaternion simulatedRotation = aimTransform.rotation;
        if (canFollowTargetSimulation)
        {
            // ターゲットのいる方向
            Vector2 direction = (target.transform.position - aimTransform.position).normalized;
            simulatedRotation = Quaternion.Euler(0f, 0f, ClampAngleToTarget(direction));
        }
        else if (angle != null)
        {
            // 指定された角度に設定する
            simulatedRotation = Quaternion.Euler(0, 0, (float)angle);
        }
        Vector2 rayDirection = simulatedRotation * Vector2.up; // aimTransformが本来向く方向

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
    public bool CanFireProjectile(GameObject target, bool canFollowTargetSimulation = true)
    {
        return IsProjectileFireable(target, canFollowTargetSimulation);
    }

    /// <summary>
    /// [角度の指定] 発射物を飛ばすことができるかどうか
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public bool CanFireProjectile(GameObject target, float? angle)
    {
        return IsProjectileFireable(target, angle: angle);
    }

    /// <summary>
    /// [射線上をチェック] 発射物を飛ばすことができるか評価
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="canFollowTargetSimulation"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    bool IsProjectileFireable(GameObject target, bool canFollowTargetSimulation = true, float? angle = null)
    {
        if (!target) return false;
        var projectileRays = GetProjectileRaycastHit(target.transform, canFollowTargetSimulation, angle);

        // プレイヤーにRayが当たっているか、何も検知ができなかった場合はtrue
        bool resultCenter = projectileRays.canterHit2D && projectileRays.canterHit2D.collider.gameObject.CompareTag("Player");
        bool resultLeft = projectileRays.leftHit2D && projectileRays.leftHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.leftHit2D;
        bool resultRight = projectileRays.rightHit2D && projectileRays.rightHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.rightHit2D;
        return resultCenter && resultLeft && resultRight;
    }

    /// <summary>
    /// 射線の可動域範囲内にターゲットがいるかどうか
    /// </summary>
    /// <returns></returns>
    public bool IsTargetInSight(GameObject target)
    {
        var projectileRays = GetProjectileRaycastHit(target.transform, false, null);

        // プレイヤーにRayが当たっているか、何も検知ができなかった場合はtrue
        bool resultCenter = projectileRays.canterHit2D && projectileRays.canterHit2D.collider.gameObject.CompareTag("Player");
        return resultCenter;
    }

    /// <summary>
    /// 射線の可動域範囲内で最も近いプレイヤーの取得
    /// </summary>
    /// <returns></returns>
    public GameObject GetNearPlayerInSight(List<GameObject> players, bool canFollowTargetSimulation = true)
    {
        float distToPlayer = float.MaxValue;
        GameObject nearPlayer = null;
        foreach (var player in players)
        {
            if (!player || player && player.GetComponent<CharacterBase>().HP <= 0) continue;
            var projectileRays = GetProjectileRaycastHit(player.transform, canFollowTargetSimulation, null);

            // プレイヤーにRayが当たっているか、何も検知ができなかった場合はtrue
            bool resultCenter = projectileRays.canterHit2D && projectileRays.canterHit2D.collider.gameObject.CompareTag("Player");
            bool resultLeft = projectileRays.leftHit2D && projectileRays.leftHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.leftHit2D;
            bool resultRight = projectileRays.rightHit2D && projectileRays.rightHit2D.collider.gameObject.CompareTag("Player") || !projectileRays.rightHit2D;

            // 射線の可動域範囲内 && より距離が近い場合
            if (resultCenter && resultLeft && resultRight && 
                distToPlayer > Vector2.Distance(player.transform.position, transform.position))
            {
                distToPlayer = Vector2.Distance(player.transform.position, transform.position);
                nearPlayer = player;
            }
        }
        return nearPlayer;
    }

    /// <summary>
    /// [デバック用:角度をターゲットに向かって追従] 発射物の射線を描画する
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(GameObject target, bool canFollowTargetSimulation = true)
    {
        DrawProjectileRay(target, canFollowTargetSimulation);
    }

    /// <summary>
    /// [デバック用:角度の指定] 発射物の射線を描画する
    /// </summary>
    /// <param name="projectile"></param>
    public void DrawProjectileRayGizmo(GameObject target, float? angle)
    {
        DrawProjectileRay(target, angle: angle);
    }

    /// <summary>
    /// [Debug用] 発射物の射線を描画する
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="followTargetRotation"></param>
    /// <param name="angle"></param>
    void DrawProjectileRay(GameObject target, bool canFollowTargetSimulation = true, float? angle = null)
    {
        if (!target) return;
        var projectileRays = GetProjectileRaycastHit(target.transform, canFollowTargetSimulation, angle);

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
        float attackDist = enemy.GetComponent<EnemyBase>().AttackDist;
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
    /// 向きに応じて調整された射線の可動域（基準 , 左右のmax）を取得
    /// </summary>
    (float maxAddAngleLeft, float maxAddAngleRight, float initialAngle) GetAdjustedAngleRangeByFacing()
    {
        // 本体のテクスチャが反転していたら、角度も反転させる
        float directionMultiplier = TransformUtils.GetFacingDirection(enemy);
        float maxAddAngleLeft = directionMultiplier == 1 ? this.maxAddAngleLeft : this.maxAddAngleRight;
        float maxAddAngleRight = directionMultiplier == 1 ? this.maxAddAngleRight : this.maxAddAngleLeft;
        float initialAngle = this.initialAngle * directionMultiplier + enemy.transform.eulerAngles.z;  // 本体が向いているワールド角度を考慮
        return (maxAddAngleLeft, maxAddAngleRight, initialAngle);
    }

    /// <summary>
    /// [デバック用] 射線の可動域を描画する
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enemy || enemy && !enemy.GetComponent<EnemyBase>().CanDrawRay) return;

        var adjustedAngleRange = GetAdjustedAngleRangeByFacing();
        float attackDist = enemy.GetComponent<EnemyBase>().AttackDist;
        Debug.DrawRay(aimTransform.position, GetDirection(adjustedAngleRange.initialAngle, -adjustedAngleRange.maxAddAngleLeft).normalized * attackDist, Color.red);
        Debug.DrawRay(aimTransform.position, GetDirection(adjustedAngleRange.initialAngle, adjustedAngleRange.maxAddAngleRight).normalized * attackDist, Color.red);
    }
}
