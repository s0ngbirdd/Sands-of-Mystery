using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static event Action OnLoadScene;

    [SerializeField] private Button[] _loadSceneButtons;
    [SerializeField] private string _sceneName;

    private void OnEnable()
    {
        foreach (Button button in _loadSceneButtons)
        {
            button.onClick.AddListener(LoadScene);
        }
    }

    private void OnDisable()
    {
        foreach (Button button in _loadSceneButtons)
        {
            button.onClick.RemoveListener(LoadScene);
        }
    }

    private void LoadScene()
    {
        Time.timeScale = 1;

        OnLoadScene?.Invoke();

        SceneManager.LoadScene(_sceneName);
    }
}
