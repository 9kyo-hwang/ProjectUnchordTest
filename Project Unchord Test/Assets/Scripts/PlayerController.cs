using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 3.26 ȸ�Ǹ� ���� ���� ������ ���̺� ������ ����
    /// �׿� �°� ���� ������ ����� �� ����
    /// </summary>

    private enum PlayerState
    {
        idle,
        jump,
    }

    private PlayerState pState;
    
    Rigidbody2D pRigid;
    PlayerInput pInput;
    private TrailRenderer trailRenderer;

    // ���ڸ� �ɱ�, ���ڸ� �� ���
    private bool canSitDown;
    private bool canHeadUp;

    // �̵�
    private bool canMove;
    public float defaultMoveSpeed;
    private float moveSpeed;

    // ���
    public float dashSpeed;
    public float defaultDashTime;
    private float dashTime;
    private bool isDashing;
    private bool canDash = true;
    private Vector2 dashDir;

    // ����
    private bool canJump;
    private bool canDoubleJump;
    public float jumpForce;

    // �۶��̵�
    public float fallSpeed;
    private bool isGliding;

    // �� �׼�. �� �Ŵ޸���/�ö󰡱�/�������� �� �� ����
    public bool isWallHanging;
    //private float gravityStore;
    public float wallMoveSpeed;
    public float wallJumpTime;
    private float wallJumpCounter;

    // �浹 üũ
    private bool isGround;
    private bool groundFront;
    private bool groundBack;
    public Transform groundCheckFront;
    public Transform groundCheckBack;
    public LayerMask groundLayer;
    public float groundCheckDistance;
    private int isRight = 1;

    private bool isFront;
    public Transform frontCheck;
    public LayerMask frontLayer;
    public float frontCheckDistance;

    void Start()
    {
        pState = PlayerState.idle;
        pRigid = GetComponent<Rigidbody2D>();
        pInput = GetComponent<PlayerInput>();
        trailRenderer = GetComponent<TrailRenderer>();

        moveSpeed = defaultMoveSpeed;
        dashTime = defaultDashTime;
        //gravityStore = pRigid.gravityScale;
    }
    private void Update()
    {
        // ParseState();
        CheckObject();
        FlipPlayer();
    }

    void FixedUpdate()
    {
        if (wallJumpCounter <= 0)
        {
            SitDown();
            Move();
            Run();
            Jump();
            Dash();
            Gliding();
            WallHanging();
            //WallMove();
            WallJump();
        }
        else wallJumpCounter -= Time.deltaTime;

        // ExecuteStateFixed();
    }

    void ParseState()
    {
        if(pState == PlayerState.idle && isGround && pInput.jumpButtonDown)
        {
            pState = PlayerState.jump;
        }
    }

    void ExecuteState()
    {

    }

    void ExecuteStateFixed()
    {
        if(pState == PlayerState.jump && pRigid.velocity.y <= 0.000001f && wallJumpCounter <= 0)
        {
            Jump();
        }
    }

    /// <summary>
    /// �� ������ ������ ���� �� �� �� �ִ� �ൿ�� ���� �Լ�
    /// �Ʒ� ����Ű ������ �ɱ׷� �ɱ�, �� ����Ű ������ �� ���
    /// ����� �ش� �ൿ �� �ٸ� ������ �Ұ��� �ϵ��� bool ������ ���� �˻��ϳ�
    /// ���� enum�� ���� ���¸� ������ ��� �ڵ尡 ���� �ٲ� ����
    /// </summary>
    void SitDown()
    {
        if(isGround && pRigid.velocity.x == 0 && pRigid.velocity.y == 0)
        {
            canSitDown = true;
            canHeadUp = true;
        }
        else
        {
            canSitDown = false;
            canHeadUp = false;
        }

        if((canSitDown || canHeadUp) && pInput.isVerInput)
        {
            canMove = false;
            canJump = false;
        }
        else
        {
            canMove = true;
            canJump = true;
        }
        
        if(canSitDown && pInput.verInput < 0)
        {
            Debug.Log("�ɱ׷� �ɴ� ��");
            if (pInput.horInput < 0)
            {
                Debug.Log("���� ���� ��");
            }
            else if (pInput.horInput > 0)
            {
                Debug.Log("������ ���� ��");
            }
        }
        
        if(canHeadUp && pInput.verInput > 0)
        {
            Debug.Log("�� ġ�ѵ�� ��");
            if (pInput.horInput < 0)
            {
                Debug.Log("���� ���� ��");
            }
            else if (pInput.horInput > 0)
            {
                Debug.Log("������ ���� ��");
            }
        }
    }

    void Move()
    {
        if(canMove)
        {
            pRigid.velocity = new Vector2(pInput.horInput * moveSpeed, pRigid.velocity.y);
        }

        if (!isGround && (groundFront || groundBack))
        {
            pRigid.velocity = new Vector2(pRigid.velocity.x, 0);
        }
    }

    void Run()
    {
        moveSpeed = pInput.run ? defaultMoveSpeed * 2 : defaultMoveSpeed;
    }

    void Jump()
    {
        if(canJump && isGround && pInput.jumpButtonDown)
        {
            canDoubleJump = true;
            pRigid.velocity = Vector2.zero;
            pRigid.velocity = new Vector2(pRigid.velocity.x, jumpForce);
        }
        else if(pInput.jumpButtonDown && canDoubleJump)
        {
            pRigid.velocity = Vector2.zero;
            pRigid.velocity = new Vector2(pRigid.velocity.x, jumpForce);
            canDoubleJump = false;
        }
        if(pInput.jumpButtonUp && pRigid.velocity.y > 0)
        {
            pRigid.velocity = new Vector2(pRigid.velocity.x, pRigid.velocity.y * 0.5f);
        }
    }

    void Dash()
    {
        if (!isGround && pInput.dash && canDash)
        {
            isDashing = true;
            canDash = false;
            trailRenderer.emitting = true;
            dashDir = new Vector2(pInput.horInput, 0);

            if(dashDir == Vector2.zero)
            {
                dashDir = new Vector2(transform.localScale.x, 0);
            }

            StartCoroutine(StopDashing());
        }

        if(isDashing)
        {
            pRigid.velocity = dashDir.normalized * dashSpeed;
            return;
        }

        if (isGround) canDash = true;
    }

    IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(dashTime);
        trailRenderer.emitting = false;
        isDashing = false;
    }

    void Gliding()
    {
        if(!isGround && !isWallHanging && pInput.glide)
        {
            isGliding = true;
        }

        if(isGliding && pRigid.velocity.y < 0f && Mathf.Abs(pRigid.velocity.y) > fallSpeed)
        {
            pRigid.velocity = new Vector2(pRigid.velocity.x, Mathf.Sign(pRigid.velocity.y) * fallSpeed); // sign : ��ȣ ��ȯ �Լ�
        }

        if(!isGround && !pInput.glide)
        {
            isGliding = false;
        }
    }

    void WallHanging()
    {
        isWallHanging = false;

        if (!isFront || isGround) return;
        
        if ((transform.localScale.x == 1f && pInput.horInput > 0) || 
            (transform.localScale.x == -1f && pInput.horInput < 0))
            isWallHanging = true;

        if(isWallHanging)
        {
            //pRigid.gravityScale = 0f;
            pRigid.velocity = Vector2.zero;
        }
        else
        {
            //pRigid.gravityScale = gravityStore;
        }
    }

    void WallJump()
    {
        if (isWallHanging && pInput.jumpButtonDown)
        {
            wallJumpCounter = wallJumpTime;

            pRigid.velocity = new Vector2(-pInput.horInput * defaultMoveSpeed, jumpForce);
            //pRigid.gravityScale = gravityStore;
            isWallHanging = false;
        }
    }

    void WallMove()
    {
        if (!isWallHanging) return;

        wallMoveSpeed = pInput.verInput > 0 ? 0.35f : 1;
        pRigid.velocity = new Vector2(pRigid.velocity.x, pInput.verInput * moveSpeed * wallMoveSpeed);
    }

    void CheckObject()
    {
        groundFront
            = Physics2D.Raycast(groundCheckFront.position, Vector2.down, groundCheckDistance, groundLayer);
        groundBack
            = Physics2D.Raycast(groundCheckBack.position, Vector2.down, groundCheckDistance, groundLayer);

        isGround = (groundFront || groundBack) ? true : false;

        isFront =
            Physics2D.Raycast(frontCheck.position, Vector2.right * isRight, frontCheckDistance, frontLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundCheckFront.position, Vector2.down * groundCheckDistance);
        Gizmos.DrawRay(groundCheckBack.position, Vector2.down * groundCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(frontCheck.position, Vector2.right * isRight * frontCheckDistance);
    }

    void FlipPlayer()
    {
        if (pRigid.velocity.x > 0)
        {
            transform.localScale = Vector3.one;
            isRight = 1;
        }
        else if (pRigid.velocity.x < 0)
        {
            transform.localScale = new Vector3(-1f, 1, 1f);
            isRight = -1;
        }
    }
}
