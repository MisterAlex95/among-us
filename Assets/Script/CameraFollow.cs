using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float timeOffset;
    public Vector3 posOffset;

    private Vector3 velocity;
    private Movement player;

    void Start()
    {
        player = Movement.instance;
    }

    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + posOffset, ref velocity, timeOffset);
    }
}
