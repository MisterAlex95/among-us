using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed;

    public static Movement instance;
    public SpriteRenderer spriteRenderer;
    public Sprite deadSprite;
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
        if (Socket.instance.currentPlayer.isDead) return;

        horizontalMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.fixedDeltaTime;
        verticalMovement = Input.GetAxis("Vertical") * moveSpeed * Time.fixedDeltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && Socket.instance.currentPlayer.imposter)
        {
            Socket.instance.ActionKnife();
        }

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
            MovementMessage d = new MovementMessage();
            d.position.Set(this.transform.position.x, this.transform.position.y);
            d.uuid = Socket.instance.currentPlayer.uuid;
            d.roomId = Socket.instance.currentPlayer.room.id;
            Socket.instance.SendDgram("JSON", JsonUtility.ToJson(d).ToString());
        }
    }

    public void onDeath()
    {
        if (Socket.instance.currentPlayer.isDead)
        {
            Destroy(animator);
            spriteRenderer.sprite = deadSprite;
        }
    }
}
