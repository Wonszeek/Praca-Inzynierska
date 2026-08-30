using TMPro;
using UnityEngine;

public class KeyUI : MonoBehaviour
{
    [Header("Referencje")]
    public PlayerInventory playerInventory;
    public TMP_Text keyText;

    private void Awake()
    {
        if (keyText == null)
            keyText = GetComponent<TMP_Text>();

        if (playerInventory == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                playerInventory = playerObj.GetComponent<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.OnKeyChanged += UpdateKeyText;
    }

    private void Start()
    {
        if (playerInventory != null)
            UpdateKeyText(playerInventory.hasKey);
        else
            Debug.LogWarning("[KeyUI] Nie znaleziono PlayerInventory!");
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.OnKeyChanged -= UpdateKeyText;
    }

    private void UpdateKeyText(bool hasKey)
    {
        if (keyText == null) return;

        keyText.text = hasKey ? "Key: 1/1" : "Key: 0/1";
    }
}