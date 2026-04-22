using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private ParticleSystem trailFX;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float jumpTime = 0.1f;
    public bool SchuinSpringen = true;

    [Header("Turn Check")]
    [SerializeField] private GameObject DirL;
    [SerializeField] private GameObject DirR;

    [Header("Ground Check")]
    [SerializeField] private float extraHeight = 0.25f;
    [SerializeField] private LayerMask whatIsGround;

    [HideInInspector] public bool IsFacingRight;
    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator anim;
    private float moveInput;

    private bool isJumping;
    private bool isFalling;
    private bool isGrounded;          // Gecached per frame
    private float jumpTimeCounter;
    private Coroutine resetTriggerCoroutine;
    private PlatformEffector2D currentPlatformEffector;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        StartDirectionCheck();
    }

    private void Update()
    {
        // Eén keer per frame de grondcheck uitvoeren
        isGrounded = CheckIsGrounded();

        Move();
        Jump();
        CheckPassThrough();
        DrawGroundCheck();
    }

    #region Movement
    private void Move()
    {
        moveInput = UserInput.instance.moveInput.x;

        anim.SetBool("IsWalking", moveInput != 0);

        if (moveInput != 0)
            TurnCheck();

        float horizontalSpeed = isGrounded ? moveInput * moveSpeed : moveInput * (moveSpeed / 2f);
        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);

        Dust();
    }

    private void Dust()
    {
        if (isGrounded && moveInput != 0)
        {
            if (!trailFX.isPlaying) trailFX.Play();
        }
        else
        {
            if (trailFX.isPlaying) trailFX.Stop();
        }
    }
    #endregion

    #region Jump
    private void Jump()
    {
        var jumpAction = UserInput.instance.controls.Jumping.Jump;

        if (jumpAction.WasPressedThisFrame() && isGrounded)
            StartJump();

        if (jumpAction.IsPressed())
            ContinueJump();

        if (jumpAction.WasReleasedThisFrame())
            EndJump();

        CheckForLand();
    }

    private void StartJump()
    {
        isJumping = true;
        jumpTimeCounter = jumpTime;
        float horizontalVel = SchuinSpringen ? rb.linearVelocity.x : 0f;
        rb.linearVelocity = new Vector2(horizontalVel, jumpForce);
        anim.SetTrigger("jump");
        trailFX.Stop();
    }

    private void ContinueJump()
    {
        if (!isJumping) return;

        if (jumpTimeCounter > 0)
        {
            float horizontalVel = SchuinSpringen ? rb.linearVelocity.x : 0f;
            rb.linearVelocity = new Vector2(horizontalVel, jumpForce);
            jumpTimeCounter -= Time.deltaTime;
        }
        else
        {
            isFalling = true;
            isJumping = false;
        }
    }

    private void EndJump()
    {
        isJumping = false;
        isFalling = true;
    }

    private void CheckForLand()
    {
        if (isFalling && isGrounded)
        {
            isFalling = false;
            anim.SetTrigger("land");

            if (resetTriggerCoroutine != null)
                StopCoroutine(resetTriggerCoroutine);
            resetTriggerCoroutine = StartCoroutine(ResetLandTrigger());
        }
    }

    private IEnumerator ResetLandTrigger()
    {
        yield return null;
        anim.ResetTrigger("land");
    }
    #endregion

    #region Ground Check
    private bool CheckIsGrounded()
    {
        return Physics2D.BoxCast(
            coll.bounds.center,
            coll.bounds.size,
            0f,
            Vector2.down,
            extraHeight,
            whatIsGround
        ).collider != null;
    }
    #endregion

    #region Turn Check
    private void StartDirectionCheck()
    {
        IsFacingRight = DirR.transform.position.x > DirL.transform.position.x;
    }

    private void TurnCheck()
    {
        if (UserInput.instance.moveInput.x > 0 && !IsFacingRight)
            Turn();
        else if (UserInput.instance.moveInput.x < 0 && IsFacingRight)
            Turn();
    }

    private void Turn()
    {
        IsFacingRight = !IsFacingRight;
        transform.rotation = Quaternion.Euler(0f, IsFacingRight ? 0f : 180f, 0f);
    }
    #endregion

    #region Pass-Through Platform
    private void CheckPassThrough()
    {
        // Gebruik het nieuwe Input System ipv de oude Input API
        var moveY = UserInput.instance.moveInput.y;
        if (moveY < -0.5f)
            StartCoroutine(PassThrough());
    }

    private IEnumerator PassThrough()
    {
        currentPlatformEffector = FindPlatformEffector();
        if (currentPlatformEffector == null) yield break;

        float originalSurfaceArc = currentPlatformEffector.surfaceArc;
        currentPlatformEffector.surfaceArc = 0f;

        // Wacht tot de speler het platform daadwerkelijk heeft verlaten
        yield return new WaitUntil(() => !isGrounded);

        currentPlatformEffector.surfaceArc = originalSurfaceArc;
    }

    private PlatformEffector2D FindPlatformEffector()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, coll.bounds.size, 0);
        foreach (Collider2D col in colliders)
        {
            PlatformEffector2D effector = col.GetComponent<PlatformEffector2D>();
            if (effector != null) return effector;
        }
        return null;
    }
    #endregion

    #region Debug
    private void DrawGroundCheck()
    {
        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x, 0),
            Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, 0),
            Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, coll.bounds.extents.y + extraHeight),
            Vector2.right * (coll.bounds.extents.x * 2), rayColor);
    }
    #endregion
}