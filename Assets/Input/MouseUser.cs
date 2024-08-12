using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameInput
{
    public class MouseUser: MonoBehaviour
    {
        public enum MouseButton {
            Left, Right
        }

        private InputControl _inputControl;

        public Vector2 MousePosition {get; private set;}
        public Vector2 MouseInWorldPosition => Camera.main.ScreenToWorldPoint(MousePosition);

        private bool _isLeftMouseButtonPressed;
        private bool _isRightMouseButtonPressed;

        private void OnEnable() {
            _inputControl = InputControl.Instance;
            _inputControl.GamePlay.MousePosition.performed += OnMousePositionPerformed;
            _inputControl.GamePlay.PerformAction.performed += OnPerformActionPerformed;
            _inputControl.GamePlay.PerformAction.canceled += OnPerformActionCacelled;
            _inputControl.GamePlay.CancelAction.performed += OnCancelActionPerformed;
            _inputControl.GamePlay.CancelAction.canceled += OnCancelActionCacelled;
        }

        private void OnDisable() {
            _inputControl.GamePlay.MousePosition.performed -= OnMousePositionPerformed;
            _inputControl.GamePlay.PerformAction.performed -= OnPerformActionPerformed;
            _inputControl.GamePlay.PerformAction.canceled -= OnPerformActionCacelled;
            _inputControl.GamePlay.CancelAction.performed -= OnCancelActionPerformed;
            _inputControl.GamePlay.CancelAction.canceled -= OnCancelActionCacelled;
        }

        private void OnMousePositionPerformed(InputAction.CallbackContext ctx) {
            MousePosition = ctx.ReadValue<Vector2>();
        }

        private void OnPerformActionPerformed(InputAction.CallbackContext ctx) {
            _isLeftMouseButtonPressed = true;
        }

        private void OnPerformActionCacelled(InputAction.CallbackContext ctx) {
            _isLeftMouseButtonPressed = false;
        }

        private void OnCancelActionPerformed(InputAction.CallbackContext ctx) {
            _isRightMouseButtonPressed = true;
        }

        private void OnCancelActionCacelled(InputAction.CallbackContext ctx) {
            _isRightMouseButtonPressed = false;
        }

        public bool IsMouseButtonPressed(MouseButton button) {
            return button == MouseButton.Left ? _isLeftMouseButtonPressed : _isRightMouseButtonPressed;
        }
    }
}