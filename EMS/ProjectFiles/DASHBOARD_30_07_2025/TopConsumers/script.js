const API_URL = "http://nhatvietindustry.ddns.net:22321/rest/v1/data";
const DEVICE_URL =
  "http://nhatvietindustry.ddns.net:22321/rest/v1/device?select=device_id,name";
const API_KEY =
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";
const headers = {
  apikey: API_KEY,
  Authorization: `Bearer ${API_KEY}`,
};
const devices = [
  { id: 1, name: "Pulp Plant" },
  { id: 2, name: "Office Building" },
  { id: 3, name: "Paper Plant" },
  { id: 4, name: "Water Treatment Plant" },
];
const toISOString = (d) => d.toISOString().split(".")[0] + "Z";

const getWeekRange = (offset = 0) => {
  const now = new Date();
  const monday = new Date(
    now.setDate(now.getDate() - now.getDay() + 1 + offset * 7)
  );
  monday.setHours(0, 0, 0, 0);
  const sunday = new Date(monday);
  sunday.setDate(monday.getDate() + 6);
  sunday.setHours(23, 59, 59, 999);
  return [toISOString(monday), toISOString(sunday)];
};
const formatDate = (dateStr) => {
  const d = new Date(dateStr);
  const day = String(d.getDate()).padStart(2, "0");
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const year = d.getFullYear();
  return `${day}/${month}/${year}`;
};

const updateColumnTitles = (currStart, currEnd, prevStart, prevEnd) => {
  document.getElementById(
    "curr-title"
  ).innerHTML = `Current week's capacity<br><span class="date-under">${formatDate(
    currStart
  )} - ${formatDate(currEnd)}</span>`;
  document.getElementById(
    "prev-title"
  ).innerHTML = `Previous week's capacity<br><span class="date-under">${formatDate(
    prevStart
  )} - ${formatDate(prevEnd)}</span>`;
};

// const [currStart, currEnd] = getWeekRange(0);
// const [prevStart, prevEnd] = getWeekRange(-1);
const [currStart, currEnd] = "2025-07-20T17:00:00Z 2025-07-27T16:59:59Z".split(
  " "
);
const [prevStart, prevEnd] = "2025-07-13T17:00:00Z 2025-07-20T16:59:59Z".split(
  " "
);
updateColumnTitles(currStart, currEnd, prevStart, prevEnd);

const fetchWeekData = async (start, end) => {
  const params = new URLSearchParams([
    ["timestamp", `gte.${start}`],
    ["timestamp", `lte.${end}`],
    ["order", "timestamp.asc"],
  ]);
  const res = await fetch(`${API_URL}?${params}`, { headers });
  if (!res.ok) throw new Error("Lỗi tải dữ liệu");
  return await res.json();
};

const fetchDeviceNames = async () => {
  const res = await fetch(DEVICE_URL, { headers });
  if (!res.ok) throw new Error("Lỗi tải tên thiết bị");
  const devices = await res.json();
  // Trả về map: {device_id: name}
  return Object.fromEntries(devices.map((d) => [d.device_id, d.name]));
};

const calculateEnergy = (records) => {
  const grouped = {};
  for (let i = 1; i < records.length; i++) {
    const prev = records[i - 1];
    const curr = records[i];
    if (curr.device_id !== prev.device_id) continue;

    const t1 = new Date(prev.timestamp).getTime();
    const t2 = new Date(curr.timestamp).getTime();
    const deltaHrs = (t2 - t1) / 3600000;
    const energy = (prev.power / 1000) * deltaHrs;

    if (!grouped[curr.device_id]) grouped[curr.device_id] = 0;
    grouped[curr.device_id] += energy;
  }
  return grouped;
};

const buildTable = (currData, prevData, deviceMap) => {
  console.log("Dữ liệu hiện tại:", deviceMap);

  const tbody = document.getElementById("ranking-body");
  tbody.innerHTML = "";

  const totalCurr = Object.values(currData).reduce((a, b) => a + b, 0);

  const devices = Object.entries(currData).map(([device_id, energy]) => {
    return {
      device_id,
      name: deviceMap[device_id],
      curr: energy,
      prev: prevData[device_id] || 0,
      delta: energy - (prevData[device_id] || 0),
    };
  });

  devices.sort((a, b) => b.curr - a.curr);

  devices.forEach((d, index) => {
    const percent = (d.curr / totalCurr) * 100;
    const tr = document.createElement("tr");

    tr.innerHTML = `
      <td>${index + 1}.    ${d.name}</td>
      <td>
        <div class="bar-container">
          <div class="bar" style="width: ${percent}%; background: #3498db;"></div>
        </div>
        <span class="bar-value">${d.curr.toFixed(2)} kWh</span>
        <span class="bar-icon">
          ${
            d.delta >= 0
              ? `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="red" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M3 17l6 -6l4 4l8 -8" /><path d="M14 7l7 0l0 7" /></svg>`
              : `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="green" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M3 7l6 6l4 -4l8 8" /><path d="M21 10l0 7l-7 0" /></svg>`
          }
        </span>
      </td>
      <td>
        <div class="bar-container">
          <div class="bar" style="width: ${
            (d.prev / totalCurr) * 100
          }%; background: #95a5a6;"></div>
        </div>
        <span class="bar-value">${d.prev.toFixed(2)} kWh</span>
      </td>
    `;
    tbody.appendChild(tr);
  });
};

const load = async () => {
  try {
    const [currRaw, prevRaw, deviceMap] = await Promise.all([
      fetchWeekData(currStart, currEnd),
      fetchWeekData(prevStart, prevEnd),
      fetchDeviceNames(),
    ]);

    const currGrouped = calculateEnergy(currRaw);
    const prevGrouped = calculateEnergy(prevRaw);
    buildTable(currGrouped, prevGrouped, deviceMap);
  } catch (err) {
    console.error("Lỗi xử lý:", err);
  }
};

load();
