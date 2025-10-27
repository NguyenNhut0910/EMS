const options = [
  { label: "Voltage (V)", max: 300, value: 0 },
  { label: "Current (A)", max: 10, value: 0 },
  { label: "Power (W)", max: 10000, value: 0 },
  { label: "Frequency (Hz)", max: 80, value: 0 },
];

function drawGauge(canvasId, value, label, max) {
  const canvas = document.getElementById(canvasId);
  const ctx = canvas.getContext("2d");

  const centerX = canvas.width / 2;
  const centerY = canvas.height * 0.85;
  const radius = 50;

  ctx.clearRect(0, 0, canvas.width, canvas.height);

  // Shadow and transparency effects
  ctx.save();
  ctx.shadowColor = "rgba(0, 0, 0, 0.12)";
  ctx.shadowBlur = 12;
  ctx.shadowOffsetX = 0;
  ctx.shadowOffsetY = 4;

  // Background ring with soft gradient
  const bgGradient = ctx.createLinearGradient(
    0,
    0,
    canvas.width,
    canvas.height
  );
  bgGradient.addColorStop(0, "rgba(255,255,255,0.5)");
  bgGradient.addColorStop(1, "rgba(200,200,200,0.25)");
  ctx.beginPath();
  ctx.arc(centerX, centerY, radius, Math.PI, 2 * Math.PI);
  ctx.strokeStyle = bgGradient;
  ctx.lineWidth = 18;
  ctx.lineCap = "round";
  ctx.stroke();
  ctx.restore();

  // Foreground arc (animated value area)
  const gradient = ctx.createLinearGradient(
    centerX - radius,
    centerY,
    centerX + radius,
    centerY
  );
  gradient.addColorStop(0, "#ebc374ff");
  gradient.addColorStop(1, "#da1616ff");

  const endAngle = Math.PI + (Math.PI * value) / max;
  ctx.beginPath();
  ctx.arc(centerX, centerY, radius, Math.PI, endAngle);
  ctx.strokeStyle = gradient;
  ctx.lineWidth = 18;
  ctx.lineCap = "round";
  ctx.stroke();

  // Inner transparent fill
  ctx.beginPath();
  ctx.arc(centerX, centerY, radius - 34, Math.PI, 2 * Math.PI);
  ctx.fillStyle = "rgba(255,255,255,0.6)";
  ctx.fill();

  // Glass shine effect
  ctx.beginPath();
  ctx.arc(centerX, centerY, radius - 20, Math.PI * 1.2, Math.PI * 1.8);
  ctx.strokeStyle = "rgba(255,255,255,0.3)";
  ctx.lineWidth = 10;
  ctx.stroke();

  // Text update
  const textEl = document.getElementById(`${canvasId}-text`);
  textEl.innerHTML = `
    <div style="margin-top: 30px; font-size: 15px; font-weight: bold; color: #1a1a1a; text-shadow: 0 1px 2px rgba(0,0,0,0.2);">
      ${value.toFixed(1)}
    </div>
    <div style="font-size: 13px; color: #5f6368; margin-top: 4px;">${label}</div>
  `;
}

function updateGauges() {
  options.forEach((opt, i) => {
    drawGauge(`gauge${i + 1}`, opt.value, opt.label, opt.max);
  });
}

function updateGaugeValues(newValues) {
  if (newValues.length === 4) {
    options[0].value = newValues[0];
    options[1].value = newValues[1];
    options[2].value = newValues[2];
    options[3].value = newValues[3];
    updateGauges();
  }
}

// MQTT connection using Paho
const client = new Paho.Client(
  "nhatvietindustry.ddns.net",
  1055,
  "web_client_" + Math.random().toString(16).substr(2, 8)
);

client.onMessageArrived = function (message) {
  try {
    const data = JSON.parse(message.payloadString);
    const values = [data.U1, data.I1, data.P1, data.f];
    updateGaugeValues(values);
  } catch (e) {
    console.error("Lỗi parse JSON từ MQTT:", e);
  }
};

client.onConnectionLost = function (responseObject) {
  console.error("MQTT mất kết nối:", responseObject.errorMessage);
};

client.connect({
  onSuccess: function () {
    console.log("Đã kết nối MQTT!");
    client.subscribe("vbox/summary");
  },
  onFailure: function (err) {
    console.error("Kết nối MQTT thất bại:", err);
  },
  useSSL: false,
});

window.onload = updateGauges;
