using System;
using System.Collections;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SwipeController : MonoBehaviour
{
    //public static event Action OnResetRandomPyramid;
    public static event Action<float> OnBuildPyramid;
    public static event Action<int> OnBuildBlock;
    public static event Action OnWrongSwipe;
    public static event Action<int> OnAddCurrency;

    //[SerializeField] private UIController _UIController;
    [SerializeField] private GameObject _blockPrefab;
    [SerializeField] private Transform[] _pyramidTransforms;
    [SerializeField] private float _tweenDuration = 0.25f;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private Image _arrowImage;
    [SerializeField] private GameObject _arrowLeft;
    [SerializeField] private GameObject _arrowRight;
    [SerializeField] private Image _arrowLeftImage;
    [SerializeField] private Image _arrowRightImage;
    [SerializeField] private CanvasGroup _arrowCanvasGroup;
    [SerializeField] private CanvasGroup _arrowLeftCanvasGroup;
    [SerializeField] private CanvasGroup _arrowRightCanvasGroup;

    [SerializeField] private int _addScoreForBlock = 5;
    [SerializeField] private float _addTimeForPyramid = 3f;
    [SerializeField] private float _timeDecreasePerPyramid = 0.05f;

    [SerializeField] private ParticleSystem _poofParticle;
    [SerializeField] private ParticleImage _gemsParticle;

    private Vector2 _startTouchPosition;
    private Vector2 _endTouchPosition;

    private int _swipeDirection;

    private List<Transform> _pyramidChildTransforms = new List<Transform>();

    private Camera _camera;
    private Transform _randomPyramidTransform;
    private int _randomPyramidTransformIndex;
    private int _randomPyramidTransformIndexTemp;
    private GameObject _currentBlock;
    private Tween _moveTween;
    private Tween _fadeTween;
    private Tween _fadeLeft;
    private Tween _fadeRight;

    private bool _canSpawn;
    private bool _canIgnoreSwipeDirection;

    private bool _playPoofParticle;

    private void OnEnable()
    {
        UIController.OnEnablePopup += BlockSpawn;
        UIController.OnDisablePopup += UnblockSpawn;
        UIController.OnIgnoreSwipe += SetIgnoreSwipeDirection;
        UIController.OnRelease += UnsetIgnoreSwipeDirection;
    }
    
    private void OnDisable()
    {
        UIController.OnEnablePopup -= BlockSpawn;
        UIController.OnDisablePopup -= UnblockSpawn;
        UIController.OnIgnoreSwipe -= SetIgnoreSwipeDirection;
        UIController.OnRelease -= UnsetIgnoreSwipeDirection;
        
        _moveTween.Kill();
        _fadeTween.Kill();
        _fadeLeft.Kill();
        _fadeRight.Kill();
    }
    

    private void Start()
    {
        _camera = Camera.main;

        //ResetRandomPyramid();
        
        _canSpawn = true;
        _swipeDirection = 1;
        _randomPyramidTransformIndex = _pyramidTransforms.Length - 1;
        _randomPyramidTransform = Instantiate(_pyramidTransforms[_randomPyramidTransformIndex]);

        foreach (Transform child in _randomPyramidTransform)
        {
            _pyramidChildTransforms.Add(child);
        }
        
        _arrowImage.rectTransform.anchoredPosition = new Vector2(Screen.width * 0.42f * _swipeDirection, _arrowImage.rectTransform.anchoredPosition.y);
        _arrowImage.rectTransform.localScale = new Vector3(_swipeDirection, 1, 1);
        
        _fadeTween = _arrowCanvasGroup.DOFade(0, _tweenDuration * 3).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        
        _arrowLeftImage.rectTransform.anchoredPosition = new Vector2(Screen.width * 0.42f * -1, _arrowLeftImage.rectTransform.anchoredPosition.y);
        _arrowLeftImage.rectTransform.localScale = new Vector3(-1, 1, 1);
        
        _arrowRightImage.rectTransform.anchoredPosition = new Vector2(Screen.width * 0.42f * 1, _arrowRightImage.rectTransform.anchoredPosition.y);
        _arrowRightImage.rectTransform.localScale = new Vector3(1, 1, 1);
    }
    
    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !_moveTween.IsActive() && _canSpawn)
        {
            _startTouchPosition = Input.GetTouch(0).position;
            
            if (_pyramidChildTransforms.Count > 0)
            {
                _currentBlock = SpawnBlock();
            }
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && _currentBlock != null && !_moveTween.IsActive() && _canSpawn)
        {
            _endTouchPosition = Input.GetTouch(0).position;
            
            // Move from right to LEFT
            if (_endTouchPosition.x < _startTouchPosition.x)
            {
                if (_pyramidChildTransforms.Count > 0)
                {
                    if (_swipeDirection == 1 || _canIgnoreSwipeDirection)
                    {
                        MoveBlock();
                    }
                    else
                    {
                        //RestartGame();

                        OnWrongSwipe?.Invoke();
                    }
                }
            }
            // Move from left to RIGHT
            else if (_endTouchPosition.x > _startTouchPosition.x)
            {
                if (_pyramidChildTransforms.Count > 0)
                {
                    if (_swipeDirection == -1 || _canIgnoreSwipeDirection)
                    {
                        MoveBlock();
                    }
                    else
                    {
                        //RestartGame();

                        OnWrongSwipe?.Invoke();
                    }
                }
            }
        }
        
        if (Time.timeScale < 0.01f && _playPoofParticle)
        {
            _poofParticle.Simulate(Time.unscaledDeltaTime, true, false);
            //_poofParticle.Simulate(Time.unscaledDeltaTime, false, true, true);
        }
    }

    private GameObject SpawnBlock()
    {
        GameObject block = Instantiate(_blockPrefab);
        //block.transform.localScale = _pyramidChildTransforms[0].localScale;
        block.GetComponent<SpriteRenderer>().size = new Vector2(_pyramidChildTransforms[0].localScale.x, 1);
        block.transform.position = new Vector2(_camera.orthographicSize * 2 * _swipeDirection, _pyramidChildTransforms[0].position.y);

        return block;
    }

    private void MoveBlock()
    {
        _moveTween = _currentBlock.transform.DOMoveX(_pyramidChildTransforms[0].position.x, _tweenDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            AudioManager.Instance.PlayOneShot("PlaceBlock");
            
            _currentBlock.transform.SetParent(_randomPyramidTransform);
            _swipeDirection *= -1;
            _pyramidChildTransforms.RemoveAt(0);
            _arrowImage.rectTransform.anchoredPosition = new Vector2(Screen.width * 0.42f * _swipeDirection, _arrowImage.rectTransform.anchoredPosition.y);
            _arrowImage.rectTransform.localScale = new Vector3(_swipeDirection, 1, 1);
            _currentBlock = null;
            
            OnBuildBlock?.Invoke(_addScoreForBlock);

            if (_pyramidChildTransforms.Count <= 0)
            {
                //_poofParticle.Simulate(Time.unscaledDeltaTime, true, false);
                _poofParticle.Play();
                _playPoofParticle = true;
                
                _gemsParticle.Play();
                
                Invoke(nameof(ResetRandomPyramid), 0.2f);
                
                
                OnBuildPyramid?.Invoke(_addTimeForPyramid);
                _addTimeForPyramid -= _addTimeForPyramid * _timeDecreasePerPyramid;

                if (_addTimeForPyramid < 1)
                {
                    _addTimeForPyramid = 1;
                }
                
                OnBuildBlock?.Invoke(_addScoreForBlock * 4);
                
                AudioManager.Instance.PlayOneShot("AddGems");
                OnAddCurrency?.Invoke(10);
                
                //RestartGame();
            }
        });
    }

    private void ResetRandomPyramid()
    {
        if (_randomPyramidTransform != null)
        {
            _randomPyramidTransformIndexTemp = _randomPyramidTransformIndex;
            Destroy(_randomPyramidTransform.gameObject);
        }
        
        _canSpawn = true;
        _swipeDirection = 1;

        do
        {
            _randomPyramidTransformIndex = Random.Range(0, _pyramidTransforms.Length);
        }
        while (_randomPyramidTransformIndex == _randomPyramidTransformIndexTemp);

        _randomPyramidTransform = Instantiate(_pyramidTransforms[_randomPyramidTransformIndex]);

        foreach (Transform child in _randomPyramidTransform)
        {
            _pyramidChildTransforms.Add(child);
        }
        
        _arrowImage.rectTransform.anchoredPosition = new Vector2(Screen.width * 0.42f * _swipeDirection, _arrowImage.rectTransform.anchoredPosition.y);
        _arrowImage.rectTransform.localScale = new Vector3(_swipeDirection, 1, 1);

        //_playPoofParticle = false;

        //OnResetRandomPyramid?.Invoke();
    }

    private void BlockSpawn()
    {
        _canSpawn = false;
    }
    
    private void UnblockSpawn()
    {
        _canSpawn = true;
    }

    private void SetIgnoreSwipeDirection(/*float time*/)
    {
        //_arrowCanvasGroup.alpha = 0;
        _fadeTween.Kill();
        _arrow.SetActive(false);
        _arrowLeft.SetActive(true);
        _arrowRight.SetActive(true);
        _arrowLeftCanvasGroup.alpha = 1;
        _arrowRightCanvasGroup.alpha = 1;
        _fadeLeft = _arrowLeftCanvasGroup.DOFade(0,_tweenDuration * 3).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        _fadeRight = _arrowRightCanvasGroup.DOFade(0,_tweenDuration * 3).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);

        _canIgnoreSwipeDirection = true;

        //StartCoroutine(UnsetIgnoreSwipeDirection(time));
    }
    
    /*private IEnumerator UnsetIgnoreSwipeDirection(float time)
    {
        yield return new WaitForSeconds(time);
        
        _fadeLeft.Kill();
        _fadeRight.Kill();
        _arrowLeft.SetActive(false);
        _arrowRight.SetActive(false);
        //_arrowCanvasGroup.alpha = 1;
        _arrow.SetActive(true);
        _fadeTween = _arrowCanvasGroup.DOFade(0,_tweenDuration * 3).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        
        _canIgnoreSwipeDirection = false;
    }*/
    
    private void UnsetIgnoreSwipeDirection()
    {
        _fadeLeft.Kill();
        _fadeRight.Kill();
        _arrowLeft.SetActive(false);
        _arrowRight.SetActive(false);
        //_arrowCanvasGroup.alpha = 1;
        _arrow.SetActive(true);
        _arrowCanvasGroup.alpha = 1;
        _fadeTween = _arrowCanvasGroup.DOFade(0,_tweenDuration * 3).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        
        _canIgnoreSwipeDirection = false;
    }

    /*private void RestartGame()
    {
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }*/
}
