using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasKey = false;

    public System.Action<bool> OnKeyChanged;

    private void Start()
    {
        OnKeyChanged?.Invoke(hasKey);
    }

    public void CollectKey()
    {
        hasKey = true;

        Debug.Log("[PlayerInventory] Zebrano klucz!");

        OnKeyChanged?.Invoke(hasKey);
    }
}