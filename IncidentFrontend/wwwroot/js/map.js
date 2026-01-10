window.initMap = (dotNetHelper) => {
    const map = L.map('map').setView([44.7722, 17.1911], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    let marker;

    map.on('click', function (e) {
        if (marker) {
            map.removeLayer(marker);
        }
        marker = L.marker(e.latlng).addTo(map)
            .bindPopup(`Odabrana lokacija<br>Lat: ${e.latlng.lat.toFixed(6)}<br>Lng: ${e.latlng.lng.toFixed(6)}`)
            .openPopup();

        dotNetHelper.invokeMethodAsync('UpdateLocation', e.latlng.lat, e.latlng.lng);
    });

    const locateControl = L.control({ position: 'topleft' });
    locateControl.onAdd = function () {
        const div = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-custom');
        div.innerHTML = '<span style="font-size: 1.5em;">📍</span>';
        div.title = 'Pronađi moju lokaciju';
        div.style.background = 'white';
        div.style.width = '34px';
        div.style.height = '34px';
        div.style.textAlign = 'center';
        div.style.cursor = 'pointer';
        div.style.border = '2px solid rgba(0,0,0,0.2)';
        div.style.borderRadius = '4px';

        div.onclick = function (e) {
            e.stopPropagation();
            navigator.geolocation.getCurrentPosition(pos => {
                const lat = pos.coords.latitude;
                const lng = pos.coords.longitude;
                map.setView([lat, lng], 15);
                if (marker) map.removeLayer(marker);
                marker = L.marker([lat, lng]).addTo(map)
                    .bindPopup('Vaša trenutna lokacija')
                    .openPopup();
                dotNetHelper.invokeMethodAsync('UpdateLocation', lat, lng);
            }, () => {
                alert('Geolokacija nije dozvoljena ili nije dostupna.');
            });
        };
        return div;
    };
    locateControl.addTo(map);
};

window.initApprovedMap = () => {
    if (window.approvedMap) {
        window.approvedMap.remove();
    }

    const map = L.map('approved-map').setView([44.7722, 17.1911], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    window.approvedMap = map;
};

window.showApprovedOnMap = (incidents) => {
    if (!window.approvedMap) {
        window.initApprovedMap();
    }

    const map = window.approvedMap;

    map.eachLayer(layer => {
        if (layer instanceof L.Marker || layer instanceof L.FeatureGroup) {
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
        const imgTag = inc.imageUrl
            ? `<img src="${inc.imageUrl}" alt="Slika incidenta" style="max-width: 100%; height: auto; max-height: 180px; border-radius: 8px; margin-top: 8px; display: block;" onerror="this.onerror=null; this.src='https://via.placeholder.com/300x180?text=Slika+nije+dostupna';" />`
            : `<p style="color: #95a5a6; font-style: italic; margin-top: 10px;">📷 Nema slike</p>`;

        const popupContent = `
            <div style="font-family: Arial, sans-serif; max-width: 280px; padding: 5px;">
                <h5 style="margin: 0 0 8px; color: #2c3e50;">Incident ID: ${inc.id}</h5>
                <p style="margin: 4px 0; font-weight: bold;">${inc.description || 'Bez opisa'}</p>
                <p style="margin: 4px 0;"><strong>Vrsta:</strong> ${inc.typeId || 'Nepoznato'}</p>
                <p style="margin: 4px 0;"><strong>Datum:</strong> ${new Date(inc.createdAt).toLocaleString('sr-RS')}</p>
                <p style="margin: 4px 0;"><strong>Lokacija:</strong> ${inc.latitude.toFixed(6)}, ${inc.longitude.toFixed(6)}</p>
                ${imgTag}
            </div>
        `;

        const marker = L.marker([inc.latitude, inc.longitude])
            .addTo(map)
            .bindPopup(popupContent, {
                maxWidth: 300,
                minWidth: 250,
                className: 'custom-popup'
            });

        markers.push(marker);
    });

    if (markers.length > 0) {
        const group = new L.featureGroup(markers);
        map.fitBounds(group.getBounds().pad(0.1));
    }
};