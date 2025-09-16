using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CameraFollow : MonoBehaviour
{
	public float FollowSpeed = 2f;
	public Transform Target;

    Transform player; // カメラが追従するプレイヤーのTransform
    public Vector2 minCameraBounds; // ステージの左下(最小)座標
    public Vector2 maxCameraBounds; // ステージの右上(最大)座標

    // Transform of the camera to shake. Grabs the Object's transform
    // if null.
    private Transform camTransform;

	// How long the object should shake for.
	public float shakeDuration = 0f;

	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.1f;
	public float decreaseFactor = 1.0f;

	Vector3 originalPos;

	void Awake()
	{
		//Cursor.visible = false;	// リリース時にコメントアウトを外しましょう : by.enomoto
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
		}
	}

	void OnEnable()
	{
		originalPos = camTransform.localPosition;
	}

	private void FixedUpdate()
	{
		if (Target == null) return;

		Vector3 newPosition = Target.position;
		newPosition.z = -12;
		transform.position = Vector3.Slerp(transform.position, newPosition, FollowSpeed * Time.deltaTime);

		if (shakeDuration > 0)
		{
			camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
	}

	public void ShakeCamera()
	{
		originalPos = camTransform.localPosition;
		shakeDuration = 0.2f;
	}

    private void LateUpdate()
    {
        Vector3 newPosition = Target.position;

        // カメラの位置をステージの範囲内に制限
        newPosition.x = Mathf.Clamp(newPosition.x, minCameraBounds.x, maxCameraBounds.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minCameraBounds.y, maxCameraBounds.y);
		newPosition.z = -10f;

        // カメラの位置を更新
        transform.position = newPosition;
    }
}
