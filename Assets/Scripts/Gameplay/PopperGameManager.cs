using System;
using System.Collections;
using PopperBurst;
using UnityEngine;

public class PopperGameManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CustomLevelSpawner _levelSpawner;

    [Header("Game Settings")]
    [SerializeField] private int _maxLevels = 4;

    private int _targetScore = 0;
    private int _maxTaps = 0;
    private int _currentLevel = 1;
    private IGameEventSystem _eventSystem;
    private int _currentScore = 0;
    private int _currentTaps = 0;
    private bool _isGameOver = false;
    private bool _isLevelComplete = false;
    private int _poppersBurst = 0;
    private int _reward = 0;

    public static PopperGameManager Instance { get; private set; }

    public int CurrentLevel => _currentLevel;
    public int MaxLevels => _maxLevels;

    public int score => _currentScore;
    public int reward => _reward;
    public int TapsLeft => _maxTaps - _currentTaps;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _eventSystem = GetComponent<GameEventSystem>();
        if (_eventSystem == null)
            _eventSystem = gameObject.AddComponent<GameEventSystem>();
    }

    private void Start()
    {
        SubscribeToEvents();
        LoadLevel(_currentLevel);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        _eventSystem.Subscribe<PopperTappedEvent>(OnPopperTapped);
        _eventSystem.Subscribe<PopperBurstEvent>(OnPopperBurst);
        _eventSystem.Subscribe<ChainReactionEvent>(OnChainReaction);
    }

    private void UnsubscribeFromEvents()
    {
        _eventSystem.Unsubscribe<PopperTappedEvent>(OnPopperTapped);
        _eventSystem.Unsubscribe<PopperBurstEvent>(OnPopperBurst);
        _eventSystem.Unsubscribe<ChainReactionEvent>(OnChainReaction);
    }

    public void LoadLevel(int levelNumber)
    {
        ResetGameState();
        
        string levelPath = $"Levels/LevelData_{levelNumber}";
        LevelData levelData = Resources.Load<LevelData>(levelPath);
        
        if (levelData == null)
        {
            Debug.LogError($"LevelData not found at path: {levelPath}");
            return;
        }

        SetLevelData(levelData);
        Debug.Log($"Loading Level {levelNumber}");
        _levelSpawner.BuildLevel(levelData);
        UpdateUI();
    }

    private void SetLevelData(LevelData levelData)
    {
        _targetScore = levelData.targetScore;
        _maxTaps = levelData.maxTaps;
    }

    private void OnPopperTapped(PopperTappedEvent evt)
    {
        Debug.Log($"Popper tapped! New color: {evt.newColor}. Taps: {_currentTaps}, Taps left: {TapsLeft}");
        StartCoroutine(CheckLevelCompletedCoroutine());
    }

    private void OnPopperBurst(PopperBurstEvent evt)
    {
        _currentScore += 10;
        _poppersBurst++;
        Debug.Log($"Popper burst! Score: {_currentScore}/{_targetScore}");
        StartCoroutine(CheckLevelCompletedCoroutine());
    }

    public void UpdateTaps()
    {
        _currentTaps++;
        UpdateUI();
    }

    private IEnumerator CheckLevelCompletedCoroutine()
    {
        yield return new WaitForSeconds(3f);

        if (_currentScore >= _targetScore)
        {
            Debug.Log("Level Complete!");
            if (_isLevelComplete)
                yield break;
            _isLevelComplete = true;
            UIManager.OnLevelCompleteEvent?.Invoke(_isLevelComplete);
            AudioManager.Instance.PlaySFX(SFXClips.LevelClear);
            yield break;
        }
        
        if (_poppersBurst < _levelSpawner._totalPoppers && TapsLeft == 0)
        {
            _isGameOver = true;
            Debug.Log("Game Over! Max taps reached.");
            AudioManager.Instance.PlaySFX(SFXClips.LevelFail);
            UIManager.OnLevelCompleteEvent?.Invoke(_isLevelComplete);
        }
    }

    private void OnChainReaction(ChainReactionEvent evt)
    {
        _reward = evt.chainLength * 5;
        // _currentScore += bonus;
        Debug.Log($"Chain reaction! Length: {evt.chainLength}, Bonus: {_reward}");
    }
    
    private void UpdateUI()
    {
        UIManager.OnUIUpdateEvent?.Invoke();
    }
    
    private void ResetGameState()
    {
        _maxTaps = 0;
        _reward = 0;
        _currentScore = 0;
        _currentTaps = 0;
        _isLevelComplete = false;
        _isGameOver = false;
        _poppersBurst = 0;
    }

    public void OnNextLevelClicked()
    {
        if (!_isLevelComplete)
        {
            Debug.LogWarning("Cannot go to next level - current level not completed!");
            return;
        }
        
        if (_currentLevel < _maxLevels)
        {
            _currentLevel++;
            LoadLevel(_currentLevel);
        }
        else
        {
            Debug.Log("All levels completed!");
            // Handle game completion
        }
    }
    
    public void OnPreviousLevelClicked()
    {
        if (_currentLevel > 1)
        {
            _currentLevel--;
            LoadLevel(_currentLevel);
        }
        else
        {
            Debug.Log("Already at first level!");
        }
    }
    
    public void OnRetryLevelClicked()
    {
        LoadLevel(_currentLevel);
    }
    
}