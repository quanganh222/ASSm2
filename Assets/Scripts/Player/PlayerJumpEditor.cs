using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Metroidvania.Gameplay
{
    [CustomEditor(typeof(PlayerJump))]
    public sealed class PlayerJumpEditor : Editor
    {
        private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle
        {
            axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y,
            handleColor = Color.yellow,
            wireframeColor = Color.yellow
        };

        private void OnSceneGUI()
        {
            serializedObject.Update();
            var anchor = serializedObject.FindProperty("groundCheck").objectReferenceValue as Transform;
            if (anchor == null) return;

            var size = serializedObject.FindProperty("groundCheckSize");
            var offset = serializedObject.FindProperty("groundCheckOffset");
            boundsHandle.center = anchor.position + (Vector3)offset.vector2Value;
            boundsHandle.size = size.vector2Value;

            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();
            if (!EditorGUI.EndChangeCheck()) return;

            size.vector2Value = new Vector2(
                Mathf.Max(0.01f, boundsHandle.size.x),
                Mathf.Max(0.01f, boundsHandle.size.y));
            offset.vector2Value = boundsHandle.center - anchor.position;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
