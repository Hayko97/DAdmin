export function downloadFile(content, filename, contentType) {
    var a = document.createElement("a");
    var file = new Blob([content], {type: contentType});
    a.href = URL.createObjectURL(file);
    a.download = filename;
    a.click();
}

export function loadGuillFunctions() {
    window.quillFunctions = {
        createQuill: function (elementId) {
            var editor = document.getElementById(elementId);
            var quill = new Quill(editor, {
                theme: 'snow'
            });

            // Store the Quill instance for later use
            editor.quillInstance = quill;
        },

        getQuillContent: function (elementId) {
            var editor = document.getElementById(elementId);
            return editor.quillInstance.root.innerHTML;
        },

        setQuillContent: function (elementId, content) {
            var editor = document.getElementById(elementId);
            editor.quillInstance.root.innerHTML = content;
        }
    };
}

//*********************Charts
export function createMultiTableActivityChart(chartId, dates, datasets) {
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

export function createChart(canvasId, chartType, chartData, chartOptions) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    const chart = new Chart(ctx, {
        type: chartType,
        data: chartData,
        options: chartOptions
    });
    charts[canvasId] = chart;
}

export function updateChart(canvasId, chartType, chartData, chartOptions) {
    const chart = charts[canvasId];
    if (chart) {
        chart.data = chartData;
        chart.options = chartOptions;
        chart.update();
    } else {
        createChart(canvasId, chartType, chartData, chartOptions);
    }
}

//*****************************************Charts END

export function loadJS(url) {
    // Check if script with the given URL already exists
    if (!document.querySelector(`script[src="${url}"]`)) {
        var script = document.createElement("script");
        script.type = "text/javascript";
        script.src = url;
        document.getElementsByTagName("head")[0].appendChild(script);
    }
}

export function loadCSS(url) {
    // Check if link with the given URL already exists
    if (!document.querySelector(`link[href="${url}"]`)) {
        var link = document.createElement("link");
        link.rel = "stylesheet";
        link.type = "text/css";
        link.href = url;
        document.getElementsByTagName("head")[0].appendChild(link);
    }
}

loadJS("https://cdn.jsdelivr.net/npm/chart.js");
loadJS("https://cdn.quilljs.com/1.3.6/quill.js");

loadCSS("_content/DAdmin/css/admin.css");
