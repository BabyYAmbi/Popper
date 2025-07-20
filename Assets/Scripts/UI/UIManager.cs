using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Transform popUpParent;

    [SerializeField] private TextMeshProUGUI tapsLeftText;
    [SerializeField] private TextMeshProUGUI levelNoText;


    [SerializeField] private LevelPopUp popUp;
    [SerializeField] private GameObject levelCompletePopUp;

    public static Action<bool> OnLevelCompleteEvent;
    public static Action OnUIUpdateEvent;
    private LevelPopUp _levelPopUpRef;

    void Start()
    {
        OnLevelCompleteEvent += OnLevelComplete;
        OnUIUpdateEvent += UpdateUIDisplay;

        UpdateUIDisplay();
    }

    void OnLevelComplete(bool levelcompleted)
    {
        if (levelcompleted && PopperGameManager.Instance.CurrentLevel == PopperGameManager.Instance.MaxLevels)
        {
            InstantiateLevelsCompletePopUP();
            return;
        }

        InstantiateLevelPopUp(popUp, levelcompleted);
    }

    void OnDestroy()
    {
        OnLevelCompleteEvent -= OnLevelComplete;
        OnUIUpdateEvent -= UpdateUIDisplay;
    }

    private void UpdateUIDisplay()
    {
        if (PopperGameManager.Instance != null)
        {
            levelNoText.text = $"Level : {PopperGameManager.Instance.CurrentLevel}";

            tapsLeftText.text = $"Taps Left: {PopperGameManager.Instance.TapsLeft}";
        }
    }

    private void InstantiateLevelPopUp(LevelPopUp gameObject, bool levelCompleted)
    {
        if (_levelPopUpRef != null)
            return;
        _levelPopUpRef = Instantiate(gameObject, popUpParent);
        _levelPopUpRef.Initialise(levelCompleted);
    }

    private void InstantiateLevelsCompletePopUP()
    {
        Instantiate(levelCompletePopUp, popUpParent);
    }
}
