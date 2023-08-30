using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSoundTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private UISoundManager soundManager;

    private void Start()
    {
        soundManager = UISoundManager.instance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        soundManager?.PlayClickSound();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        soundManager?.PlayHoverSound();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // If needed
    }
}
