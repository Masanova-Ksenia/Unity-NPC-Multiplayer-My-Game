using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // НИКАКОЙ музыки в меню!

    public void StartSinglePlayer()
    {
        // НЕ запускать музыку здесь!
        // Она запустится в StartSinglePlayerGame()

        PlayerPrefs.SetInt("GameMode", 0); // 0 = Single
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public void StartMultiplayer()
    {
        // НЕ останавливать музыку (её и так нет в меню)

        PlayerPrefs.SetInt("GameMode", 1); // 1 = Multi
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}