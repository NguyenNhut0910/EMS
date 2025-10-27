const API_URL = "http://nhatvietindustry.ddns.net:22321/rest/v1";
const API_KEY =
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";

const headers = {
  apikey: API_KEY,
  Authorization: `Bearer ${API_KEY}`,
};

async function fetchData() {
  const [areaRes, machineRes, deviceRes, dataRes] = await Promise.all([
    fetch(`${API_URL}/area`, { headers }),
    fetch(`${API_URL}/machine`, { headers }),
    fetch(`${API_URL}/device`, { headers }),
    // fetch(`${API_URL}/data?select=device_id,power,timestamp&order=timestamp.desc`, { headers }),
    fetch(
      `${API_URL}/data?select=device_id,power,timestamp&order=timestamp.asc`,
      { headers }
    ),
  ]);

  const areas = await areaRes.json();
  const machines = await machineRes.json();
  const devices = await deviceRes.json();
  const rawData = await dataRes.json();

  return { areas, machines, devices, rawData };
}

function processData({ areas, machines, devices, rawData }) {
  const areaMap = Object.fromEntries(
    areas.map((a) => [a.area_id, a.area_name])
  );
  const machineMap = Object.fromEntries(machines.map((m) => [m.machine_id, m]));
  const deviceToMachine = Object.fromEntries(
    devices.map((d) => [d.device_id, d.machine_id])
  );

  const latestPower = {};
  for (const row of rawData) {
    const did = row.device_id;
    if (!latestPower[did]) latestPower[did] = row;
  }

  const powerByMachine = {};
  for (const [device_id, row] of Object.entries(latestPower)) {
    const machine_id = deviceToMachine[device_id];
    if (!machine_id) continue;
    const power = row.power || 0;
    powerByMachine[machine_id] = (powerByMachine[machine_id] || 0) + power;
  }

  // Gom theo khu vực
  const areaGrouped = {};
  for (const [machine_id, power] of Object.entries(powerByMachine)) {
    const machine = machineMap[machine_id];
    if (!machine) continue;
    const areaName = areaMap[machine.area_id] || "Không rõ";
    if (!areaGrouped[areaName]) areaGrouped[areaName] = [];
    areaGrouped[areaName].push({
      name: machine.name,
      value: power,
    });
  }

  return areaGrouped;
}

function drawMultipleCharts(data) {
  const container = document.getElementById("chart");
  container.innerHTML = ""; // clear cũ nếu có

  Object.entries(data).forEach(([areaName, machineList], index) => {
    const chartDiv = document.createElement("div");
    chartDiv.style.width = "100%";
    chartDiv.style.height = "200px";
    // chartDiv.style.marginTop = "0px";
    container.appendChild(chartDiv);

    const chart = echarts.init(chartDiv);

    // const option = {
    //   title: {
    //     text: `Khu vực: ${areaName}`,
    //     left: "center",
    //     top:20,
    //     textStyle: {
    //       fontSize: 16,
    //       fontWeight: "bold",
    //     },
    //   },
    //   tooltip: {
    //     trigger: "item",
    //     formatter: "{b}<br/>⚡ {c} W ({d}%)"
    //   },
    //   legend: {
    //     orient: "vertical",
    //     left: "left",

    //   },
    //   series: [
    //     {
    //       name: "Công suất",
    //       type: "pie",
    //       radius: "60%",
    //       data: machineList,
    //       emphasis: {
    //         itemStyle: {
    //           shadowBlur: 10,
    //           shadowOffsetX: 0,
    //           shadowColor: "rgba(0, 0, 0, 0.5)",
    //         },
    //       },
    //     },

    //   ],
    // };
    const option = {
      title: {
        text: `Khu vực: ${areaName}`,
        left: "center",
        top: 20,
        textStyle: {
          fontSize: 16,
          fontFamily: "Quicksand, Arial, sans-serif",
        },
      },
      tooltip: {
        trigger: "item",
        formatter: "{b}<br/>⚡ {c} W ({d}%)",
      },
      legend: {
        orient: "vertical",
        right: 0, // Sát mép phải
        top: "middle",
        type: "scroll", // Nếu có nhiều item
        itemWidth: 12, // Thu nhỏ icon
        itemGap: 8,
        textStyle: {
          fontSize: 8,
        },
      },
      series: [
        {
          name: "Công suất",
          type: "pie",
          labelLine: {
            show: false,
          },
          label: {
            show: false,
          },
          radius: "50%", // Nhỏ hơn để vừa trong khung
          center: ["30%", "50%"], // Dịch sang trái để nhường chỗ legend
          data: machineList,
          emphasis: {
            itemStyle: {
              shadowBlur: 10,
              shadowOffsetX: 0,
              shadowColor: "rgba(0, 0, 0, 0.5)",
            },
          },
        },
      ],
    };

    chart.setOption(option);
  });
}

(async () => {
  try {
    const raw = await fetchData();
    const groupedData = processData(raw);
    drawMultipleCharts(groupedData);
  } catch (err) {
    console.error("Lỗi khi tải dữ liệu:", err);
  }
})();
