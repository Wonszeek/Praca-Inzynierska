using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerInventory inventory = collision.GetComponent<PlayerInventory>();

        if (inventory != null)
        {
            inventory.CollectKey();
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("[KeyPickup] Player nie ma komponentu PlayerInventory!");
        }
    }
}