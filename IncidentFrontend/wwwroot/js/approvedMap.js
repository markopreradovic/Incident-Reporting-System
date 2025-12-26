window.showApprovedOnMap = (incidents) => {
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
        return;
    }

    incidents.forEach(inc => {
        L.marker([inc.latitude, inc.longitude])
            .addTo(map)
            .bindPopup(`<b>${inc.description}</b><br>ID: ${inc.id}<br>Vrsta: ${inc.typeId}`);
    });

    const group = new L.featureGroup(incidents.map(inc => L.marker([inc.latitude, inc.longitude])));
    map.fitBounds(group.getBounds().pad(0.1));

    window.approvedMap = map;
};