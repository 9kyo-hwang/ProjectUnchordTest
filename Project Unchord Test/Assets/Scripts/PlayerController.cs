using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 3.26 회의를 통해 상태 데이터 테이블 구축할 예정
    /// 그에 맞게 상태 종류가 변경될 수 있음
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

    // 제자리 앉기, 제자리 고개 들기
    private bool canSitDown;
    private bool canHeadUp;

    // 이동
    private bool canMove;
    public float defaultMoveSpeed;
    private float moveSpeed;

    // 대시
    public float dashSpeed;
    public float defaultDashTime;
    private float dashTime;
    private bool isDashing;
    private bool canDash = true;
    private Vector2 dashDir;

    // 점프
    private bool canJump;
    private bool canDoubleJump;
    public float jumpForce;

    // 글라이딩
    public float fallSpeed;
    private bool isGliding;

    // 벽 액션. 벽 매달리기/올라가기/내려가기 및 벽 점프
    public bool isWallHanging;
    //private float gravityStore;
    public float wallMoveSpeed;
    public float wallJumpTime;
    private float wallJumpCounter;

    // 충돌 체크
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
    /// 땅 위에서 가만히 있을 때 할 수 있는 행동에 대한 함수
    /// 아랫 방향키 누르면 쪼그려 앉기, 윗 방향키 누르면 고개 들기
    /// 현재는 해당 행동 중 다른 동작이 불가능 하도록 bool 조건을 통해 검사하나
    /// 이후 enum에 따라 상태를 정의할 경우 코드가 많이 바뀔 예정
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
            Debug.Log("쪼그려 앉는 중");
            if (pInput.horInput < 0)
            {
                Debug.Log("왼쪽 보는 중");
            }
            else if (pInput.horInput > 0)
            {
                Debug.Log("오른쪽 보는 중");
            }
        }
        
        if(canHeadUp && pInput.verInput > 0)
        {
            Debug.Log("고개 치켜드는 중");
            if (pInput.horInput < 0)
            {
                Debug.Log("왼쪽 보는 중");
            }
            else if (pInput.horInput > 0)
            {
                Debug.Log("오른쪽 보는 중");
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
            pRigid.velocity = new Vector2(pRigid.velocity.x, Mathf.Sign(pRigid.velocity.y) * fallSpeed); // sign : 부호 반환 함수
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
