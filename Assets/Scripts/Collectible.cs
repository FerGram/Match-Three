using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : GameDot
{
    public bool clearedByBomb = false;
    public bool clearedAtBottom = false;

    private void Start()
    {
        matchValue = MatchValue.None;
    }
}
