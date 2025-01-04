using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace SG
{
    public class PopupMessageManager : MonoBehaviour
    {
        [Header("Popup UI Elements")]
        public GameObject popupPanel;
        public TextMeshProUGUI popupText;
        public Image icon;

        [Header("Popup Settings")]
        public float displayDuration = 1f;

        private Queue<ItemClass> messageQueue = new Queue<ItemClass>();
        private bool isDisplayingMessage = false;

        private void Start()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);
        }

        public void ShowPopup(ItemClass item)
        {
            if (item == null)
            {
                Debug.LogWarning("Attempted to show a popup for a null item.");
                return;
            }

            messageQueue.Enqueue(item);

            if (!isDisplayingMessage)
                StartCoroutine(ProcessQueue());
        }


        private IEnumerator ProcessQueue()
        {
            isDisplayingMessage = true;

            while (messageQueue.Count > 0)
            {
                ItemClass currentItem = messageQueue.Dequeue();
                popupPanel.SetActive(true);
                popupText.text = currentItem.displayName;
                // icon.sprite = currentItem.icon;

                yield return new WaitForSeconds(displayDuration);

                popupPanel.SetActive(false);
            }

            isDisplayingMessage = false;
        }
    }
}
