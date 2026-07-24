window.dashboardCharts = {

    destroyChart(canvas) {
        if (canvas.chart) {
            canvas.chart.destroy();
            canvas.chart = null;
        }
    },

    renderPieChart(canvasId, labels, values) {

        const canvas = document.getElementById(canvasId);

        if (!canvas)
            return;

        this.destroyChart(canvas);

        canvas.chart = new Chart(canvas, {

            type: 'pie',

            data: {
                labels: labels,
                datasets: [{
                    data: values
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    },

    renderBarChart(canvasId, labels, values) {

        const canvas = document.getElementById(canvasId);

        if (!canvas)
            return;

        this.destroyChart(canvas);

        canvas.chart = new Chart(canvas, {

            type: 'bar',

            data: {

                labels: labels,

                datasets: [{
                    label: 'Cases',
                    data: values
                }]
            },

            options: {

                responsive: true,
                maintainAspectRatio: false,

                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    },

    renderLineChart(canvasId, labels, values) {

        const canvas = document.getElementById(canvasId);

        if (!canvas)
            return;

        this.destroyChart(canvas);

        canvas.chart = new Chart(canvas, {

            type: 'line',

            data: {

                labels: labels,

                datasets: [{
                    label: 'Cases',
                    data: values,
                    tension: 0.3,
                    fill: false
                }]
            },

            options: {

                responsive: true,
                maintainAspectRatio: false,

                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    },

    renderGroupedBarChart(canvasId, labels, retrieved, created, errors) {

        const canvas = document.getElementById(canvasId);

        if (!canvas)
            return;

        this.destroyChart(canvas);

        canvas.chart = new Chart(canvas, {

            type: 'bar',

            data: {

                labels: labels,

                datasets: [

                    {
                        label: 'Retrieved',
                        data: retrieved
                    },

                    {
                        label: 'Cases Created',
                        data: created
                    },

                    {
                        label: 'Errors',
                        data: errors
                    }

                ]
            },

            options: {

                responsive: true,
                maintainAspectRatio: false,

                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    },

    renderDoughnutChart: function (canvasId, labels, values) {

        const canvas = document.getElementById(canvasId);

        if (!canvas)
            return;

        if (canvas.chart)
            canvas.chart.destroy();

        canvas.chart = new Chart(canvas, {
            type: 'doughnut',

            data: {
                labels: labels,

                datasets: [{
                    data: values
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }
};