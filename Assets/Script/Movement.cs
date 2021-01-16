﻿using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed;

    public static Movement instance;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public Animator animator;

    private Vector3 velocity = Vector3.zero;
    private float horizontalMovement;
    private float verticalMovement;
    private bool initialized = false;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("There is more than one instance of Movement !");
            return;
        }
        instance = this;
    }

    // Only for physics
    void FixedUpdate()
    {
        MovePlayer(horizontalMovement, verticalMovement);

        if (!initialized && Socket.instance.currentPlayer != null && Socket.instance.currentPlayer.color != null)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString("#" + Socket.instance.currentPlayer.color, out color))
                spriteRenderer.color = color;
            initialized = true;
        }
    }

    void Update()
    {
        horizontalMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.fixedDeltaTime;
        verticalMovement = Input.GetAxis("Vertical") * moveSpeed * Time.fixedDeltaTime;

        animator.SetBool("Move", (horizontalMovement + verticalMovement) != 0);
        if (horizontalMovement < 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    void MovePlayer(float _horizontalMovement, float _verticalMovement)
    {
        Vector3 targetVelocity = new Vector2(_horizontalMovement, _verticalMovement);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, .05f);
        if (targetVelocity.x != 0 || targetVelocity.y != 0)
        {
            DynamicObject d = new DynamicObject();
            d.position.Set(this.transform.position.x, this.transform.position.y);
            d.uuid = Socket.instance.currentPlayer.uuid;
            Socket.instance.SendDgram("JSON", JsonUtility.ToJson(d).ToString());
        }
    }
}
