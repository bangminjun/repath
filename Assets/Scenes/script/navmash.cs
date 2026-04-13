// MQTT 를 이용한 NavMeshAgent 이동 스크립트
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MqttPointManager : MonoBehaviour
{
    [Header("Navigation Settings")]
    public NavMeshAgent agent;
    public List<Transform> waypoints;

    private MqttClient client;
    //private string brokerIp = "";//node-red 용 ip 주소
    //private string brokerIp = "broker.emqx.io.1883";
    private string brokerIp = "10.150.151.106";//라즈베리파이 ip 주소
    private string topic = "test/topic";

    private int nextTargetIndex = -1;
    private readonly object _lock = new object();

    private bool isMovingToTarget = false;   // 목표 지점으로 이동 중인지
    private bool isReturningToBase = false;  // 0 번으로 복귀 중인지
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private string lastMessage = ""; // 필드로 선언

    void Start()
    {
        client = new MqttClient(brokerIp);
        client.MqttMsgPublishReceived += OnMessageReceived;

        string clientId = System.Guid.NewGuid().ToString();
        client.Connect(clientId);

        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        Debug.Log($"MQTT 연결 완료: {brokerIp} / 토픽: {topic}");
    }

    public void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
{
    string message = Encoding.UTF8.GetString(e.Message);
    Debug.Log("수신된 메시지: " + message);

    lock (_lock)
    {
        lastMessage = message; // 필드에 저장
        if (int.TryParse(message, out int index))
        {
            nextTargetIndex = index;
        }
    }
}

    void Update()
    {
        int indexToMove = -1;
        lock (_lock)
        {
            if (nextTargetIndex != -1)
            {
                indexToMove = nextTargetIndex;
                nextTargetIndex = -1;
            }
        }

        // MQTT 명령 수신 시 목표 지점으로 이동
        if (indexToMove != -1)
        {
            MoveToWaypoint(indexToMove);
            isMovingToTarget = true;
            isReturningToBase = false;
        }

       if (!agent.pathPending && agent.remainingDistance < 0.5f)
{
    if (isMovingToTarget && !isWaiting)
    {
        isWaiting = true;
        waitTimer = 0f;
        client.Publish("test/topic", Encoding.UTF8.GetBytes("4"));
        Debug.Log("[발행] test/topic : 4");
        Debug.Log("목표 도착! 3 초 후 0 번 위치로 복귀합니다...");
    }
}

// 대기 타이머
if (isWaiting)
{
    waitTimer += Time.deltaTime;
    if (waitTimer >= 3f && lastMessage.StartsWith("5"))
    {
        isWaiting = false;
        isMovingToTarget = false;
        isReturningToBase = true;
        waitTimer = 0f;
        MoveToWaypoint(0);
        Debug.Log("3 초 완료! 0 번 위치로 복귀 시작");
    }
}

// 0 번 복귀 완료
if (!agent.pathPending && agent.remainingDistance < 0.5f)
{
    if (isReturningToBase)
    {
        isReturningToBase = false;   
        client.Publish("test/topic", Encoding.UTF8.GetBytes("6"));     
        Debug.Log("0 번 위치 복귀 완료!");
    }
}
    }

    private void MoveToWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
        {
            if (waypoints[index] != null)
            {
                agent.SetDestination(waypoints[index].position);
                Debug.Log($"{index}번 지점으로 이동 시작: {waypoints[index].name}");
            }
        }
        else
        {
            Debug.LogWarning($"리스트 범위를 벗어난 인덱스입니다: {index}");
        }
    }

    private void OnApplicationQuit()
    {
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
        }
    }
}
