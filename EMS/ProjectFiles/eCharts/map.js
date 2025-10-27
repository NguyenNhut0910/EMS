const map = L.map('map').setView([15.9, 108], 6);

L.tileLayer('./vn_map_tiles/{z}/{x}/{y}.png', {
  minZoom: 6,
  maxZoom: 10,
  bounds: L.latLngBounds([6, 101], [24, 120]),
  noWrap: true,
  errorTileUrl: "blank.png"
}).addTo(map);

// Tạo icon động theo zoom
function createDynamicIcon(zoom) {
  const size = Math.max(16, zoom * 4);
  return L.divIcon({
    className: '',
    html: `<div class="zoom-icon" style="font-size: ${size}px;">📍</div>`,
    iconSize: [size, size],
    iconAnchor: [size / 2, size],
    popupAnchor: [0, -size]
  });
}

// Dữ liệu marker
const markerData = [
  {
    pos: [10.9804, 106.6519],
    content: `<div class="popup-custom"><b>🏭 Bình Dương</b><br>Công suất: <b>120 kW</b><br>Trạng thái: <b>Cảnh báo</b></div>`
  },
  {
    pos: [21.1820, 106.0511],
    content: `<div class="popup-custom"><b>🏭 Bắc Ninh</b><br>Công suất: <b>90 kW</b><br>Trạng thái: <b>Bình thường</b></div>`
  }
];

let markers = [];

function refreshMarkers() {
  markers.forEach(m => map.removeLayer(m));
  markers = markerData.map(data =>
    L.marker(data.pos, { icon: createDynamicIcon(map.getZoom()) })
      .addTo(map)
      .bindPopup(data.content)
  );
}

map.on('zoomend', refreshMarkers);
refreshMarkers();
