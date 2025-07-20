using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPopUp : MonoBehaviour
{
    [SerializeField] private Button nextLevelButon;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button previousLevelButton;
    [SerializeField] private TextMeshProUGUI headerText;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rewardText;


    private bool _levelCompleted = false;
    public void Initialise(bool levelcompleted)
    {
        _levelCompleted = levelcompleted;
        SetTexts();
        SetLevelState();
        SetButtonListeners();
        AnimatePopUp();
        SetHeaderText(levelcompleted);
    }

    private void SetButtonListeners()
    {
        nextLevelButon.onClick.AddListener(OnClickNextButtonClick);
        retryButton.onClick.AddListener(OnRetryButtonClick);
        previousLevelButton.onClick.AddListener(OnPreviosButtonClick);
    }

    private void RemoveButtonListeners()
    {
        nextLevelButon.onClick.RemoveListener(OnClickNextButtonClick);
        retryButton.onClick.RemoveListener(OnRetryButtonClick);
        previousLevelButton.onClick.RemoveListener(OnPreviosButtonClick);
    }

    private void SetLevelState()
    {
        if (!_levelCompleted)
        {
            nextLevelButon.interactable = false;
        }
        if (PopperGameManager.Instance.CurrentLevel == 1)
        {
            previousLevelButton.interactable = false;
        }
    }

    private void SetHeaderText(bool levelcompleted)
    {
        if (levelcompleted)
        {
            headerText.text = "Super!";
        }
        else
        {
            headerText.text = "Failed!";
        }
    }

    private void SetTexts()
    {
        scoreText.text = $"Score : <color=#ffffff>{PopperGameManager.Instance.score}</color>";
        rewardText.text = $"Reward : <color=#ffffff>{PopperGameManager.Instance.reward}</color>";
    }

    public void Disable()
    {
        RemoveButtonListeners();
        Destroy(gameObject);
    }

    private void AnimatePopUp()
    {
        transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            nextLevelButon.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack);
            retryButton.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack);
            previousLevelButton.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack);
        });
    }

    private void OnClickNextButtonClick()
    {
        PopperGameManager.Instance.OnNextLevelClicked();
        HidePopUp();
    }

    private void OnPreviosButtonClick()
    {
        PopperGameManager.Instance.OnPreviousLevelClicked();
        HidePopUp();
    }

    private void OnRetryButtonClick()
    {
        PopperGameManager.Instance.OnRetryLevelClicked();
        HidePopUp();
    }
    
    private void HidePopUp()
    {
        Disable();
    }

}
