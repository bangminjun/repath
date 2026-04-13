const express = require('express');
const mqtt = require('mqtt');
const path = require('path');

const app = express();
const PORT = 3000;

// ── MQTT 설정 ──────────────────────────────────────
const BROKER_ADDRESS = '10.150.3.168';
const MQTT_TOPIC = 'test/topic';

const mqttClient = mqtt.connect(`mqtt://${BROKER_ADDRESS}`, {
  connectTimeout: 5000,
  reconnectPeriod: 3000,
});

mqttClient.on('connect', () => {
  console.log(`✅ MQTT 브로커 연결 성공: ${BROKER_ADDRESS}`);
});

mqttClient.on('error', (err) => {
  console.error('❌ MQTT 연결 오류:', err.message);
});

mqttClient.on('reconnect', () => {
  console.log('🔄 MQTT 재연결 시도 중...');
});

// ── Express 미들웨어 ──────────────────────────────
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// ── 층 → MQTT 메시지 매핑 ─────────────────────────
const floorMap = {
  '1F/': '1',
  '2F/': '2',
  '3F/': '3',
};

// ── API: 층 선택 확정 → MQTT 발행 ─────────────────
app.post('/api/select-floor', (req, res) => {
  const { floor } = req.body;

  if (!floor || !floorMap[floor]) {
    return res.status(400).json({ success: false, message: '올바르지 않은 층 값입니다.' });
  }

  const message = floorMap[floor];

  if (!mqttClient.connected) {
    return res.status(503).json({ success: false, message: 'MQTT 브로커에 연결되지 않았습니다.' });
  }

  mqttClient.publish(MQTT_TOPIC, message, { qos: 1 }, (err) => {
    if (err) {
      console.error('❌ MQTT 발행 실패:', err.message);
      return res.status(500).json({ success: false, message: 'MQTT 발행 실패' });
    }

    console.log(`📤 MQTT 발행 완료 | topic: ${MQTT_TOPIC} | message: "${message}" (${floor})`);
    res.json({ success: true, floor, message, topic: MQTT_TOPIC });
  });
});

// ── API: MQTT 연결 상태 확인 ──────────────────────
app.get('/api/status', (req, res) => {
  res.json({
    mqtt: mqttClient.connected ? 'connected' : 'disconnected',
    broker: BROKER_ADDRESS,
    topic: MQTT_TOPIC,
  });
});

// ── 서버 시작 ─────────────────────────────────────
app.listen(PORT, () => {
  console.log(`🚀 RE:PATH 서버 실행 중: http://localhost:${PORT}`);
  console.log(`📡 MQTT 브로커: mqtt://${BROKER_ADDRESS}`);
  console.log(`📬 발행 토픽: ${MQTT_TOPIC}`);
});
