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

		// �L�����̈ړ�
		horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

		if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
		{	// �W�����v������
			jump = true;
		}

		if (Input.GetKeyDown(KeyCode.C ) || Input.GetButtonDown("Blink"))
		{	// �u�����N������
			dash = true;
		}
	}

	/// <summary>
	/// ��������
	/// </summary>
	public void OnFall()
	{
		animator.SetBool("IsJumping", true);
	}

	/// <summary>
	/// ���n����
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
