// MQTT를 이용한 NavMeshAgent 이동 스크립트
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;



public class MQTTTest : M2MqttUnityClient
{
    public string topic = "test/topic";

    public GameObject targertObject;
    public GameObject targertObject1;
    public GameObject targertObject2;
    protected override void Start()
    {
       brokerAddress = "10.150.151.106"; // 라즈베리파이 IP
      // brokerAddress = "broker.emqx.io";
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
    // 1. 메시지 변환
    string msg = Encoding.UTF8.GetString(message);
    Debug.Log("MQTT 수신: " + msg);

    if (msg.StartsWith("1"))
    {
        ApplyColor(targertObject, Color.green);
        ApplyColor(targertObject1, Color.red);
        ApplyColor(targertObject2, Color.red);
    }
    else if (msg.StartsWith("2"))
    {
        ApplyColor(targertObject, Color.red);
        ApplyColor(targertObject1, Color.green);
        ApplyColor(targertObject2, Color.red);
    }
    else if (msg.StartsWith("3"))
    {
        ApplyColor(targertObject, Color.red);
        ApplyColor(targertObject1, Color.red);
        ApplyColor(targertObject2, Color.green);
    }
}

private void ApplyColor(GameObject obj, Color color)
{
    if (obj != null)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 일반 3D와 URP 모두 대응
            renderer.material.SetColor("_Color", color);
            renderer.material.SetColor("_BaseColor", color);
        }
    }
}

}


