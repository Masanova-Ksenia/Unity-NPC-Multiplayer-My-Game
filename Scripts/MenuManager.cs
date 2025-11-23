using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("MenuManager запущен");
    }

    public void StartGame()
    {
        Debug.Log("StartGame вызвана!");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RestartMusic();
        }

        SceneManager.LoadScene("SampleScene");
    }

    public void ExitGame()
    {
        Debug.Log("ExitGame вызвана!");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}