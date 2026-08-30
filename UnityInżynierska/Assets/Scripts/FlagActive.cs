using UnityEngine;

public class FlagActivator : MonoBehaviour
{
    [Header("Player")]
    public PlayerInventory playerInventory;

    [Header("Trigger flagi")]
    public Collider2D flagTrigger;

    private void Awake()
    {
        if (playerInventory == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                playerInventory = playerObj.GetComponent<PlayerInventory>();
        }

        if (flagTrigger == null)
            flagTrigger = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.OnKeyChanged += UpdateFlagTrigger;
    }

    private void Start()
    {
        if (playerInventory != null)
            UpdateFlagTrigger(playerInventory.hasKey);
        else
            UpdateFlagTrigger(false);
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.OnKeyChanged -= UpdateFlagTrigger;
    }

    private void UpdateFlagTrigger(bool hasKey)
    {
        if (flagTrigger != null)
            flagTrigger.enabled = hasKey;

        Debug.Log(hasKey ? "[FlagActivator] Trigger flagi aktywny!" : "[FlagActivator] Trigger flagi wyłączony.");
    }
}