using System.Diagnostics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动相关")]
    public float speed = 5f;
    public float groundDist = 1.0f; // 角色脚底离地面的期望距离
    public LayerMask groundLayer;

    [Header("台阶相关")]
    public float stepHeight = 0.5f; // 能爬上的台阶最大高度
    public float stepDetectionDistance = 0.5f; // 向前探测台阶的距离
    public float stepUpSpeed = 8f; // 向上移动的速度，用于爬台阶

    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical"); // 在3D空间中，前后移动是Z轴
        moveDirection = new Vector3(x, 0, z).normalized;

        if (x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity = moveDirection * speed;

        Vector3 finalMove = CalculateStepAndGround(targetVelocity * Time.fixedDeltaTime);

        rb.MovePosition(rb.position + finalMove);
    }

    /// <summary>
    /// 核心逻辑：计算地面吸附和上台阶
    /// </summary>
    private Vector3 CalculateStepAndGround(Vector3 desiredMove)
    {
        // 1. 向下探测，找到当前脚下的地面
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, groundDist + 0.2f, groundLayer))
        {
            // 向前探测台阶
            Vector3 stepCheckOrigin = groundHit.point + Vector3.up * 0.05f; // 从地面往上一点的位置开始探测
            if (Physics.Raycast(stepCheckOrigin, moveDirection, out RaycastHit stepHit, stepDetectionDistance, groundLayer))
            {
                // 检查头顶是否有空间迈上去
                Vector3 stepTopCheckOrigin = stepCheckOrigin + Vector3.up * stepHeight;
                if (!Physics.Raycast(stepTopCheckOrigin, moveDirection, stepDetectionDistance, groundLayer))
                {
                    Vector3 targetPosition = rb.position + desiredMove;
                    targetPosition.y = Mathf.Lerp(rb.position.y, groundHit.point.y + stepHeight, Time.fixedDeltaTime * stepUpSpeed);
                    return targetPosition - rb.position;
                }
            }

            // 没有台阶或无法上台阶，就执行地面吸附
            Vector3 targetPos = rb.position + desiredMove;
            targetPos.y = groundHit.point.y + groundDist; 
            return targetPos - rb.position;
        }

        // 如果脚下没有地面只进行水平移动，让重力处理Y轴
        return new Vector3(desiredMove.x, 0, desiredMove.z);
    }

}
