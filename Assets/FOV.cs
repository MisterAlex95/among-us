using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOV : MonoBehaviour
{
    public float viewRadius = 5;
    public float viewAngle = 135;
    public Collider2D[] playerInRadius;
    public List<string> visiblePlayer = new List<string>();
    public LayerMask obstacleMask;

    void FixedUpdate()
    {
        FindVisiblePlayer();
    }

    void FindVisiblePlayer()
    {
        playerInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius);
        visiblePlayer.Clear();
        for (int i = 0; i < playerInRadius.Length; i++)
        {
            GameObject player = playerInRadius[i].gameObject;
            if (!player.CompareTag("Player")) continue;

            Vector2 dirPlayer = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.y - transform.position.y);
            if (Vector2.Angle(dirPlayer, transform.right) < viewAngle / 2)
            {
                float distancePlayer = Vector2.Distance(transform.position, player.transform.position);
                if (!Physics2D.Raycast(transform.position, dirPlayer, obstacleMask))
                {
                    Debug.Log("1 " + player.name);
                    visiblePlayer.Add(player.name);
                }

            }
        }
    }

    public Vector2 DirFromAngle(float angleDeg, bool global)
    {
        if (!global)
        {
            angleDeg += transform.eulerAngles.z;
        }
        return new Vector2(Mathf.Sin(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
    }
}
