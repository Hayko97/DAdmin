function createMultiTableActivityChart(chartId, dates, datasets) {
    console.log('Dates:', dates);
    console.log('Datasets:', datasets);

    var canvas = document.getElementById(chartId);
    if (!canvas) {
        console.error('No canvas element found with id:', chartId);
        return;
    }
    var ctx = canvas.getContext('2d');

    var chartDatasets = datasets.map(function (dataset) {
        return {
            label: dataset.label,
            data: dataset.data,
            fill: false,
            borderColor: getRandomColor(),
            borderWidth: 1
        };
    });
    console.log(chartDatasets);
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: dates,
            datasets: chartDatasets
        },
        options: {
            responsive: true,
            title: {
                display: true,
                text: 'Activity by Table'
            },
            scales: {
                xAxes: [{
                    display: true,
                    scaleLabel: {
                        display: true,
                        labelString: 'Date'
                    }
                }],
                yAxes: [{
                    display: true,
                    scaleLabel: {
                        display: true,
                        labelString: 'Activity Count'
                    },
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }
    });
}


function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}


let charts = {};

function createChart(canvasId, chartType, chartData, chartOptions) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    const chart = new Chart(ctx, {
        type: chartType,
        data: chartData,
        options: chartOptions
    });
    charts[canvasId] = chart;
}

function updateChart(canvasId, chartType, chartData, chartOptions) {
    const chart = charts[canvasId];
    if (chart) {
        chart.data = chartData;
        chart.options = chartOptions;
        chart.update();
    } else {
        createChart(canvasId, chartType, chartData, chartOptions);
    }
}

function loadScript(url, callback) {
    var script = document.createElement("script");
    script.type = "text/javascript";

    if (script.readyState) {  // Only required for IE <9
        script.onreadystatechange = function () {
            if (script.readyState === "loaded" || script.readyState === "complete") {
                script.onreadystatechange = null;
                callback();
            }
        };
    } else {  // Others
        script.onload = function () {
            callback();
        };
    }

    script.src = url;
    document.getElementsByTagName("head")[0].appendChild(script);
}

loadScript("https://cdn.jsdelivr.net/npm/chart.js", function () {
    console.log("jQuery loaded!");
});