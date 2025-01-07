using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class BallChaserAgent : Agent
{
    public Transform ball;
    public float speed = 10f;
    private Rigidbody rBody;
    private Vector3 lastPosition;
    private float timeSinceLastMove;
    private float timeAwayFromBall = 0f;

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        lastPosition = transform.localPosition;
        timeSinceLastMove = 0f;
    }

    public override void OnEpisodeBegin()
    {
        // エージェントが落ちた場合のリセット処理
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // 最後の位置のリセット
        lastPosition = transform.localPosition;
        timeSinceLastMove = 0f;
        ball.GetComponent<Ball>().ResetPosition();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // エージェントの位置とボールの位置を観察
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(ball.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = Mathf.Clamp(actions.ContinuousActions[0], -1, 1);
        controlSignal.z = Mathf.Clamp(actions.ContinuousActions[1], -1, 1);
        rBody.AddForce(new Vector3(controlSignal.x, 0, controlSignal.z) * speed, ForceMode.VelocityChange); // Y軸に力を与えない

        float distanceToBall = Vector3.Distance(this.transform.localPosition, ball.localPosition);

        // ボールに近づくほど報酬
        float distanceReward = 1.0f - (distanceToBall / 50.0f);
        AddReward(distanceReward * 0.01f);

        // エージェントが前回の位置から移動した場合に報酬
        float distanceMoved = Vector3.Distance(transform.localPosition, lastPosition);
        if (distanceMoved > 0.01f)
        {
            AddReward(distanceMoved * 0.01f); // 移動した距離に基づいた報酬
            lastPosition = transform.localPosition;
            timeSinceLastMove = 0f;
        }
        else
        {
            timeSinceLastMove += Time.deltaTime; // 移動がなければタイマー進行
        }

        // 一定時間以上動かない場合にペナルティを与える
        if (timeSinceLastMove > 5.0f)
        {
            AddReward(-0.5f);
            timeSinceLastMove = 0f;
        }

        //ボールに近づかなかったら時間を増加
        if (distanceToBall > 1.42f)
        {
            timeAwayFromBall += Time.deltaTime;
        }
        else
        {
            timeAwayFromBall = 0f;
        }

        if (timeAwayFromBall > 3f)
        {
            AddReward(-1f);
            timeAwayFromBall = 0f;
        }


        // ボールをキャッチしたら大きな報酬
        if (distanceToBall < 1.42f)
        {
            SetReward(1.0f);
            ball.GetComponent<Ball>().ResetPosition();
            Debug.Log("キャッチ");
            EndEpisode();
        }

        // エージェントが不必要に動いた場合にペナルティを与える
        if (this.transform.localPosition.y < -1.0f)
        {
            AddReward(-0.1f);
            EndEpisode();
        }

        // カスタムログを追加
      //  Debug.Log($"Step: {StepCount}, Reward: {GetCumulativeReward()}");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            // 壁に衝突した際に罰を与える
            AddReward(-0.1f);

        }
        else if (collision.gameObject.CompareTag("ball"))
        {
            SetReward(1.0f);
            ball.GetComponent<Ball>().ResetPosition();
            Debug.Log("キャッチ");
            EndEpisode();
        }
    }
}
