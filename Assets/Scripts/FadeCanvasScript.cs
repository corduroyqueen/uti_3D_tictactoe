using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeCanvasScript : MonoBehaviour
{
    /// This script that fades the menus in and out with coroutines, which is activated by the menu buttons
    public CanvasGroup targetCanvas;

    public void StartFadeOutCanvasGroup()
    {
        StartCoroutine(FadeTextToZeroAlpha(1f, targetCanvas));
    }

    public void StartFadeInCanvasGroup()
    {
        StartCoroutine(FadeTextToFullAlpha(1f, targetCanvas));
    }

    IEnumerator FadeTextToFullAlpha(float t, CanvasGroup i)
    {
        i.alpha = 1.0f;
        RectTransform trans = GetComponent<RectTransform>();
        trans.anchoredPosition3D = new Vector3(0f, 0f, 50000f);
        while (trans.anchoredPosition3D.z>0f)
        {
            trans.anchoredPosition3D = Vector3.Lerp(trans.anchoredPosition3D, new Vector3(0f, 0f, -100f), 0.4f); //this code fades the menu in by making it zoom in from far away
            yield return null;
        }
        trans.anchoredPosition3D = Vector3.zero;
        yield break;
    }

    IEnumerator FadeTextToZeroAlpha(float t, CanvasGroup i)
    {
        i.alpha = 1.0f;
        while (i.alpha > 0.0f)
        {
            i.alpha -= Time.deltaTime / t; //this code makes the starting and ending menus fade to 0 alpha
            yield return null;
        }
        yield break;
    }
}
