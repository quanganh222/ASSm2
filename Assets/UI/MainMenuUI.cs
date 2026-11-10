using Metroidvania.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Nút Play mở GamePlay. Nút Exit ở menu mới thoát ứng dụng.
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    private void OnEnable()
    {
        GameAudioBank.Current?.EnsureMusicPlaying();
        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    // Gỡ listener khi menu bị tắt để mỗi nút chỉ gọi một lần khi bật lại.
    private void OnDisable()
    {
        if (playButton != null) playButton.onClick.RemoveListener(OnPlayButtonClicked);
        if (exitButton != null) exitButton.onClick.RemoveListener(OnExitButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("GamePlay");
    }

    private void OnExitButtonClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}