using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartLoading : MonoBehaviour
{
    [SerializeField] private float _totalLoadingTime = 5;
    [SerializeField] private Image _fillImage;
    [SerializeField] private string _sceneToLoad;

    private float _currentLoadingTime;

    private void Start()
    {
        Invoke(nameof(LoadGame), _totalLoadingTime);
    }

    private void Update()
    {
        if (_currentLoadingTime < 5)
        {
            _currentLoadingTime += Time.deltaTime;
            _fillImage.fillAmount = _currentLoadingTime / _totalLoadingTime;
        }
    }

    private void LoadGame()
    {
        SceneManager.LoadScene(_sceneToLoad);
    }
}
