const API_URL = new URL("http://nhatvietindustry.ddns.net:22321/rest/v1/data");
const API_KEY =
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";

const today = new Date();
const currentDate = today.toISOString().split("T")[0]; // Định dạng: YYYY-MM-DD
// console.log(currentDate);

const device = "1,2,3,4";
const startTimee = "2025-01-01";
const endTimee = "2025-05-20";
const enery = "device_id,timestamp,power,voltage";
const mode = "month";

function getTitleByMode(mode) {
  switch (mode) {
    case "month":
      return "year";
    case "week":
      return "month";
    case "day":
      return "week";
    case "hour":
      return "day";
    default:
      return mode;
  }
}

function groupBy(array, keyFn) {
  return array.reduce((acc, item) => {
    const key = keyFn(item);
    acc[key] = acc[key] || [];
    acc[key].push(item);
    return acc;
  }, {});
}

function getDateInfo(timestampStr) {
  const date = new Date(timestampStr);
  const day = date.getDate();
  const month = date.getMonth() + 1;
  const year = date.getFullYear();
  const hour = date.getHours();

  // Tuần trong tháng (1–5)
  const weekInMonth = Math.ceil(day / 7);

  return {
    hour,
    day,
    week: weekInMonth,
    month,
    year,
    label: {
      hour: `${hour}:00`,
      day: `${day}/${month}`,
      week: `Week ${weekInMonth}`,
      month: `Month ${month}`,
    },
  };
}

function getWeekNumber(date) {
  const temp = new Date(date.getTime());
  temp.setHours(0, 0, 0, 0);
  temp.setDate(temp.getDate() + 4 - (temp.getDay() || 7));
  const yearStart = new Date(temp.getFullYear(), 0, 1);
  const weekNo = Math.ceil(((temp - yearStart) / 86400000 + 1) / 7);
  return weekNo;
}
function showNotice(msg) {
  const notice = document.getElementById("notice");
  notice.innerText = msg;
}

function clearNotice() {
  document.getElementById("notice").innerText = "";
}

function validateRange(data, mode) {
  const first = getDateInfo(data[0].timestamp);
  const last = getDateInfo(data[data.length - 1].timestamp);
  const dayDiff = parseInt(last.day) - parseInt(first.day);
  console.log("dayDiff", dayDiff);

  switch (mode) {
    case "hour":
      if (
        first.year !== last.year ||
        first.month !== last.month ||
        first.day !== last.day
      ) {
        showNotice("  Các bản ghi không nằm cùng một ngày!");
        return false;
      }
      break;
    case "day":
      if (first.year !== last.year || first.month !== last.month) {
        showNotice("  Các bản ghi không nằm cùng tháng!");
        return false;
      }
      // const dayDiff = parseInt(last.day) - parseInt(first.day);
      if (dayDiff < 0 || dayDiff > 7) {
        showNotice("  Khoảng ngày không hợp lệ (nên <= 7 ngày)!");
        return false;
      }
      break;
    case "week":
      if (
        !(first.year === last.year && first.month === last.month && dayDiff > 7)
      ) {
        showNotice(
          "  Khoảng dữ liệu không đủ để chia theo tuần trong tháng! (Cần cùng tháng và > 7 ngày)"
        );
        // showNotice(`Khoảng: ${data[data.length - 1].Timestamp}`);
        return false;
      }
      break;
    case "month":
      if (first.year !== last.year) {
        showNotice("  Các bản ghi không cùng năm!");
        return false;
      }
      break;
  }

  clearNotice();
  return true;
}

let barChartInstance = null;
let doughnutChartInstance = null;

function processData(data, mode = "hour") {
  if (!validateRange(data, mode)) return;
  // console.log("asdasd")
  const sorted = data
    .filter((d) => !isNaN(d.power))
    .sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp));

  const energyMap = new Map(); // { timeKey => { label, device_id1: value, device_id2: value } }

  for (let i = 1; i < sorted.length; i++) {
    const prev = sorted[i - 1];
    const curr = sorted[i];

    if (curr.power <= 0 || curr.device_id !== prev.device_id) continue;

    const t1 = new Date(prev.timestamp);
    const t2 = new Date(curr.timestamp);
    const deltaSeconds = (t2 - t1) / 1000;
    if (deltaSeconds <= 0) continue;

    const energy_kWh = (curr.power / 1000) * (deltaSeconds / 3600);

    const { label, ...keys } = getDateInfo(curr.timestamp);
    const timeKey = keys[mode];
    const labelStr = label[mode];

    const key = `${timeKey}`;
    if (!energyMap.has(key)) {
      energyMap.set(key, { label: labelStr });
    }

    const entry = energyMap.get(key);
    const devKey = `device_${curr.device_id}`;
    entry[devKey] = (entry[devKey] || 0) + energy_kWh;
  }

  const allDeviceIds = Array.from(
    new Set(
      Array.from(energyMap.values()).flatMap((entry) =>
        Object.keys(entry).filter((key) => key.startsWith("device_"))
      )
    )
  ).sort((a, b) => {
    const aNum = parseInt(a.replace("device_", ""));
    const bNum = parseInt(b.replace("device_", ""));
    return aNum - bNum;
  });
  // Chuẩn hóa đầu ra
  const labels = [];
  const deviceDatasets = {};
  for (const dev of allDeviceIds) {
    deviceDatasets[dev] = [];
  }

  for (const entry of energyMap.values()) {
    labels.push(entry.label);
    for (const dev of allDeviceIds) {
      deviceDatasets[dev].push(parseFloat((entry[dev] || 0).toFixed(3)));
    }
  }

  const datasets = {};
  [...allDeviceIds].forEach((dev) => {
    datasets[dev] = deviceDatasets[dev];
  });
  console.log(labels, datasets);
  drawBarChart(labels, datasets, mode);
  // drawDoughnutChart(data, mode);
  drawDoughnutChart(energyMap);

  // return { labels, datasets };
}

function drawBarChart(labels, valuesByDevice, mode) {
  const ctx = document.getElementById("barChart").getContext("2d");
  if (barChartInstance) barChartInstance.destroy();

  const deviceIds = Object.keys(valuesByDevice);
  const totalDevices = deviceIds.length;

  // Hàm tạo màu pastel dựa trên chỉ số
  function generatePastelColor(index) {
    const hue = Math.floor((index * 360) / totalDevices);
    return `hsl(${hue}, 60%, 75%)`;
  }

  const datasets = deviceIds.map((deviceId, index) => ({
    label: `Device ${deviceId.replace("device_", "")}`,
    data: valuesByDevice[deviceId],
    backgroundColor: generatePastelColor(index),
    borderColor: "rgba(0,0,0,0.1)",
    borderWidth: 1,
  }));

  barChartInstance = new Chart(ctx, {
    type: "bar",
    data: {
      labels,
      datasets,
    },
    options: {
      responsive: true,

      plugins: {
        title: {
          display: true,
          text: `Average voltage chart by ${getTitleByMode(mode)}`,
        },
        tooltip: {
          callbacks: {
            label: (ctx) => `${ctx.dataset.label}: ${ctx.parsed.y} (kWh)`,
          },
        },
      },
      scales: {
        y: {
          title: {
            display: true,
            text: "Total energy (kWh)",
          },
        },
      },
    },
  });
}

function drawDoughnutChart(energyMap) {
  const deviceTotalEnergy = {};

  for (const entry of energyMap.values()) {
    for (const key of Object.keys(entry)) {
      if (key.startsWith("device_")) {
        deviceTotalEnergy[key] = (deviceTotalEnergy[key] || 0) + entry[key];
      }
    }
  }

  const deviceIds = Object.keys(deviceTotalEnergy).sort((a, b) => {
    const aNum = parseInt(a.replace("device_", ""));
    const bNum = parseInt(b.replace("device_", ""));
    return aNum - bNum;
  });
  console.log("deviceIds", deviceIds);
  const labels = deviceIds.map((id) => `Device ${id.replace("device_", "")}`);
  const values = deviceIds.map((id) => +deviceTotalEnergy[id].toFixed(2));

  const max = Math.max(...values);
  const min = Math.min(...values);

  const backgroundColors = values.map((val) => {
    const percent = (val - min) / (max - min || 1);
    const lightness = 80 - percent * 40;
    return `hsl(140, 60%, ${lightness}%)`;
  });

  const ctx = document.getElementById("doughnutChart").getContext("2d");
  if (doughnutChartInstance) doughnutChartInstance.destroy();

  doughnutChartInstance = new Chart(ctx, {
    type: "doughnut",
    data: {
      labels,
      datasets: [
        {
          label: "Tổng năng lượng (kWh)",
          data: values,
          backgroundColor: backgroundColors,
        },
      ],
    },
    options: {
      responsive: true,
      plugins: {
        title: {
          display: true,
          text: `Total energy consumption by device in the ${getTitleByMode(
            mode
          )}`,
        },
        tooltip: {
          callbacks: {
            label: (ctx) => `${ctx.label}: ${ctx.parsed} kWh`,
          },
        },
      },
    },
  });
}

function fetchAndRender() {
  // const mode = document.getElementById('mode').value;
  // API_URL.searchParams.append("device_id", `in.(${device})`);
  API_URL.searchParams.append("timestamp", `gte.${startTimee}T00:00:00Z`);
  API_URL.searchParams.append("timestamp", `lte.${endTimee}T23:59:59Z`);
  // API_URL.searchParams.append("select", `${enery}`);
  API_URL.searchParams.append("order", "timestamp.asc");
  // API_URL.searchParams.append("limit", "10");
  fetch(API_URL.toString(), {
    method: "GET",
    headers: {
      apikey: API_KEY,
      Authorization: `Bearer ${API_KEY}`,
      Accept: "application/json",
    },
  })
    .then((res) => res.json())
    .then((data) => {
      data.sort((a, b) => new Date(a.Timestamp) - new Date(b.Timestamp));
      processData(data, mode);
      console.log("data", data.length);
    })
    .catch((err) => {
      // alert("  Lỗi khi lấy dữ liệu: " + err);
    });
}

window.onload = fetchAndRender;
