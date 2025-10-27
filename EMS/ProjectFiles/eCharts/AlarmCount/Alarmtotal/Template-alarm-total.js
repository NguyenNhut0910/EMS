var dom = document.getElementById('container');
var myChart = echarts.init(dom, null, {
    renderer: 'canvas',
    useDirtyRect: false
});
var app = {};
var option;
option = {
  backgroundColor: "transparent",
  title: [
    {
      text: $Warn + $Error,
      subtext: "Total",
      left: "center",
      top: "center",
      textStyle: {
        fontSize: 28,
        fontWeight: "normal",
        color: "#333",
      },
      subtextStyle: {
        fontSize: 14,
        color: "#666",
      },
    },
    {
      text: "$Date",
      left: "center",
      // bottom: 40,   // đẩy ngày xuống gần lề dưới
      top: 5,
      textStyle: {
        fontSize: 14,
        color: "#666",
        lineHeight: 20,
      },
    },
  ],
  tooltip: {
    trigger: "item",
  },
  legend: {
    bottom: 10,
    left: "center",
  },
  series: [
    {
      name: "Status",
      type: "pie",
      radius: ["40%", "70%"],
      avoidLabelOverlap: false,
      itemStyle: {
        borderRadius: 10,
        borderColor: "#fff",
        borderWidth: 2,
      },
      emphasis: {
        label: {
          show: true,
          fontSize: 24,
          fontWeight: "bold",
        },
      },
      labelLine: {
        show: false,
      },
      data: [
        { value: $Error, name: "Error", itemStyle: { color: "#ff4757" } },
        { value: $Warn, name: "Warning", itemStyle: { color: "#ffa502" } },
      ],
    },
  ],
};

if (option && typeof option === 'object') {
    myChart.setOption(option);
}

window.addEventListener('resize', myChart.resize);