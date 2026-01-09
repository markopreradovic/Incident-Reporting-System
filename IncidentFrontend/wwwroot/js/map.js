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
    if (!window.approvedMap) {
        window.initApprovedMap();
    }

    const map = window.approvedMap;

    map.eachLayer(layer => {
        if (layer instanceof L.Marker) {
            map.removeLayer(layer);
        }
    });

    if (incidents.length === 0) {
        L.marker([44.7722, 17.1911]).addTo(map)
            .bindPopup("Nema odobrenih incidenata za odabrani filter")
            .openPopup();
        return;
    }

    const markers = [];
    incidents.forEach(inc => {
        const popupContent = `
        <b>${inc.description}</b><br>
        ID: ${inc.id}<br>
        Vrsta: ${inc.typeId}<br>
        Datum: ${new Date(inc.createdAt).toLocaleString('sr-RS')}<br>
        ${inc.imageUrl ? `<img src="${inc.imageUrl}" alt="Slika incidenta" style="max-width:200px; margin-top:8px; border-radius:6px;">` : '<i>Nema slike</i>'}
    `;

        const marker = L.marker([inc.latitude, inc.longitude])
            .addTo(map)
            .bindPopup(popupContent);

        markers.push(marker);
    });

    const group = new L.featureGroup(markers);
    map.fitBounds(group.getBounds().pad(0.1));
};