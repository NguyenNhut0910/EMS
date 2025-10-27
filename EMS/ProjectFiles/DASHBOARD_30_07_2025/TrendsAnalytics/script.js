const apiKey =
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";
const headers = {
  apikey: apiKey,
  Authorization: `Bearer ${apiKey}`,
};

const baseUrlData = "http://nhatvietindustry.ddns.net:22321/rest/v1/data";
const baseUrlDevices = "http://nhatvietindustry.ddns.net:22321/rest/v1/device"; // bảng deviced

// Ngày hiện tại (test cố định)
const today = new Date("2025-07-20T17:00:00Z");
const dayOfWeek = today.getDay() === 0 ? 7 : today.getDay();
const start = new Date(today);
start.setDate(today.getDate() - dayOfWeek + 1);
const end = new Date(start);
end.setDate(start.getDate() + 6);

const isoStart = start.toISOString().split("T")[0] + "T00:00:00Z";
const isoEnd = end.toISOString().split("T")[0] + "T23:59:59Z";

const params = new URLSearchParams([
  ["select", "device_id,timestamp,power"],
  ["timestamp", `gte.${isoStart}`],
  ["timestamp", `lte.${isoEnd}`],
  ["order", "timestamp.asc"],
]);

// Lấy dữ liệu device name trước
Promise.all([
  fetch(`${baseUrlDevices}?select=device_id,name`, { headers }).then((res) =>
    res.json()
  ),
  fetch(`${baseUrlData}?${params.toString()}`, { headers }).then((res) =>
    res.json()
  ),
])
  .then(([devicesList, rawData]) => {
    // Tạo map device_id -> device_name
    const deviceMap = {};
    devicesList.forEach((d) => {
      deviceMap[d.device_id] = d.name;
    });

    const daysOfWeek = [...Array(7)].map((_, i) => {
      const d = new Date(start);
      d.setDate(d.getDate() + i);
      return d.toISOString().split("T")[0];
    });

    const deviceData = {};
    for (let i = 1; i < rawData.length; i++) {
      const prev = rawData[i - 1];
      const curr = rawData[i];
      if (curr.device_id !== prev.device_id) continue;

      const t1 = new Date(prev.timestamp).getTime();
      const t2 = new Date(curr.timestamp).getTime();
      const duration = (t2 - t1) / 1000;
      if (duration <= 0 || duration > 3600) continue;

      const energy = (prev.power / 1000) * (duration / 3600);
      const dateStr = new Date(prev.timestamp).toISOString().split("T")[0];

      if (!deviceData[curr.device_id]) deviceData[curr.device_id] = {};
      if (!deviceData[curr.device_id][dateStr])
        deviceData[curr.device_id][dateStr] = 0;
      deviceData[curr.device_id][dateStr] += energy;
    }

    const pastelPalette = [
      "#AEDFF7",
      "#F9D5E5",
      "#FFEAA7",
      "#D5F5E3",
      "#E8DAEF",
      "#FAD7A0",
      "#D6EAF8",
      "#F5CBA7",
    ];

    const chartContainer = document.getElementById("charts");
    let deviceIndex = 0;

    for (const [deviceId, dataByDate] of Object.entries(deviceData)) {
      const div = document.createElement("div");
      div.className = "chart-box";
      div.style.height = "180px";
      div.style.paddingTop = "0";
      div.style.marginTop = "0";
      chartContainer.appendChild(div);

      const seriesData = daysOfWeek.map((day) => dataByDate[day] || 0);
      const averageData = daysOfWeek.map((day) => {
        const energy = dataByDate[day] || 0;
        return energy > 0 ? energy / 24 : 0;
      });

      const todayStr = today.toISOString().split("T")[0];
      const baseColor = pastelPalette[deviceIndex % pastelPalette.length];
      const darkColor = tinycolor(baseColor).darken(20).toHexString();
      const midColor = tinycolor(baseColor).darken(15).toHexString();
      const lightColor = tinycolor(baseColor).lighten(5).toHexString();
      deviceIndex++;

      const colors = daysOfWeek.map((day) => {
        if (day < todayStr) return darkColor;
        else if (day === todayStr) return midColor;
        else return lightColor;
      });

      const dateLabels = daysOfWeek.map((d) => {
        const date = new Date(d);
        return `${date.getDate()}/${date.getMonth() + 1}`;
      });

      const chart = echarts.init(div);
      chart.setOption({
        textStyle: {
          fontFamily: "Quicksand, Roboto, Tahoma, sans-serif",
          fontSize: 14,
        },
        title: {
          text: deviceMap[deviceId] || `Device ${deviceId}`, // dùng tên thiết bị
          left: "center",
          top: 0,
          textStyle: {
            fontSize: 14,
            color: "#2c3e50",
          },
        },
        tooltip: {
          trigger: "axis",
          axisPointer: {
            type: "shadow",
            shadowStyle: { color: "rgba(0, 0, 0, 0.05)" },
          },
          formatter: function (params) {
            const date = daysOfWeek[params[0].dataIndex];
            const value = params[0].value;
            return `
              <div style="font-weight:bold;margin-bottom:5px">${
                dateLabels[params[0].dataIndex]
              }</div>
              <div>Power: <b>${value.toFixed(2)} kWh</b></div>
            `;
          },
        },
        grid: {
          top: 25,
          left: "3%",
          right: "4%",
          bottom: "3%",
          containLabel: true,
        },
        xAxis: {
          type: "category",
          data: dateLabels,
          axisLine: { lineStyle: { color: "#e0e0e0" } },
          axisLabel: { color: "#666", rotate: 45, margin: 15, fontSize: 8 },
          axisTick: { alignWithLabel: true },
        },
        yAxis: {
          type: "value",
          axisLine: { show: true, lineStyle: { color: "#e0e0e0" } },
          axisLabel: { color: "#666", fontSize: 8 },
          splitLine: { lineStyle: { color: "#f0f0f0", type: "dashed" } },
        },
        series: [
          {
            name: "Công suất",
            type: "bar",
            barWidth: "80%",
            data: seriesData,
            itemStyle: {
              color: (params) => colors[params.dataIndex],
              borderRadius: [4, 4, 0, 0],
              shadowColor: "rgba(0, 0, 0, 0.1)",
              shadowBlur: 5,
              shadowOffsetY: 2,
            },
          },
          {
            name: "Trung bình (kWh/h)",
            type: "line",
            data: averageData,
            smooth: true,
            symbol: "none",
            lineStyle: {
              width: 2,
              type: "solid",
              color: "rgba(52, 152, 219, 0.5)",
            },
          },
        ],
      });

      window.addEventListener("resize", () => chart.resize());
    }
  })
  .catch((err) => {
    document.getElementById("charts").innerHTML = `
      <div style="text-align:center;padding:50px;color:#e74c3c">
        <h3>Lỗi tải dữ liệu</h3>
        <p>${err.message}</p>
      </div>
    `;
  });
