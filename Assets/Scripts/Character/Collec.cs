using UnityEngine;

public class Collec : MonoBehaviour
{
    public CollectibleData collectibleData; // Reference to the Scriptable Object

    private void OnValidate()
    {
        if (collectibleData != null)
        {
            gameObject.name = collectibleData.displayName; // Optionally, update the GameObject's name
        }
    }
}
