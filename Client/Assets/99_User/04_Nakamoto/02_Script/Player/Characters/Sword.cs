//--------------------------------------------------------------
// ���m�L���� [ SampleChara.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
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
    /// �X�V����
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // �U��2

        }
    }
}
