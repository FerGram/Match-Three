using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Normal, Obstacle, Breakable
}

[RequireComponent(typeof(SpriteRenderer))]

public class Tile : MonoBehaviour
{
    public int xCoord;
    public int yCoord;
    public TileType tileType = TileType.Normal;

    public int breakableValue = 0;
    public Sprite[] breakableSprites;
    public Color normalColor;

    private Board _board;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(int x, int y, Board board)
    {
        xCoord = x;
        yCoord = y;
        _board = board;

        if (tileType == TileType.Breakable && breakableSprites[breakableValue] != null)
        {
            _spriteRenderer.sprite = breakableSprites[breakableValue];
        }
    }

    private void OnMouseDown()
    {
        if (_board != null)
        {
            _board.ClickTile(this);
        }
    }
    private void OnMouseEnter()
    {
        if (_board != null)
        {
            _board.DragToTile(this);
        }
    }
    private void OnMouseUp()
    {
        if (_board != null)
        {
            _board.ReleaseTile();
        }
    }

    public void BreakTile()
    {
        StartCoroutine(BreakTileRoutine());
    }
    IEnumerator BreakTileRoutine()
    {
        breakableValue--;
        breakableValue = Mathf.Clamp(breakableValue, 0, breakableValue);
        yield return new WaitForSeconds(0.25f);
        if (breakableSprites[breakableValue] != null)
        {
            _spriteRenderer.sprite = breakableSprites[breakableValue];
        }
        if (breakableValue <= 0)
        {
            tileType = TileType.Normal;
            _spriteRenderer.color = normalColor;
        }
    }
}
