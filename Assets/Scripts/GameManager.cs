using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Metroidvania.Gameplay
{
    // Giữ điểm, HUD và kết quả của một lượt chơi xuyên các map.
    public sealed class GameManager : MonoBehaviour
    {
        private const string FirstMap = "Map1";
        public static GameManager Instance { get; private set; }
        public int Score { get; private set; }
        public bool RunEnded { get; private set; }

        private PlayerHitDeath player;
        private RunHud hud;
        private bool leavingRun;
        private string startingScene;
        private string retryScene;

        public static void Begin(PlayerHitDeath player)
        {
            if (Instance == null)
            {
                new GameObject(nameof(GameManager)).AddComponent<GameManager>();
                Scene scene = player.gameObject.scene;
                Instance.startingScene = scene.path;
                if (HasCheckpoint(scene))
                {
                    Transform checkpoint = SceneTransitionManager.FindSpawnPoint(scene, "SpawnPoint");
                    if (checkpoint != null)
                        SceneTransitionManager.PlacePlayer(player.transform, checkpoint);
                }
            }

            Instance.player = player;
        }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            hud = new RunHud(transform, Reload, Exit);
            hud.SetScore(0);
            GameAudioBank.Current?.EnsureMusicPlaying();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                if (RunEnded) Time.timeScale = 1f;
                Instance = null;
            }
        }

        // Mỗi lần ghi nhận hạ quái cộng 10 điểm và mở quyền hồi máu cho Player.
        public void AwardKill()
        {
            if (RunEnded) return;

            Score += 10;
            hud.SetScore(Score);
            if (player != null && player.TryGetComponent<PlayerHealth>(out var health))
                health.OnEnemyDefeated();
        }

        internal void ShowDamage(Transform target, int amount, bool playerTookDamage)
        {
            Vector3 position = target.position;
            if (target.TryGetComponent<Collider2D>(out var collider) && collider.isActiveAndEnabled)
            {
                Bounds bounds = collider.bounds;
                position.x = bounds.center.x;
                position.y = bounds.max.y;
            }
            else if (target.TryGetComponent<SpriteRenderer>(out var sprite))
            {
                Bounds bounds = sprite.bounds;
                position.x = bounds.center.x;
                position.y = bounds.max.y;
            }

            hud.ShowDamage(position + Vector3.up * 0.45f, amount,
                playerTookDamage ? new Color32(255, 65, 65, 255) : Color.white);
        }

        public void PlayerDied(PlayerHitDeath deadPlayer)
        {
            if (player != deadPlayer || RunEnded) return;

            Score = 0;
            hud.SetScore(Score);
            Scene scene = SceneManager.GetActiveScene();
            if (HasCheckpoint(scene))
                retryScene = scene.path;
            EndRun(false, 1.05f);
        }

        private static bool HasCheckpoint(Scene scene) => scene.name == "Map2" || scene.name == "Map3";

        public void BossDefeated(EnemyTrainingDummy boss)
        {
            if (!RunEnded && boss.gameObject.scene.name == "Map3" && boss.GetComponent<BossCombat>() != null)
                EndRun(true, 0.7f);
        }

        private void EndRun(bool won, float animationTime)
        {
            RunEnded = true;
            StartCoroutine(ShowResult(won, animationTime));
        }

        // Chờ animation kết thúc và cổng chuyển map hoàn tất rồi mới dừng game để hiện kết quả.
        private IEnumerator ShowResult(bool won, float animationTime)
        {
            yield return new WaitForSecondsRealtime(animationTime);

            while (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning)
                yield return null;

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (EventSystem.current == null)
            {
                var events = new GameObject("Result EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
            }
            hud.ShowResult(won, Score);
        }

        private void Reload()
        {
            string scene = retryScene ?? startingScene;
            if (!Application.CanStreamedLevelBeLoaded(scene))
                scene = FirstMap;
            LeaveRun(scene);
        }

        private void Exit() => LeaveRun("Gamemenu");

        private void LeaveRun(string scene)
        {
            if (leavingRun) return;
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.LogError($"Cannot load {scene}: check the scene list in Build Profiles.");
                return;
            }

            leavingRun = true;
            hud.Hide();
            Time.timeScale = 1f;
            StartCoroutine(LoadAfterCleanup(scene));
        }

        private IEnumerator LoadAfterCleanup(string scene)
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.ClearRunObjects();

            // Chờ các object tồn tại xuyên scene được dọn trước khi nạp scene tiếp theo.
            yield return null;
            Instance = null;
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
            Destroy(gameObject);
        }
    }


    // Tạo các thành phần UI của lượt chơi bằng code, gồm điểm, số sát thương và bảng kết quả.
    internal sealed class RunHud
    {
        private static readonly Color Accent = new Color32(231, 179, 109, 255);
        private static readonly Color Ink = new Color32(26, 24, 29, 255);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        private readonly Canvas canvas;
        private readonly Font font;
        private readonly Text scoreLabel;
        private readonly Image overlay;
        private readonly Text title;
        private readonly Text finalScore;
        private readonly Button reloadButton;

        internal RunHud(Transform parent, UnityEngine.Events.UnityAction reload, UnityEngine.Events.UnityAction exit)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("Run HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(parent, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var score = Panel("Score", root.transform, Vector2.one, new Vector2(-152, -56),
                new Vector2(248, 62), Ink);
            score.raycastTarget = false;
            scoreLabel = Label("Score label", score.transform, Vector2.zero, new Vector2(248, 62), 30, Accent);

            overlay = Panel("Result overlay", root.transform, Center, Vector2.zero, Vector2.zero,
                new Color32(12, 10, 15, 218));
            overlay.rectTransform.anchorMin = Vector2.zero;
            overlay.rectTransform.anchorMax = Vector2.one;
            var card = Panel("Result card", overlay.transform, Center, Vector2.zero, new Vector2(620, 324), Ink);
            title = Label("Result title", card.transform, new Vector2(0, 86), new Vector2(580, 76), 60, Accent);
            finalScore = Label("Result score", card.transform, new Vector2(0, 12), new Vector2(580, 52), 30, Color.white);
            reloadButton = AddButton("Reload", card.transform, -132, reload);
            var exitButton = AddButton("Exit", card.transform, 132, exit);
            reloadButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnRight = exitButton };
            exitButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = reloadButton };
            overlay.gameObject.SetActive(false);
        }

        internal void SetScore(int score) => scoreLabel.text = $"SCORE  {score}";

        internal void ShowDamage(Vector3 position, int amount, Color color)
        {
            var label = Label("Damage number", canvas.transform, Vector2.zero, new Vector2(150, 60), 38, color);
            label.transform.SetAsFirstSibling();
            label.text = amount.ToString();
            label.fontStyle = FontStyle.Bold;
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(15, 12, 17, 240);
            outline.effectDistance = new Vector2(2f, -2f);
            label.gameObject.AddComponent<DamageNumber>().Show(label, (RectTransform)canvas.transform, position);
        }

        internal void Hide() => canvas.gameObject.SetActive(false);

        internal void ShowResult(bool won, int score)
        {
            title.text = won ? "WIN" : "GAME OVER";
            finalScore.text = $"SCORE  {score}";
            overlay.gameObject.SetActive(true);
            reloadButton.Select();
        }

        private Button AddButton(string name, Transform parent, float x, UnityEngine.Events.UnityAction action)
        {
            var size = new Vector2(220, 62);
            var background = Panel(name, parent, Center, new Vector2(x, -90), size, Color.white);
            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            var colors = button.colors;
            colors.normalColor = new Color32(77, 57, 43, 255);
            colors.highlightedColor = new Color32(129, 87, 52, 255);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = colors.normalColor;
            button.colors = colors;
            button.onClick.AddListener(action);
            Label(name + " label", background.transform, Vector2.zero, size, 30, Color.white).text = name;
            return button;
        }

        private Text Label(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var label = new GameObject(name, typeof(RectTransform)).AddComponent<Text>();
            Place(label.rectTransform, parent, Center, position, size);
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        private static Image Panel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform)).AddComponent<Image>();
            Place(image.rectTransform, parent, anchor, position, size);
            image.color = color;
            return image;
        }

        private static void Place(RectTransform rect, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
