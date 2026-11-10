using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameplayTests
{
    public sealed class WinReloadTests
    {
        [UnityTest]
        // Lặp lại luồng thắng và bấm Reload hai lần để phát hiện trạng thái UI còn sót từ lượt trước.
        public IEnumerator WinningAndClickingReloadRestartsMap3Twice()
        {
            yield return SceneManager.LoadSceneAsync("Map3");
            yield return null;

            var bossType = GameplayType("BossCombat");
            var enemyType = GameplayType("EnemyTrainingDummy");
            var managerType = GameplayType("GameManager");

            for (int attempt = 0; attempt < 2; attempt++)
            {
                var oldPlayer = GameObject.FindWithTag("Player");
                var boss = (Component)Object.FindAnyObjectByType(bossType);
                Assert.That(oldPlayer, Is.Not.Null);
                Assert.That(boss, Is.Not.Null);
                enemyType.GetMethod("ReceiveHitFrom").Invoke(boss.GetComponent(enemyType),
                    new object[] { (Vector2)oldPlayer.transform.position, int.MaxValue });

                float deadline = Time.realtimeSinceStartup + 10f;
                while (GameObject.Find("Result title") == null && Time.realtimeSinceStartup < deadline)
                    yield return null;

                var title = GameObject.Find("Result title");
                Assert.That(title, Is.Not.Null);
                Assert.That(title.GetComponent<Text>().text, Is.EqualTo("WIN"));
                Assert.That(Time.timeScale, Is.Zero);

                var reload = title.transform.parent.Find("Reload").GetComponent<Button>();
                Assert.That(EventSystem.current, Is.Not.Null);
                reload.OnPointerClick(new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left });

                deadline = Time.realtimeSinceStartup + 20f;
                while (oldPlayer != null || GameObject.FindWithTag("Player") == null)
                {
                    Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Map3 did not reload.");
                    yield return null;
                }

                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Map3"));
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                Assert.That(GameObject.FindGameObjectsWithTag("Player").Length, Is.EqualTo(1));
                Assert.That(GameObject.Find("Result title"), Is.Null);
                var manager = (Component)Object.FindAnyObjectByType(managerType);
                Assert.That(manager, Is.Not.Null);
                Assert.That(managerType.GetProperty("Score").GetValue(manager), Is.Zero);
            }
        }

        private static Type GameplayType(string name) =>
            Type.GetType($"Metroidvania.Gameplay.{name}, Assembly-CSharp", throwOnError: true);
    }
}
