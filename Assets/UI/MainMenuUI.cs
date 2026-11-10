using Metroidvania.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Bắt buộc phải có dòng này để chuyển Scene

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

    private void OnDisable()
    {
        if (playButton != null) playButton.onClick.RemoveListener(OnPlayButtonClicked);
        if (exitButton != null) exitButton.onClick.RemoveListener(OnExitButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        // Chuyển từ Scene MainMenu sang Scene GamePlay
        SceneManager.LoadScene("GamePlay"); // Nhập chính xác tên Scene Gameplay của bạn
    }

    private void OnExitButtonClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}