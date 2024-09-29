using UnityEngine;

public class ShelfMover : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    private Vector3 targetPosition;
    public float speed = 1f;
    public bool moveHorizontally = true;
    public bool moveVertically = false;

    private bool movingToEnd = true;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = endPosition;
        Debug.Log($"Start Position: {startPosition}, End Position: {endPosition}, Initial Target: {targetPosition}");
    }

    private void Update()
    {
        Vector3 newPosition = transform.position;

        if (moveHorizontally)
        {
            newPosition.x = Mathf.MoveTowards(newPosition.x, targetPosition.x, speed * Time.deltaTime);
        }
        if (moveVertically)
        {
            newPosition.y = Mathf.MoveTowards(newPosition.y, targetPosition.y, speed * Time.deltaTime);
        }

        transform.position = newPosition;

        //Debug.Log($"Current Position: {newPosition}, Target: {targetPosition}, Moving To End: {movingToEnd}");

        // Проверяем, достигли ли мы цели по активным осям движения
        bool reachedTarget = 
            (!moveHorizontally || Mathf.Approximately(newPosition.x, targetPosition.x)) &&
            (!moveVertically || Mathf.Approximately(newPosition.y, targetPosition.y));

        if (reachedTarget)
        {
            movingToEnd = !movingToEnd;
            targetPosition = movingToEnd ? endPosition : startPosition;
            //Debug.Log($"Changed direction. New target: {targetPosition}");
        }
    }

    private void OnValidate()
    {
        if (!moveHorizontally && !moveVertically)
        {
            Debug.LogWarning("Shelf Mover: Neither horizontal nor vertical movement is enabled.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPosition, endPosition);
    }
}