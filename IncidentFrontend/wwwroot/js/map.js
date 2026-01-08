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

window.showApprovedOnMap = (incidents) => {
    if (!incidents || !Array.isArray(incidents)) {
        console.warn('Nema incidenata za prikaz');
        incidents = [];
    }

    if (window.approvedMap) {
        window.approvedMap.remove();
    }

    const map = L.map('approved-map').setView([44.7722, 17.1911], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    if (incidents.length === 0) {
        L.marker([44.7722, 17.1911]).addTo(map)
            .bindPopup("Nema odobrenih incidenata za odabrani filter")
            .openPopup();
        window.approvedMap = map;
        return;
    }

    incidents.forEach(inc => {
        L.marker([inc.latitude, inc.longitude])
            .addTo(map)
            .bindPopup(`<b>${inc.description}</b><br>ID: ${inc.id}<br>Vrsta: ${inc.typeId}`);
    });

    const group = new L.featureGroup(incidents.map(inc =>
        L.marker([inc.latitude, inc.longitude])
    ));
    map.fitBounds(group.getBounds().pad(0.1));

    window.approvedMap = map;
};