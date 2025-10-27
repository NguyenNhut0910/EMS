var dom = document.getElementById('container');
var myChart = echarts.init(dom, null, {
    renderer: 'canvas',
    useDirtyRect: false
});
var app = {};

var option;

option = {
title: {
    text: 'Warning chart of the last 15 days',
    left: 'center',
    top: 10, // 👈 Đẩy tiêu đề lên trên
    textStyle: {
        fontSize: 18,
        fontWeight: 'bold'
    }
},
tooltip: {
    trigger: 'axis'
},
legend: {
    data: ['Error', 'Warning'],
    // top: 30
    top: 35 // 👈 Đẩy legend lên cao hơn tí
},
grid: {
    top: 80,     // 👈 Đẩy biểu đồ xuống
    bottom: 40,  // 👈 Chừa khoảng dưới để dễ nhìn (nếu có label dài)
    left: 50,
    right: 30
},
xAxis: {
    type: 'category',
    boundaryGap: false,
    name: 'Date',
    nameLocation: 'middle',
    nameGap: 25,
    nameTextStyle: {
        fontSize: 12,
        fontWeight: 'bold'
    },
    data: [$Date],
    axisLabel: {
        fontSize: 10
    }
},
yAxis: {
    type: 'value',
    minInterval: 1,
    name: 'Count',
    nameLocation: 'middle',
    nameGap: 35,
    nameTextStyle: {
        fontSize: 12,
        fontWeight: 'bold'
    }
},
series: [
    {
        name: 'Error',
        type: 'line',
        data: [$Error],
        symbolSize: 12,
        itemStyle: {
            color: '#FF4C4C'
        },
        label: {
            show: true,
            position: 'top'
        }
    },
    {
        name: 'Warning',
        type: 'line',
        data: [$Warn],
        symbolSize: 12,
        itemStyle: {
            color: '#FFA500'
        },
        label: {
            show: true,
            position: 'top'
        }
    }
]
};

if (option && typeof option === 'object') {
    myChart.setOption(option);
}

window.addEventListener('resize', myChart.resize);