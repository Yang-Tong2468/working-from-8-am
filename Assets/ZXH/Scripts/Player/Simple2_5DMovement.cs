using UnityEngine;

public class Simple2_5DMovement : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal"); // 左右移动
        float vertical = Input.GetAxis("Vertical");     // 纵向移动（爬梯子 / 平移）

        Vector3 direction = new Vector3(horizontal, vertical, 0f);
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
