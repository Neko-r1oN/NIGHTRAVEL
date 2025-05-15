using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

	public PlayerController2D controller;
	public Animator animator;

	public float runSpeed = 40f;

	float horizontalMove = 0f;
	bool jump = false;
	bool dash = false;
	
	void Update () {

		// キャラの移動
		horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

		if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
		{	// ジャンプ押下時
			jump = true;
		}

		if (Input.GetKeyDown(KeyCode.C ) || Input.GetButtonDown("Blink"))
		{	// ブリンク押下時
			dash = true;
		}
	}

	/// <summary>
	/// 落下処理
	/// </summary>
	public void OnFall()
	{
		animator.SetBool("IsJumping", true);
	}

	/// <summary>
	/// 着地処理
	/// </summary>
	public void OnLanding()
	{
		animator.SetBool("IsJumping", false);
	}

	void FixedUpdate ()
	{
		// Move our character
		controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
		jump = false;
		dash = false;
	}
}
