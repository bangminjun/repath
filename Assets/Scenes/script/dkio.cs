using UnityEngine;
using UnityEngine.SceneManagement; // 씬 로드를 위해 필요
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MqttRestartController : MonoBehaviour
{
    private MqttClient client;
    private string brokerAddress = "10.150.151.106"; // 본인의 MQTT 브로커 IP 주소
    private string topic = "test/topic";

    // 메시지 수신 여부를 확인하는 플래그
    private bool shouldRestart = false;

    void Start()
    {
        try
        {
            // 1. 클라이언트 생성 및 연결
            client = new MqttClient(brokerAddress);
            client.MqttMsgPublishReceived += OnMessageReceived;

            string clientId = System.Guid.NewGuid().ToString();
            client.Connect(clientId);

            // 2. 토픽 구독
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            Debug.Log($"MQTT 연결 성공: [{topic}] 구독 중...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MQTT 연결 실패: {e.Message}");
        }
    }

    // MQTT 메시지 수신 (백그라운드 스레드)
    void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Message);
        Debug.Log($"수신된 메시지: {message}");

        // 메시지가 "6"이면 플래그를 true로 변경
        if (message == "6")
        {
            shouldRestart = true;
        }
    }

    void Update()
    {
        // 3. 메인 스레드에서 플래그를 확인하여 씬 재시작 실행
        if (shouldRestart)
        {
            shouldRestart = false; // 중복 실행 방지를 위해 즉시 초기화
            RestartCurrentScene();
        }
    }

    void RestartCurrentScene()
    {
        Debug.Log("씬을 초기화합니다.");
        // 현재 활성화된 씬의 이름을 가져와서 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnApplicationQuit()
    {
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
        }
    }
}