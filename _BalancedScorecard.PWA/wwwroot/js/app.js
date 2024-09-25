function saveTextFile(fileName, content) {
    const blob = new Blob([content], { type: 'text/plain' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function drawPlot(elementId, data, layout) {
    var chartData = JSON.parse(data);
    var chartLayout = JSON.parse(layout);

    Plotly.newPlot(elementId, chartData, chartLayout);
}


