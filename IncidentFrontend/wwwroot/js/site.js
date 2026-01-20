window.drawAnalyticsCharts = (typeStats, dateStats) => {
    console.log("Drawing charts with data:", typeStats, dateStats);

    if (window.typeChart && typeof window.typeChart.destroy === 'function') {
        window.typeChart.destroy();
    }
    if (window.dateChart && typeof window.dateChart.destroy === 'function') {
        window.dateChart.destroy();
    }

    const typeCtx = document.getElementById('typeChart');
    if (typeCtx && typeStats && typeStats.length > 0) {
        window.typeChart = new Chart(typeCtx, {
            type: 'doughnut',
            data: {
                labels: typeStats.map(s => s.typeName),
                datasets: [{
                    data: typeStats.map(s => s.count),
                    backgroundColor: [
                        'rgba(59, 130, 246, 0.8)',   // plava
                        'rgba(16, 185, 129, 0.8)',   // zelena
                        'rgba(245, 158, 11, 0.8)',   // narandžasta
                        'rgba(239, 68, 68, 0.8)',    // crvena
                        'rgba(139, 92, 246, 0.8)'    // ljubičasta
                    ],
                    borderColor: [
                        'rgba(59, 130, 246, 1)',
                        'rgba(16, 185, 129, 1)',
                        'rgba(245, 158, 11, 1)',
                        'rgba(239, 68, 68, 1)',
                        'rgba(139, 92, 246, 1)'
                    ],
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: '#e0e0e0',
                            font: {
                                size: 12,
                                family: "'Open Sans', sans-serif"
                            },
                            padding: 15
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(30, 30, 50, 0.95)',
                        titleColor: '#ffffff',
                        bodyColor: '#e0e0e0',
                        borderColor: 'rgba(255, 255, 255, 0.1)',
                        borderWidth: 1,
                        padding: 12,
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.parsed || 0;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    const dateCtx = document.getElementById('dateChart');
    if (dateCtx && dateStats && dateStats.length > 0) {
        window.dateChart = new Chart(dateCtx, {
            type: 'line',
            data: {
                labels: dateStats.map(s => s.date),
                datasets: [{
                    label: 'Broj prijava',
                    data: dateStats.map(s => s.count),
                    borderColor: 'rgba(59, 130, 246, 1)',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    fill: true,
                    tension: 0.4,
                    borderWidth: 3,
                    pointBackgroundColor: 'rgba(59, 130, 246, 1)',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            color: '#b8c5d6',
                            stepSize: 1
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.05)',
                            drawBorder: false
                        }
                    },
                    x: {
                        ticks: {
                            color: '#b8c5d6',
                            maxRotation: 45,
                            minRotation: 45
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.05)',
                            drawBorder: false
                        }
                    }
                },
                plugins: {
                    legend: {
                        labels: {
                            color: '#e0e0e0',
                            font: {
                                size: 12,
                                family: "'Open Sans', sans-serif"
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(30, 30, 50, 0.95)',
                        titleColor: '#ffffff',
                        bodyColor: '#e0e0e0',
                        borderColor: 'rgba(255, 255, 255, 0.1)',
                        borderWidth: 1,
                        padding: 12
                    }
                }
            }
        });
    }

    console.log("Charts created successfully");
};