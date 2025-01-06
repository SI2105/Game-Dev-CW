using UnityEngine;
using UnityEngine.UI;

public class InfiniteScrollingPanel : MonoBehaviour
{
    [SerializeField] private RectTransform content; // The content panel containing the story text
    [SerializeField] private float scrollSpeed = 50f; // Speed of the scrolling

    private float contentHeight;
    private float panelHeight;

    void Start()
    {
        if (content == null)
        {
            Debug.LogError("Content RectTransform is not assigned!");
            return;
        }

        // Get the height of the content and the panel
        contentHeight = content.rect.height;
        panelHeight = GetComponent<RectTransform>().rect.height;

        if (contentHeight <= panelHeight)
        {
            Debug.LogError("Content height must be larger than the panel height for scrolling to work!");
        }
    }

    void Update()
    {
        if (content == null) return;

        // Move the content upwards
        content.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // Reset content position if it moves completely out of view
        if (content.anchoredPosition.y >= contentHeight)
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, -panelHeight);
        }
    }
}
