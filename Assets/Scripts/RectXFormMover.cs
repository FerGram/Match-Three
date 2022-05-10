using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectXFormMover : MonoBehaviour
{
    public float inPosX;
    public float outPosX;
    public float timeToMove;

    public void MoveXIn()
    {
        LeanTween.moveX(gameObject.GetComponent<RectTransform>(), inPosX, timeToMove).setEaseOutBack();
    }

    public void MoveXOut()
    {
        LeanTween.moveX(gameObject.GetComponent<RectTransform>(), outPosX, timeToMove).setEaseInBack();
    }
}
