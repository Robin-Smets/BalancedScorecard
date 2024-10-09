function createPlot(elementId, data, layout) {
    Plotly.newPlot(elementId, data, layout);

    var plotElement = document.getElementById(elementId);

    plotElement.on('plotly_afterplot', function () {
        console.log('Plot fully rendered');

        plotElement.on('plotly_doubleclick', function (data) {
            console.log('Plot double clicked');
        });
        plotElement.on('plotly_click', function (data) {
            console.log('Plot clicked');
            if (data.points && data.points.length > 0) {
                console.log('Data has points');
                var point = data.points[0];
                var label = point.label;
                var value = point.value;
                DotNet.invokeMethodAsync('BalancedScorecard', 'OnPlotClick', elementId, label, value);
            }
        });
    });




}
