using System.Collections;
using UnityEngine;
using TMPro;

public class PokedexToast : MonoBehaviour
{
    public TMP_Text text;
    public CanvasGroup canvasGroup;
    public float showSeconds = 1.2f;

    Coroutine co;

    public void Show(string msg)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Run(msg));
    }

    IEnumerator Run(string msg)
    {
        if (text) text.text = msg;
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        else gameObject.SetActive(true);

        yield return new WaitForSeconds(showSeconds);

        if (canvasGroup) canvasGroup.alpha = 0f;
        else gameObject.SetActive(false);
    }
}
