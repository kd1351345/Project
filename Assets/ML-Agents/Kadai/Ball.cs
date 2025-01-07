using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Transform ground;
    public float margin = 1.0f; // 端からのマージン
    public float collisionCooldown = 2.0f; // クールダウン時間

    private bool isCoolingDown = false; // クールダウン中かどうかのフラグ

    void Start()
    {
        ResetPosition();
    }

    public void ResetPosition()
    {
        float x = Random.Range(-ground.localScale.x / 2 + margin, ground.localScale.x / 2 - margin);
        float z = Random.Range(-ground.localScale.z / 2 + margin, ground.localScale.z / 2 - margin);
        transform.position = new Vector3(x, 1f, z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("wall")) && !isCoolingDown)
        {
            ResetPosition();
            StartCoroutine(CollisionCooldown());
        }
    }

    private IEnumerator CollisionCooldown()
    {
        isCoolingDown = true; // クールダウン開始
        yield return new WaitForSeconds(collisionCooldown); // クールダウン時間待機
        isCoolingDown = false; // クールダウン終了
    }
}
