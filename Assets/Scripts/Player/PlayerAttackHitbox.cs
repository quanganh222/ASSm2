using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PlayerAttackHitbox : MonoBehaviour
    {
        private static readonly int AttackTypeHash = Animator.StringToHash("AttackType");

        [SerializeField] private PlayerMovement movement;
        [SerializeField] private Transform origin;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Vector2 offset = new Vector2(0.65f, 0f);
        [SerializeField, Min(0.01f)] private float radius = 0.5f;
        [SerializeField] private Vector2 attack1Offset = new Vector2(1f, -0.3f);
        [SerializeField] private Vector2 attack1Size = new Vector2(2.4f, 0.8f);
        [SerializeField] private Vector2 jumpAttackOffset = new Vector2(0.25f, -0.55f);
        [SerializeField] private Vector2 jumpAttackSize = new Vector2(0.9f, 2.2f);

        [Header("Damage")]
        [SerializeField, Min(1)] private int attack1Damage = 25;
        [SerializeField, Min(1)] private int attack2Damage = 30;
        [SerializeField, Min(1)] private int jumpAttackDamage = 35;

        private readonly HashSet<EnemyTrainingDummy> hitTargets = new HashSet<EnemyTrainingDummy>();
        private Animator animator;

        internal int CurrentAttack => animator == null
            ? 0
            : Mathf.RoundToInt(animator.GetFloat(AttackTypeHash));

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (movement == null)
                movement = GetComponentInParent<PlayerMovement>();
        }

        // Reset once per swing, not once per Animation Event or collider.
        public void BeginAttack()
        {
            hitTargets.Clear();
        }

        // Called by an Animation Event on the frame where the slash connects.
        public void Hit()
        {
            switch (CurrentAttack)
            {
                case 1:
                    HitAttack2();
                    break;
                case 2:
                    HitJumpAttack();
                    break;
                default:
                    HitAttack1();
                    break;
            }
        }

        public void HitAttack1()
        {
            if (CanHit)
                ApplyHits(Physics2D.OverlapBoxAll(GetCenter(attack1Offset), attack1Size, 0f, enemyLayer), attack1Damage);
        }

        public void HitAttack2()
        {
            if (CanHit)
                ApplyHits(Physics2D.OverlapCircleAll(GetCenter(offset), radius, enemyLayer), attack2Damage);
        }

        public void HitJumpAttack()
        {
            if (CanHit)
                ApplyHits(Physics2D.OverlapBoxAll(GetCenter(jumpAttackOffset), jumpAttackSize, 0f, enemyLayer), jumpAttackDamage);
        }

        private bool CanHit => isActiveAndEnabled && movement != null && movement.isActiveAndEnabled &&
            GameManager.Instance?.RunEnded != true;

        private void ApplyHits(Collider2D[] colliders, int damage)
        {
            foreach (Collider2D targetCollider in colliders)
            {
                var target = targetCollider.GetComponentInParent<EnemyTrainingDummy>();
                if (target == null || target.IsDefeated || !hitTargets.Add(target))
                    continue;

                target.ReceiveHitFrom(movement.transform.position, damage);
            }
        }

        internal Vector3 GetCenter(Vector2 attackOffset)
        {
            Vector3 center = origin != null ? origin.position : transform.position;
            float direction = Application.isPlaying && movement != null && !movement.FacingRight ? -1f : 1f;
            return center + new Vector3(attackOffset.x * direction, attackOffset.y, 0f);
        }

    }
}

#if UNITY_EDITOR
namespace Metroidvania.Gameplay
{
    [CustomEditor(typeof(PlayerAttackHitbox))]
    public sealed class PlayerAttackHitboxEditor : Editor
    {
        private static readonly string[] PreviewOptions = { "Attack 1", "Attack 2", "Jump Attack" };
        private int preview;
        private readonly BoxBoundsHandle boxHandle = new BoxBoundsHandle
        {
            axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y,
            handleColor = Color.red,
            wireframeColor = Color.red
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUI.BeginChangeCheck();
            preview = EditorGUILayout.Popup("Scene Preview", preview, PreviewOptions);
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();
            var hitbox = (PlayerAttackHitbox)target;
            int attack = EditorApplication.isPlaying ? hitbox.CurrentAttack : preview;
            if (attack != 1)
            {
                DrawBox(hitbox, attack == 2 ? "jumpAttackOffset" : "attack1Offset",
                    attack == 2 ? "jumpAttackSize" : "attack1Size");
                return;
            }

            var offset = serializedObject.FindProperty("offset");
            var radius = serializedObject.FindProperty("radius");
            Vector3 center = hitbox.GetCenter(offset.vector2Value);
            float currentRadius = radius.floatValue;
            using var colorScope = new Handles.DrawingScope(Color.red);
            Handles.DrawWireDisc(center, Vector3.forward, currentRadius);
            if (EditorApplication.isPlaying) return;

            EditorGUI.BeginChangeCheck();
            Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
                offset.vector2Value += (Vector2)(newCenter - center);

            Vector3 edge = center + Vector3.right * currentRadius;
            float handleSize = HandleUtility.GetHandleSize(edge) * 0.08f;
            EditorGUI.BeginChangeCheck();
            Vector3 newEdge = Handles.Slider(edge, Vector3.right, handleSize, Handles.DotHandleCap, 0f);
            if (EditorGUI.EndChangeCheck())
                radius.floatValue = Mathf.Max(0.01f, newEdge.x - center.x);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBox(PlayerAttackHitbox hitbox, string offsetName, string sizeName)
        {
            var offset = serializedObject.FindProperty(offsetName);
            var size = serializedObject.FindProperty(sizeName);
            Vector3 center = hitbox.GetCenter(offset.vector2Value);
            if (EditorApplication.isPlaying)
            {
                using var colorScope = new Handles.DrawingScope(Color.red);
                Handles.DrawWireCube(center, size.vector2Value);
                return;
            }

            boxHandle.center = center;
            boxHandle.size = size.vector2Value;
            EditorGUI.BeginChangeCheck();
            boxHandle.DrawHandle();
            if (!EditorGUI.EndChangeCheck()) return;

            offset.vector2Value += (Vector2)(boxHandle.center - center);
            size.vector2Value = new Vector2(
                Mathf.Max(0.01f, boxHandle.size.x),
                Mathf.Max(0.01f, boxHandle.size.y));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
