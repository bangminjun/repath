using System.Collections;
using UnityEngine;
using M2MqttUnity;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
public class MQTTTest1 : M2MqttUnityClient
{
    public string topic = "test/topic";
    public GameObject target1;
    public GameObject target2;
    public GameObject target3;
    public GameObject Robot;

    private int pendingTarget = 0; // 1,2,3 메시지 대기 저장용

    protected override void Start()
    {
        // brokerAddress = "broker.emqx.io";
        brokerAddress = "10.150.151.106";
        brokerPort = 1883;
        base.Start();
        Connect();
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = Encoding.UTF8.GetString(message).Trim();
        Debug.Log("MQTT 수신: " + msg);

        // 1,2,3 수신 시 → 대기 타겟 저장
        if (msg == "1" || msg == "2" || msg == "3")
        {
            pendingTarget = int.Parse(msg);
            Debug.Log($"타겟 {pendingTarget}번 대기 중...");
        }
        // 4 수신 시 → 대기 중인 타겟 활성화
        else if (msg == "4")
        {
            Debug.Log("ddddddd");
            if (pendingTarget == 1)
            {
                target1.transform.SetParent(Robot.transform); // Robot의 자식으로 설정
                target1.transform.localPosition = new Vector3(0f, 0.1f, 0f); // 위치 초기화
                Debug.Log("1번 타겟 활성화!");
            }
            else if (pendingTarget == 2)
            {
                target2.transform.SetParent(Robot.transform); // Robot의 자식으로 설정
                target2.transform.localPosition = new Vector3(0f, 0.1f, 0f); // 위치 초기화
                Debug.Log("2번 타겟 활성화!");
            }
            else if (pendingTarget == 3)
            {
                target3.transform.SetParent(Robot.transform); // Robot의 자식으로 설정
                target3.transform.localPosition = new Vector3(0f, 0.1f, 0f); // 위치 초기화
                Debug.Log("3번 타겟 활성화!");
            }
            else
            {
                Debug.LogWarning("4 수신했지만 대기 중인 타겟 없음!");
            }

            pendingTarget = 0; // 초기화
        }
    }
}