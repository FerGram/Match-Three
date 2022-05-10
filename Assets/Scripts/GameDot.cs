using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchValue
{
    Blue, Pink, Red, Green, Yellow, Orange, Purple, Any, None
}

public class GameDot : MonoBehaviour
{
    public int xCoord;
    public int yCoord;
    public int scoreValue = 20;
    public MatchValue matchValue;
    public AudioClip clearSound;

    private bool _isMoving = false;
    private Board _board;


    public void Init (Board board)
    {
        _board = board;
    }

    public void SetCoordinates(int x, int y)
    {
        xCoord = x;
        yCoord = y;
    }

    public void MoveDot(int destX, int destY, float timeToMove)
    {
        if (!_isMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(destX, destY), timeToMove));
        }
    }
    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        _isMoving = true;

        Vector3 startPos = transform.position;
        bool destinationReached = false;
        float elapsedTime = 0f;
        
        while (!destinationReached)
        {
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                destinationReached = true;
                _board.PlaceDot(this, (int)destination.x, (int)destination.y);
                break;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            t = t * t * t *(t * (t * 6 - 15) + 10);
            transform.position = Vector3.Lerp(transform.position, destination, t);

            yield return null;
        }
        _isMoving = false;
    }

    public void ChangeColor(GameDot dotToMatch)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && dotToMatch != null)
        {
            sr.color = dotToMatch.GetComponent<SpriteRenderer>().color;
            matchValue = dotToMatch.matchValue;
        }
    }

    public void ScorePoints(int multiplier = 1, int bonus = 0)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue * multiplier + bonus);
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClipAtPoint(clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
        }
    }
}
