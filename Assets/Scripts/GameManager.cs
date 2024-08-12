using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour
{
    private void OnEnable() {
        GameInput.InputControl.Instance.GamePlay.Exit.performed += Exit;
    }

    private void Exit(InputAction.CallbackContext context)
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
