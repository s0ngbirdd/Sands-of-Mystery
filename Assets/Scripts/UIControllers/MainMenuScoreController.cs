using TMPro;
using UnityEngine;

public class MainMenuScoreController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;

    private int _score;

    private void Start()
    {
        //_score = SaveLoadSystem.Instance.LoadGame();
        _score = PlayerPrefs.GetInt("Score", 0);
        _scoreText.text = _score.ToString();
    }
}
