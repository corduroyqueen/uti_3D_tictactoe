using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeCanvasScript : MonoBehaviour
{
    //simple script that fades the menus with coroutines
    //activated from buttons in the inspector
    public CanvasGroup targetCanvas;

    public void startFadeOutCanvasGroup()
    {
        StartCoroutine(fadeTextToZeroAlpha(1f, targetCanvas));
    }

    public void startFadeInCanvasGroup()
    {
        StartCoroutine(fadeTextToFullAlpha(0f, targetCanvas));
    }

    IEnumerator fadeTextToFullAlpha(float t, CanvasGroup i)
    {
        i.alpha = 0.0f;
        while (i.alpha < 1.0f)
        {
            i.alpha += Time.deltaTime / t;
            yield return null;
        }
        yield break;
    }

    IEnumerator fadeTextToZeroAlpha(float t, CanvasGroup i)
    {
        i.alpha = 1.0f;
        while (i.alpha > 0.0f)
        {
            i.alpha -= Time.deltaTime / t;
            yield return null;
        }
        yield break;
    }
}
