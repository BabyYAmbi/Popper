using PopperBurst;
using UnityEngine;

public class PopperController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PopperAnimator animator;
    [SerializeField] private ProjectileLauncher projectileLauncher;

    private IPopperState currentState;
    private IGameEventSystem eventSystem;
    private bool isProcessing = false;

    public PopperColor CurrentColor => currentState.GetColor();
    public bool CanBurst => currentState.CanBurst();

    void Awake()
    {
        // Initialize with purple state
        currentState = new PurpleState();

        // Get dependencies
        if (!animator) animator = GetComponent<PopperAnimator>();
        if (!projectileLauncher) projectileLauncher = GetComponent<ProjectileLauncher>();

        // Find event system
        eventSystem = FindObjectOfType<GameEventSystem>();
    }

    void Start()
    {
        // Update visual appearance and start animations
        UpdateVisuals();
        animator?.PlayIdleAnimation();
    }

    void OnMouseDown()
    {
        HandleTap();
    }

    public void HandleTap()
    {
        if (isProcessing || PopperGameManager.Instance.TapsLeft == 0) return;

        isProcessing = true;
        PopperGameManager.Instance.UpdateTaps();
        Debug.Log($"Current Color: {currentState.GetColor()}, Can Burst: {currentState.CanBurst()}");

        if (currentState.CanBurst())
        {
            Debug.Log("Triggering burst!");
            TriggerBurst();
        }
        else
        {
            Debug.Log("Advancing color...");
            AdvanceColor();
        }

        isProcessing = false;
    }

    private void AdvanceColor()
    {
        currentState = currentState.GetNextState();
        UpdateVisuals();
        animator?.PlayTapAnimation();

        // Publish tap event
        eventSystem?.Publish(new PopperTappedEvent
        {
            popper = this,
            newColor = currentState.GetColor()
        });
    }

    private void TriggerBurst()
    {
        Debug.Log("BURST TRIGGERED!");
        animator?.PlayBurstAnimation();
            AudioManager.Instance.PlaySFX(SFXClips.Pop);

        // Launch projectiles
        Debug.Log("Launching projectiles...");
        projectileLauncher?.LaunchProjectiles(transform.position, OnProjectileHit);

        // Publish burst event
        eventSystem?.Publish(new PopperBurstEvent
        {
            popper = this,
            position = transform.position
        });
    }

    private void OnProjectileHit(Collider2D hitCollider)
    {
        PopperController hitPopper = hitCollider.GetComponent<PopperController>();
        if (hitPopper != null && hitPopper != this) // Ignore self-collision
        {
            if (hitPopper.CanBurst)
            {
                // Chain reaction
                hitPopper.TriggerBurst();
            }
            else
            {
                // Advance color
                hitPopper.AdvanceColor();
            }
        }
    }

    private void UpdateVisuals()
    {
        Sprite newSprite = currentState.GetSprite();
        animator?.UpdateSprite(newSprite);
    }

    public void ForceAdvanceToColor(PopperColor targetColor)
    {
        currentState = PopperStateFactory.CreateState(targetColor);
        UpdateVisuals();
    }
}