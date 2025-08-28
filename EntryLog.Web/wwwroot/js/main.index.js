(() => {
    'use strict'

    // Map
    var map = L.map('map').setView([4.5709, -74.2973], 5);

    // Capa base
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    $.ajax({
        url: '/menu/ultimas_locaciones',
        type: 'GET',
        async: true,
        success: (result) => {

            let locations = result.locations;

            const tbody = document.getElementById('list-locations');

            locations.forEach((l, index) => {

                let row = document.createElement('tr');
                row.innerHTML = `
                    <td>
                        <div class="flag">
                            <img src="/lib/kaiadmin/assets/img/flags/co.png" alt="colombia">
                        </div>
                    </td>
                    <td>${l.city}, ${l.country}</td>
                    <td class="text-end">
                        ${l.latitude}
                    </td>
                    <td class="text-end">
                        ${l.longitude}
                    </td>
                `;

                tbody.appendChild(row, tbody.lastChild);

                L.marker([Number(l.latitude), Number(l.longitude)]).addTo(map)
                    .bindPopup(`${l.city}`);
            });
        },
        error: (err) => {
            console.log("Error: ", err);
        }
    });
})();