window.initMap = (dotNetHelper) => {
    const map = L.map('map').setView([44.7722, 17.1911], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    let marker;

    map.on('click', function (e) {
        if (marker) {
            map.removeLayer(marker);
        }
        marker = L.marker(e.latlng).addTo(map)
            .bindPopup("Odabrana lokacija")
            .openPopup();

        // Pošalji koordinate u C#
        dotNetHelper.invokeMethodAsync('SetLocation', e.latlng.lat, e.latlng.lng);
    });

    // Geolocation
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(position => {
            const lat = position.coords.latitude;
            const lng = position.coords.longitude;
            map.setView([lat, lng], 15);
            L.marker([lat, lng]).addTo(map)
                .bindPopup("Vaša trenutna lokacija")
                .openPopup();
            dotNetHelper.invokeMethodAsync('SetLocation', lat, lng);
        });
    }
};