using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameplayTests
{
    public sealed class CombatTests
    {
        private const BindingFlags Members =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        // Tìm component qua reflection vì assembly test không tham chiếu trực tiếp được Assembly-CSharp.
        // Cách này giữ nguyên assembly và tham chiếu scene của gameplay.
        private static Type GameType(string name) =>
            Type.GetType($"Metroidvania.Gameplay.{name}, Assembly-CSharp", throwOnError: true);

        private GameObject root;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Combat Test Objects");
            root.transform.position = new Vector3(1000, 1000, 0);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root);
            var manager = GameType("GameManager").GetProperty("Instance").GetValue(null) as Component;
            if (manager != null)
                Object.Destroy(manager.gameObject);
            yield return null;
        }

        [TestCase(25, 75, false)]
        [TestCase(100, 0, true)]
        [TestCase(150, 0, true)]
        [TestCase(0, 100, false)]
        [TestCase(-10, 100, false)]
        public void PlayerDamageClampsAtZeroAndTriggersDeath(int damage, int remaining, bool dead)
        {
            var health = Add(Child("Player"), "PlayerHealth");
            var reactions = health.GetComponent(GameType("PlayerHitDeath"));
            Assert.That(Read<int>(health, "MaxHealth"), Is.EqualTo(100));
            Call(health, "TakeDamage", damage);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(remaining));
            Assert.That(Read<bool>(reactions, "IsDead"), Is.EqualTo(dead));
            if (dead)
            {
                Call(health, "TakeDamage", 25);
                Call(reactions, "Die");
                Assert.That(Read<int>(health, "CurrentHealth"), Is.Zero);
            }
        }

        [Test]
        public void EnergySpendingRejectsInvalidCostsAndOverdraft()
        {
            var energy = Add(Child("Player"), "PlayerEnergy");
            Assert.That(Read<float>(energy, "MaxEnergy"), Is.EqualTo(100));
            Assert.That(Call(energy, "TrySpend", -1f), Is.False);
            Assert.That(Call(energy, "TrySpend", 0f), Is.True);
            Assert.That(Call(energy, "TrySpend", 101f), Is.False);
            Assert.That(Read<float>(energy, "CurrentEnergy"), Is.EqualTo(100));
            Assert.That(Call(energy, "TrySpend", 25f), Is.True);
            Assert.That(Call(energy, "TrySpend", 75f), Is.True);
            Assert.That(Call(energy, "TrySpend", 1f), Is.False);
            Assert.That(Read<float>(energy, "CurrentEnergy"), Is.Zero);
        }

        [UnityTest]
        public IEnumerator EnergyRegeneratesAfterCooldownAndStopsAtMaximum()
        {
            var player = Child("Player");
            player.SetActive(false);
            var energy = Add(player, "PlayerEnergy");
            Set(energy, "regenerationDelay", 0.1f);
            Set(energy, "regenerationPerSecond", 100f);
            player.SetActive(true);

            Assert.That(Call(energy, "TrySpend", 10f), Is.True);
            Assert.That(Read<float>(energy, "CurrentEnergy"), Is.EqualTo(90));
            yield return new WaitForSeconds(0.3f);
            Assert.That(Read<float>(energy, "CurrentEnergy"), Is.EqualTo(100));
        }

        [Test]
        public void DashOwnsVelocityUntilItEndsAndRestoresGravity()
        {
            var motor = Add(Child("Player"), "PlayerMotor2D");
            var body = motor.GetComponent<Rigidbody2D>();
            body.gravityScale = 2f;
            Call(motor, "EndDash");
            Call(motor, "Jump", 8f);
            Call(motor, "MoveHorizontal", 3f);
            Assert.That(body.linearVelocity, Is.EqualTo(new Vector2(3f, 8f)));
            Call(motor, "BeginDash", -12f);
            Call(motor, "BeginDash", 99f);
            Call(motor, "Jump", 4f);
            Call(motor, "MoveHorizontal", 4f);
            Assert.That(Read<bool>(motor, "IsDashing"), Is.True);
            Assert.That(body.linearVelocity, Is.EqualTo(new Vector2(-12f, 0f)));
            Assert.That(body.gravityScale, Is.Zero);
            Call(motor, "EndDash");
            Assert.That(Read<bool>(motor, "IsDashing"), Is.False);
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
            Assert.That(body.gravityScale, Is.EqualTo(2f));
        }

        [Test]
        public void EnemyDamageUsesHpAndRaisesHitEventOnlyForAcceptedHits()
        {
            var enemy = Add(Child("Enemy"), "EnemyTrainingDummy");
            var accepted = 0;
            GameType("EnemyTrainingDummy").GetEvent("HitReceived")
                .AddEventHandler(enemy, (Action)(() => accepted++));

            Assert.That(Read<int>(enemy, "MaxHealth"), Is.EqualTo(100));
            Call(enemy, "ReceiveHitFrom", Vector2.zero, 0);
            Call(enemy, "ReceiveHitFrom", Vector2.zero, -5);
            Call(enemy, "ReceiveHit");
            Assert.That(Read<int>(enemy, "CurrentHealth"), Is.EqualTo(75));
            Call(enemy, "ReceiveHitFrom", Vector2.zero, 30);
            Assert.That(Read<int>(enemy, "CurrentHealth"), Is.EqualTo(45));
            Call(enemy, "ReceiveHitFrom", Vector2.zero, 80);
            Call(enemy, "ReceiveHitFrom", Vector2.zero, 25);
            Assert.That(Read<int>(enemy, "CurrentHealth"), Is.Zero);
            Assert.That(Read<bool>(enemy, "IsDefeated"), Is.True);
            Assert.That(accepted, Is.EqualTo(3));
        }

        [TestCase("HitAttack1", 75)]
        [TestCase("HitAttack2", 70)]
        [TestCase("HitJumpAttack", 65)]
        public void SwingDamagesEachEnemyOnceEvenWithTwoColliders(string hitMethod, int remaining)
        {
            var player = Child("Player");
            player.AddComponent<SpriteRenderer>();
            Add(player, "PlayerMovement");
            var attack = Add(player, "PlayerAttackHitbox");
            var enemy = Child("Enemy");
            enemy.layer = 29;
            enemy.transform.localPosition = new Vector3(0.5f, -0.3f, 0f);
            enemy.AddComponent<BoxCollider2D>();
            enemy.AddComponent<CircleCollider2D>();
            var health = Add(enemy, "EnemyTrainingDummy");
            Set(attack, "enemyLayer", (LayerMask)(1 << enemy.layer));
            Physics2D.SyncTransforms();

            Call(attack, "BeginAttack");
            Call(attack, hitMethod);
            Call(attack, hitMethod);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(remaining));
            Call(attack, "BeginAttack");
            Call(attack, hitMethod);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(remaining * 2 - 100));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void KnockbackPushesAwayAndClearsVelocityOnDisable(bool flying)
        {
            var enemy = Child("Enemy");
            var knockback = (Behaviour)Add(enemy, "EnemyKnockback");
            var body = enemy.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.up * 2f;
            Set(knockback, "flyingEnemy", flying);
            Call(knockback, "PushAwayFrom", body.position + new Vector2(3f, 4f));
            Assert.That(Read<bool>(knockback, "IsActive"), Is.True);
            Assert.That(body.linearVelocity.x, Is.EqualTo(flying ? -3f : -5f).Within(0.001f));
            Assert.That(body.linearVelocity.y, Is.EqualTo(flying ? -4f : 2f).Within(0.001f));
            knockback.enabled = false;
            Assert.That(Read<bool>(knockback, "IsActive"), Is.False);
            Assert.That(body.linearVelocity, Is.EqualTo(flying ? Vector2.zero : Vector2.up * 2f));
        }

        [UnityTest]
        public IEnumerator DamageLabelsUseCorrectColorsAndDeathResetsScore()
        {
            var playerHealth = Add(Child("Player"), "PlayerHealth");
            yield return null;

            var manager = (Component)GameType("GameManager").GetProperty("Instance").GetValue(null);
            Assert.That(manager, Is.Not.Null);
            var enemy = Add(Child("Enemy"), "EnemyTrainingDummy");
            Call(enemy, "ReceiveHitFrom", Vector2.zero, 25);
            Call(playerHealth, "TakeDamage", 15);
            var labels = manager.GetComponentsInChildren<Text>(true);
            Assert.That(Array.Exists(labels, label => label.text == "25" && label.color == Color.white), Is.True);
            Assert.That(Array.Exists(labels, label =>
                label.text == "15" && ((Color32)label.color).Equals(new Color32(255, 65, 65, 255))), Is.True);

            Call(enemy, "ReceiveHitFrom", Vector2.zero, 100);
            Call(enemy, "ReceiveHitFrom", Vector2.zero, 100);
            Assert.That(Read<int>(manager, "Score"), Is.EqualTo(10));
            Call(playerHealth, "TakeDamage", 100);
            Assert.That(Read<int>(manager, "Score"), Is.Zero);
            Assert.That(Read<bool>(manager, "RunEnded"), Is.True);
            Call(manager, "AwardKill");
            Assert.That(Read<int>(manager, "Score"), Is.Zero);
        }

        [UnityTest]
        public IEnumerator EnemyHealthBarTracksHpAndHidesOnDefeat()
        {
            var enemy = Child("Enemy");
            enemy.SetActive(false);
            var health = Add(enemy, "EnemyTrainingDummy");
            var bar = Add(enemy, "EnemyHealthBarUI");
            var sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(enemy.transform, false);
            var slider = sliderObject.GetComponent<Slider>();
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(slider.transform, false);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            slider.fillRect = fill.GetComponent<RectTransform>();
            Set(bar, "enemy", health);
            Set(bar, "healthSlider", slider);
            enemy.SetActive(true);
            yield return null;

            Assert.That(slider.value, Is.EqualTo(1f));
            Call(health, "ReceiveHitFrom", Vector2.zero, 25);
            yield return null;
            Assert.That(slider.value, Is.EqualTo(0.75f));
            Assert.That(slider.fillRect.anchorMax.x, Is.EqualTo(0.75f));
            Call(health, "ReceiveHitFrom", Vector2.zero, 75);
            yield return null;
            Assert.That(slider.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator WebProjectilePassesAnotherColliderAndDamagesTargetOnce()
        {
            var player = Child("Player");
            player.transform.localPosition = Vector3.right * 2f;
            player.AddComponent<BoxCollider2D>();
            var health = Add(player, "PlayerHealth");
            var obstacle = Child("Non-target");
            obstacle.transform.localPosition = Vector3.right;
            obstacle.AddComponent<BoxCollider2D>().size = Vector2.one * 0.3f;
            var shot = Child("Web");
            shot.AddComponent<CircleCollider2D>().isTrigger = true;
            var body = shot.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var projectile = Add(shot, "EnemyWebProjectile");
            Call(projectile, "Launch", Vector2.right, player.transform);

            float deadline = Time.realtimeSinceStartup + 3f;
            while (shot != null && Time.realtimeSinceStartup < deadline)
                yield return null;

            Assert.That(shot == null, Is.True);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(90));
        }

        [UnityTest]
        public IEnumerator SpawnerCreatesOneEnemyAtEachPointOnStart()
        {
            var zone = Child("Spawn Zone");
            zone.SetActive(false);
            var spawner = Add(zone, "EnemySpawner");
            var prefab = Child("Enemy Template");
            var first = Child("First Point").transform;
            first.localPosition = new Vector3(-2f, 1f, 0f);
            var second = Child("Second Point").transform;
            second.localPosition = new Vector3(2f, 1f, 0f);
            var entriesField = spawner.GetType().GetField("enemies", Members);
            var entryType = entriesField.FieldType.GetElementType();
            var entries = Array.CreateInstance(entryType, 2);
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = Activator.CreateInstance(entryType);
                entryType.GetField("spawnPoint").SetValue(entry, i == 0 ? first : second);
                entries.SetValue(entry, i);
            }
            entriesField.SetValue(spawner, entries);
            Set(spawner, "randomPrefabs", new[] { null, prefab });

            Assert.That(zone.transform.childCount, Is.Zero);
            zone.SetActive(true);
            yield return null;
            Assert.That(zone.transform.childCount, Is.EqualTo(2));
            Assert.That(zone.transform.GetChild(0).position, Is.EqualTo(first.position));
            Assert.That(zone.transform.GetChild(1).position, Is.EqualTo(second.position));
            yield return null;
            Assert.That(zone.transform.childCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator ChaseStopsInMeleeRangeAndWhenPlayerDies()
        {
            var player = Child("Player");
            player.tag = "Player";
            player.transform.localPosition = Vector3.left * 8f;
            var health = Add(player, "PlayerHealth");
            var enemy = Child("Enemy");
            var sprite = enemy.AddComponent<SpriteRenderer>();
            var chase = (Behaviour)Add(enemy, "EnemyChase");
            var body = enemy.GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;

            yield return new WaitForFixedUpdate();
            Assert.That(Read<Transform>(chase, "Target"), Is.EqualTo(player.transform));
            Assert.That(body.linearVelocity.x, Is.Zero);

            player.transform.position = enemy.transform.position + Vector3.left * 3f;
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.x, Is.EqualTo(-4f));
            Assert.That(sprite.flipX, Is.True);

            player.transform.position = enemy.transform.position + Vector3.right * 0.4f;
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.x, Is.Zero);
            Assert.That(sprite.flipX, Is.False);

            player.transform.position = enemy.transform.position + Vector3.right * 3f;
            Call(health, "TakeDamage", 100);
            yield return new WaitForFixedUpdate();
            Assert.That(Read<bool>(chase, "HasLivingTarget"), Is.False);
            Assert.That(body.linearVelocity.x, Is.Zero);
        }

        [UnityTest]
        public IEnumerator CameraFollowsPlayerAndSwitchesPriority()
        {
            var player = Child("Player");
            player.tag = "Player";
            var output = Child("Output Camera");
            output.tag = "MainCamera";
            var outputCamera = output.AddComponent<Camera>();
            outputCamera.enabled = false;
            output.AddComponent<CinemachineBrain>();

            var duplicate = Child("Duplicate Camera");
            duplicate.tag = "MainCamera";
            var duplicateCamera = duplicate.AddComponent<Camera>();
            var near = Child("Near Camera").AddComponent<CinemachineCamera>();
            near.transform.localPosition = Vector3.right;
            var far = Child("Far Camera").AddComponent<CinemachineCamera>();
            far.transform.localPosition = Vector3.right * 20f;
            var bounds = Child("Near Camera Bounds").AddComponent<BoxCollider2D>();
            bounds.size = Vector2.one * 4f;
            bounds.isTrigger = true;
            near.gameObject.AddComponent<CinemachineConfiner2D>().BoundingShape2D = bounds;
            Physics2D.SyncTransforms();

            var manager = Child("Camera Manager").AddComponent(
                Type.GetType("CameraManager, Assembly-CSharp", throwOnError: true));
            manager.GetType().GetField("initialCamera").SetValue(manager, far);
            yield return null;

            Assert.That(outputCamera.enabled, Is.True);
            Assert.That(duplicateCamera.enabled, Is.False);
            Assert.That(near.Follow, Is.EqualTo(player.transform));
            Assert.That(far.Follow, Is.EqualTo(player.transform));
            Assert.That(near.Priority.Value, Is.EqualTo(20));
            Assert.That(far.Priority.Value, Is.Zero);

            Call(manager, "SwitchCamera", far);
            Assert.That(near.Priority.Value, Is.Zero);
            Assert.That(far.Priority.Value, Is.EqualTo(20));
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator WormExplosionDamagesPlayerOnceAfterFuse() => CheckWormBlast(0, false, 75);

        [UnityTest]
        public IEnumerator WormExplosionMissesPlayerWhoEscapes() => CheckWormBlast(0, true, 100);

        [UnityTest]
        public IEnumerator WormKilledBeforeBlastDoesNotDamagePlayer() => CheckWormBlast(100, false, 100);

        // Dùng chung kịch bản để so sánh đứng trong vụ nổ, né ra ngoài và chém sâu trước frame nổ.
        private IEnumerator CheckWormBlast(int attackDamage, bool escape, int expectedHp)
        {
            var player = Child("Player");
            player.transform.localPosition = Vector3.right;
            player.AddComponent<BoxCollider2D>();
            var playerHealth = Add(player, "PlayerHealth");

            var worm = Child("Worm");
            worm.SetActive(false);
            var health = Add(worm, "EnemyTrainingDummy");
            var chase = (Behaviour)Add(worm, "EnemyChase");
            Set(chase, "player", player.transform);
            var explosion = Add(worm, "EnemyExplosion");
            Set(explosion, "fuseSeconds", 0.06f);
            worm.GetComponent<Rigidbody2D>().gravityScale = 0f;
            UseController(worm, "EnemyWorm");
            worm.SetActive(true);
            Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(Read<bool>(explosion, "IsArmed"), Is.True);
            Assert.That(chase.enabled, Is.False);
            Call(explosion, "ApplyBlast");
            Assert.That(Read<int>(playerHealth, "CurrentHealth"), Is.EqualTo(100));
            if (attackDamage > 0)
                Call(health, "ReceiveHitFrom", (Vector2)player.transform.position, attackDamage);
            if (escape)
            {
                player.transform.localPosition = Vector3.right * 5f;
                Physics2D.SyncTransforms();
            }
            yield return new WaitForSeconds(0.12f);

            Call(explosion, "ApplyBlast");
            Call(explosion, "ApplyBlast");
            Assert.That(Read<int>(playerHealth, "CurrentHealth"), Is.EqualTo(expectedHp));
            Call(explosion, "FinishExplosion");
            yield return null;
            Assert.That(worm == null, Is.True);
        }

        [UnityTest]
        public IEnumerator BossSlashesAtCloseRange() => CheckBossAttack(2f, "SlashHit", "LaserHit", 75);

        [UnityTest]
        public IEnumerator BossLasersAtLongRange() => CheckBossAttack(6f, "LaserHit", "SlashHit", 65);

        private static readonly float[] BossAttackPositions = { -6f, -2f, 2f, 6f };

        [UnityTest]
        public IEnumerator BossPrefabHitsInFacingDirection(
            [ValueSource(nameof(BossAttackPositions))] float horizontalDistance)
        {
            var player = Child("Player");
            player.transform.localPosition = new Vector3(horizontalDistance, -3f);
            player.AddComponent<BoxCollider2D>();
            var health = Add(player, "PlayerHealth");
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EnemyBoss.prefab");
            var boss = Object.Instantiate(prefab, root.transform.position, Quaternion.identity, root.transform);
            boss.GetComponent<Rigidbody2D>().gravityScale = 0f;
            Set(boss.GetComponent(GameType("BossCombat")), "player", health);

            float deadline = Time.realtimeSinceStartup + 4f;
            while (Read<int>(health, "CurrentHealth") == 100 && Time.realtimeSinceStartup < deadline)
                yield return null;

            int expectedHp = Mathf.Abs(horizontalDistance) > 4f ? 65 : 75;
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(expectedHp));
        }

        private IEnumerator CheckBossAttack(float distance, string hit, string otherHit, int expectedHp)
        {
            var player = Child("Player");
            player.transform.localPosition = Vector3.right * 16f;
            player.AddComponent<BoxCollider2D>();
            var health = Add(player, "PlayerHealth");
            var boss = Child("Boss");
            boss.SetActive(false);
            boss.AddComponent<SpriteRenderer>();
            var combat = Add(boss, "BossCombat");
            Set(combat, "player", health);
            var body = boss.GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            UseController(boss, "EnemyBoss");
            boss.SetActive(true);
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.x, Is.Zero);

            player.transform.position = boss.transform.position + Vector3.right * 11f;
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.x, Is.EqualTo(3f));

            player.transform.position = boss.transform.position + Vector3.right * distance;
            Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.x, Is.Zero);
            Call(combat, otherHit);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(100));
            Call(combat, hit);
            Call(combat, hit);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(expectedHp));
        }

        private static void UseController(GameObject actor, string prefabName)
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null);
            var controller = prefab.GetComponent<Animator>().runtimeAnimatorController;
            Assert.That(controller, Is.Not.Null);
            var animator = actor.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            // Sau khi xác nhận AI chọn đúng đòn, test tự gọi event để kiểm tra sát thương theo frame.
            animator.speed = 0f;
        }
#endif

        [UnityTest]
        public IEnumerator PlayerHealthBarShowsMissingQuarterAfterDamage()
        {
            var player = Child("Player");
            player.SetActive(false);
            var health = Add(player, "PlayerHealth");
            var region = new GameObject("Health Region", typeof(RectTransform)).GetComponent<RectTransform>();
            region.SetParent(player.transform, false);
            region.sizeDelta = new Vector2(200f, 10f);
            var missing = new GameObject("Missing Health", typeof(RectTransform)).GetComponent<RectTransform>();
            missing.SetParent(region, false);
            Set(health, "healthRegion", region);
            Set(health, "missingHealth", missing);
            player.SetActive(true);
            yield return null;
            Assert.That(missing.rect.width, Is.Zero);
            Call(health, "TakeDamage", 25);
            yield return null;
            Assert.That(missing.rect.width, Is.EqualTo(50f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator HealthRegeneratesGraduallyOnlyAfterKillingAnEnemy()
        {
            var player = Child("Player");
            player.SetActive(false);
            var health = Add(player, "PlayerHealth");
            Add(player, "PlayerMovement");
            player.GetComponent<Rigidbody2D>().gravityScale = 0f;
            var attack = Add(player, "PlayerAttackHitbox");
            var enemy = Child("Enemy");
            enemy.layer = 29;
            enemy.transform.localPosition = Vector3.right * 8f;
            enemy.AddComponent<BoxCollider2D>();
            Add(enemy, "EnemyTrainingDummy");
            Set(attack, "enemyLayer", (LayerMask)(1 << enemy.layer));
            Set(health, "regenerationDelay", 0.12f);
            Set(health, "regenerationPerSecond", 50f);
            player.SetActive(true);
            Call(health, "TakeDamage", 25);
            Physics2D.SyncTransforms();
            Call(attack, "HitAttack1");
            yield return new WaitForSeconds(0.2f);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(75));

            enemy.transform.localPosition = new Vector3(1f, -0.3f, 0f);
            Physics2D.SyncTransforms();
            Call(attack, "BeginAttack");
            Call(attack, "HitAttack1");
            yield return new WaitForSeconds(0.2f);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(75));

            for (int swing = 0; swing < 3; swing++)
            {
                Call(attack, "BeginAttack");
                Call(attack, "HitAttack1");
            }
            yield return new WaitForSeconds(0.06f);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(75));

            yield return new WaitForSeconds(0.19f);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.InRange(76, 99));
            yield return new WaitForSeconds(0.6f);
            Assert.That(Read<int>(health, "CurrentHealth"), Is.EqualTo(100));
        }

        private GameObject Child(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(root.transform, false);
            return child;
        }

        private static Component Add(GameObject target, string type) => target.AddComponent(GameType(type));
        private static T Read<T>(Component target, string name) =>
            (T)target.GetType().GetProperty(name, Members).GetValue(target);
        private static void Set(Component target, string name, object value) =>
            target.GetType().GetField(name, Members).SetValue(target, value);
        private static object Call(Component target, string name, params object[] args) =>
            target.GetType().GetMethod(name, Members).Invoke(target, args);
    }
}
