//--------------------------------------------------------------
// 剣士キャラ [ SampleChara.cs ]
// Author：Kenta Nakamoto
// 引用：https://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using Pixeye.Unity;
using System;
using HardLight2DUtil;

public class Sword : PlayerBase
{
    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // 攻撃2

        }
    }
}
