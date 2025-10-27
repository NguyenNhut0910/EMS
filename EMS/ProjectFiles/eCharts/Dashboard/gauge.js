// Cấu hình tùy chọn gauge
const option = {
  value: [0, 0, 0, 0],
  label: ["Điện áp (V)", "Dòng điện (A)", "Công suất (W)", "Tần số (Hz)"],
  max: [300, 10, 10000, 80],
  grad: [
    { start: 200, mid: 280, end: 340 },
    { start: 120, mid: 60, end: 0 },
    { start: 60, mid: 30, end: 0 },
    { start: 300, mid: 200, end: 0 },
  ],
};

// Hàm cập nhật tất cả gauge
function updateGauges() {
  for (let i = 0; i < 4; i++) {
    renderSemiGauge(
      "gauge" + (i + 1),
      "gauge" + (i + 1) + "-text",
      option.value[i],
      option.label[i],
      option.max[i],
      option.grad[i]
    );
  }
}

// Hàm cập nhật giá trị mới
function updateGaugeValues(newValues) {
  option.value = newValues;
  updateGauges();
}

// Hàm vẽ gauge đơn
function renderSemiGauge(gaugeId, textId, value, label, max, grad) {
  const gauge = document.getElementById(gaugeId);
  if (!gauge) return;
  const dots = 30;
  const percentValue = Math.max(0, Math.min(100, (value / max) * 100));
  const colored = Math.floor((dots * percentValue) / 100);
  const angleStep = 180 / (dots - 1);
  let points = "";

  const start = grad.start,
    mid = grad.mid,
    end = grad.end;

  for (let i = 0; i < dots; i++) {
    const angle = -90 + i * angleStep;
    const percent = (i * 100) / (dots - 1);
    const marked = i < colored ? " marked" : "";
    const height = i === colored - 1 ? 14 : 9;
    let style = `width:2.5px;height:${height}px;background:#666;position:absolute;left:50%;bottom:0;border-radius:1.25px;transform-origin:bottom center;transition:background 0.3s, box-shadow 0.3s;opacity:0.88;`;
    style += `transform: rotate(${angle}deg) translateY(-45px);`;

    if (i < colored) {
      let h1, h2;
      if (percent < 50) {
        h1 = start + (mid - start) * (percent / 50);
        h2 = start + (mid - start) * ((percent + 2) / 50);
      } else {
        h1 = mid + (end - mid) * ((percent - 50) / 50);
        h2 = mid + (end - mid) * ((percent - 50 + 2) / 50);
      }
      style += `background: linear-gradient(180deg, hsl(${h2},97%,58%) 0%, hsl(${h1},97%,46%) 100%);box-shadow:0 0 5px #fff5;opacity:1;`;
    }

    points += `<div class="points${marked}" style="${style}"></div>`;
  }

  gauge.innerHTML = points;
  // document.getElementById(textId).innerHTML = `<span class="percent">${value.toFixed(
  //   2
  // )}</span><span class="label">${label}</span>`;
  document.getElementById(textId).innerHTML = `
  <div style="
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    text-align: center;
    
  ">
    <div style="font-weight: bold; font-size: 10px;">${value.toFixed(2)}</div>
    <div style="font-size: 8px;">${label}</div>
  </div>`;
}

// Khởi tạo gauge khi trang tải
document.addEventListener("DOMContentLoaded", updateGauges);

// === ✅ Tích hợp Paho MQTT (WebSocket an toàn cho FactoryTalk Optix) ===

// Khởi tạo MQTT Client
const client = new Paho.Client(
  "nhatvietindustry.ddns.net",
  1055,
  "web_client_" + Math.random().toString(16).substr(2, 8)
);

// Thiết lập sự kiện khi nhận message
client.onMessageArrived = function (message) {
  try {
    const data = JSON.parse(message.payloadString);
    const values = [data.U1, data.I1, data.P1, data.f];
    updateGaugeValues(values);
  } catch (e) {
    console.error("Lỗi parse JSON từ MQTT:", e);
  }
};

// Sự kiện khi kết nối thành công
client.onConnectionLost = function (responseObject) {
  console.error("MQTT mất kết nối:", responseObject.errorMessage);
};

// Kết nối tới broker
client.connect({
  onSuccess: function () {
    console.log("Đã kết nối MQTT!");
    client.subscribe("vbox/summary");
  },
  onFailure: function (err) {
    console.error("Kết nối MQTT thất bại:", err);
  },
  useSSL: false, // Nếu không dùng SSL
});
