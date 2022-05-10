using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MaskableGraphic))]
public class ScreenFader : MonoBehaviour
{
    public float solidAlpha = 1f;
    public float clearAlpha = 0f;
    public float delay = 0f;
    public float timeToFade = 1f;

    private MaskableGraphic _graphic;

    void Start()
    {
        _graphic = GetComponent<MaskableGraphic>();
    }

    IEnumerator FadeRoutine (float alpha)
    {
        yield return new WaitForSeconds(delay);
        _graphic.CrossFadeAlpha(alpha, timeToFade, true);
    }

    public void FadeOn()
    {
        StartCoroutine(FadeRoutine(solidAlpha));
    }
    public void FadeOff()
    {
        StartCoroutine(FadeRoutine(clearAlpha));
    }
}
