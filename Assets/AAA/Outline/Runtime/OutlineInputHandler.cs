using UnityEngine;
using UnityEngine.InputSystem;

namespace AAA.Outline
{
    /// <summary>
    /// Bridges Unity's New Input System callbacks to outline actions.
    /// Use with a PlayerInput component set to "Invoke Unity Events" or "Send Messages".
    /// </summary>
    public sealed class OutlineInputHandler : MonoBehaviour
    {
        /// <summary>Target outline component to control.</summary>
        public OutlineComponent targetOutline;

        /// <summary>Thickness step applied when moving on the vertical axis (Move.y).</summary>
        public float thicknessStepPerUnit = 1.0f;

        /// <summary>
        /// Toggle outline on/off.
        /// Bind an InputAction named "ToggleOutline" to call this.
        /// </summary>
        public void OnToggleOutline(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (targetOutline == null) return;

            targetOutline.Toggle();
            UnityGameLogger.Debug(nameof(OutlineInputHandler), $"ToggleOutline -> IsApplied: {targetOutline.IsApplied}", targetOutline);
        }

        /// <summary>
        /// Adjust thickness via a 2D Move input (e.g., WASD or stick).
        /// Move.y &gt; 0 increases thickness; &lt; 0 decreases.
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            if (targetOutline == null) return;

            Vector2 moveInput = context.ReadValue<Vector2>();
            if (Mathf.Approximately(moveInput.y, 0f)) return;

            float delta = moveInput.y * thicknessStepPerUnit * Time.deltaTime * 60f;
            // Choose correct domain based on current mode
            float current = targetOutline.useWorldSpaceThickness ? targetOutline.thicknessWorld : targetOutline.thicknessPixels;
            float updated = Mathf.Max(0f, current + delta);
            targetOutline.SetThickness(updated);

            UnityGameLogger.Debug(nameof(OutlineInputHandler), $"Move.y={moveInput.y:F2}, Thickness={updated:F2}", targetOutline);
        }
    }
}