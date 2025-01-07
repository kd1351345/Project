using UnityEngine;

public class Ball : MonoBehaviour
{
    public Transform ground;

    void Start()
    {
        ResetPosition();
    }

    public void ResetPosition()
    {
        float x = Random.Range(-ground.localScale.x / 2, ground.localScale.x / 2);
        float z = Random.Range(-ground.localScale.z / 2, ground.localScale.z / 2);
        transform.position = new Vector3(x, 1f, z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ResetPosition();
        }
    }
}
