using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static event Action OnEnablePopup;
    public static event Action OnDisablePopup;
    public static event Action OnIgnoreSwipe;
    //public static event Action OnClick;
    public static event Action OnRelease;
    
    [SerializeField] private float _startTime = 10;
    [SerializeField] private TextMeshProUGUI _scoreTMP;
    [SerializeField] private TextMeshProUGUI _timeTMP;
    [SerializeField] private TextMeshProUGUI _currencyTMP;
    
    [SerializeField] private GameObject _pausePopup;
    [SerializeField] private GameObject _gameOverPopup;
    [SerializeField] private CanvasGroup _pauseCanvasGroup;
    [SerializeField] private CanvasGroup _gameOverCanvasGroup;
    [SerializeField] private float _fadeTime = 0.5f;
    
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _pauseRestartButton;
    [SerializeField] private Button _gameOverRestartButton;
    [SerializeField] private Button _addTimeButton;
    [SerializeField] private Button _ignoreSwipeButton;

    [SerializeField] private float _addTime = 5;
    //[SerializeField] private float _ignoreSwipeTime = 3;
    [SerializeField] private int _addTimeCost = 20;
    [SerializeField] private int _ignoreSwipeCost = 30;
    
    [SerializeField] private float _requiredHoldTime = 2;
    [SerializeField] private Image _fillImage;

    [SerializeField] private Button _soundButton;
    [SerializeField] private Image _soundImage;
    [SerializeField] private Sprite _soundEnabledSprite;
    [SerializeField] private Sprite _soundDisabledSprite;

    private int _startScore;
    private int _startCurrency;
    private Tween _tween;
    private bool _isGameOver;

    private bool _isCoroutineEnd = true;
    
    private bool _pointerDown;
    private float _pointerDownTimer;

    private void OnEnable()
    {
        _pauseButton.onClick.AddListener(EnablePausePopup);
        _resumeButton.onClick.AddListener(DisablePausePopup);
        _pauseRestartButton.onClick.AddListener(RestartGame);
        _gameOverRestartButton.onClick.AddListener(RestartGame);
        _addTimeButton.onClick.AddListener(AddTime);
        _ignoreSwipeButton.onClick.AddListener(IgnoreSwipe);
        _soundButton.onClick.AddListener(EnableDisableSound);
        
        SwipeController.OnBuildBlock += UpdateScore;
        SwipeController.OnBuildPyramid += UpdateTime;
        SwipeController.OnAddCurrency += UpdateCurrency;
        SwipeController.OnWrongSwipe += EnableGameOverPopup;

        SceneLoader.OnLoadScene += CheckScoreForSave;
    }

    private void OnDisable()
    {
        _pauseButton.onClick.RemoveListener(EnablePausePopup);
        _resumeButton.onClick.RemoveListener(DisablePausePopup);
        _pauseRestartButton.onClick.RemoveListener(RestartGame);
        _gameOverRestartButton.onClick.RemoveListener(RestartGame);
        _addTimeButton.onClick.RemoveListener(AddTime);
        _ignoreSwipeButton.onClick.RemoveListener(IgnoreSwipe);
        _soundButton.onClick.RemoveListener(EnableDisableSound);
        
        SwipeController.OnBuildBlock -= UpdateScore;
        SwipeController.OnBuildPyramid -= UpdateTime;
        SwipeController.OnAddCurrency -= UpdateCurrency;
        SwipeController.OnWrongSwipe -= EnableGameOverPopup;
        
        SceneLoader.OnLoadScene -= CheckScoreForSave;

        _tween.Kill();
    }

    private void Start()
    {
        _startCurrency = SaveLoadSystem.Instance.LoadGame1();
        _currencyTMP.text = _startCurrency.ToString();
        
        if (AudioManager.Instance.ReturnSoundEnabled())
        {
            _soundImage.sprite = _soundEnabledSprite;
        }
        else if (!AudioManager.Instance.ReturnSoundEnabled())
        {
            _soundImage.sprite = _soundDisabledSprite;
        }
    }

    private void Update()
    {
        UpdateTime(-Time.deltaTime);

        if (_startTime <= 0 && !_isGameOver)
        {
            //_isGameOver = true;
            EnableGameOverPopup();
            //RestartGame();
        }

        if (_pointerDown)
        {
            _pointerDownTimer += Time.deltaTime;
            
            _fillImage.fillAmount = 1 - _pointerDownTimer / _requiredHoldTime;

            if (_pointerDownTimer >= _requiredHoldTime)
            {
                Reset();
                OnRelease?.Invoke();
            }
        }
    }
    
    private void UpdateScore(int score)
    {
        StartCoroutine(UpdateScoreCoroutine(score));
    }

    private IEnumerator UpdateScoreCoroutine(int score)
    {
        for (int i = 0; i < score; i++)
        {
            _startScore++;
            _scoreTMP.text = _startScore.ToString();

            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    private void UpdateTime(float time)
    {
        if (Mathf.Sign(time) < 0)
        {
            _startTime += time;
            _timeTMP.text = Mathf.Round(_startTime).ToString();
        }
        else
        {
            StartCoroutine(UpdateTimeCoroutine(time));
        }
    }
    
    private IEnumerator UpdateTimeCoroutine(float time)
    {
        for (int i = 0; i < (int)time; i++)
        {
            _startTime++;
            _timeTMP.text = Mathf.Round(_startTime).ToString();

            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
    
    private void UpdateCurrency(int currency)
    {
        StartCoroutine(IncreaseCurrencyCoroutine(currency));
    }

    private IEnumerator IncreaseCurrencyCoroutine(int currency)
    {
        for (int i = 0; i < currency; i++)
        {
            _startCurrency++;
            _currencyTMP.text = _startCurrency.ToString();

            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
    
    private IEnumerator DecreaseCurrencyCoroutine(int currency)
    {
        for (int i = 0; i < currency; i++)
        {
            _startCurrency--;
            _currencyTMP.text = _startCurrency.ToString();

            yield return new WaitForSecondsRealtime(0.05f);
        }
        
        _isCoroutineEnd = true;
    }

    private void EnablePausePopup()
    {
        AudioManager.Instance.PlayOneShot("Click");
        
        OnEnablePopup?.Invoke();
        _pausePopup.SetActive(true);
        Time.timeScale = 0;
        _tween = _pauseCanvasGroup.DOFade(1, _fadeTime).SetEase(Ease.Linear).SetUpdate(true);
    }

    private void DisablePausePopup()
    {
        AudioManager.Instance.PlayOneShot("Click");
        
        _tween = _pauseCanvasGroup.DOFade(0, _fadeTime).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
        {
            _pausePopup.SetActive(false);
            Time.timeScale = 1;
            OnDisablePopup?.Invoke();
        });
    }

    private void EnableGameOverPopup()
    {
        AudioManager.Instance.PlayOneShot("GameOver");
        
        OnEnablePopup?.Invoke();
        _isGameOver = true;
        _gameOverPopup.SetActive(true);
        Time.timeScale = 0;
        _tween = _gameOverCanvasGroup.DOFade(1, _fadeTime).SetEase(Ease.Linear).SetUpdate(true);
    }

    private void RestartGame()
    {
        AudioManager.Instance.PlayOneShot("Click");
        
        Time.timeScale = 1;
        
        CheckScoreForSave();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void AddTime()
    {
        AudioManager.Instance.PlayOneShot("Click");
        
        if (_startCurrency >= _addTimeCost && _isCoroutineEnd)
        {
            StartCoroutine(DecreaseCurrencyCoroutine(_addTimeCost));
            UpdateTime(_addTime);
            //StartCoroutine(UpdateTimeCoroutine(_addTime));

            _isCoroutineEnd = false;
        }
    }
    
    private void IgnoreSwipe()
    {
        AudioManager.Instance.PlayOneShot("Click");
        
        if (_startCurrency >= _ignoreSwipeCost && _isCoroutineEnd && !_pointerDown)
        {
            OnIgnoreSwipe?.Invoke(/*_ignoreSwipeTime*/);
            StartCoroutine(DecreaseCurrencyCoroutine(_ignoreSwipeCost));

            _isCoroutineEnd = false;

            _pointerDown = true;
        }
    }

    private void Reset()
    {
        _pointerDown = false;
        _pointerDownTimer = 0;
        _fillImage.fillAmount = 0;
    }
    
    private void EnableDisableSound()
    {
        AudioManager.Instance.PlayOneShot(("Click"));
        AudioManager.Instance.EnableDisableSoundVolume();

        if (AudioManager.Instance.ReturnSoundEnabled())
        {
            _soundImage.sprite = _soundEnabledSprite;
        }
        else if (!AudioManager.Instance.ReturnSoundEnabled())
        {
            _soundImage.sprite = _soundDisabledSprite;
        }
    }

    private void CheckScoreForSave()
    {
        SaveLoadSystem.Instance.SaveGame1(_startCurrency);
        
        if (_startScore > SaveLoadSystem.Instance.LoadGame())
        {
            SaveLoadSystem.Instance.SaveGame(_startScore);
        }
    }

    private void OnApplicationQuit()
    {
        CheckScoreForSave();
    }
}
