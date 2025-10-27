import os
import requests
from math import log, tan, radians, pi, floor, cos

# URL tile của MapTech - đúng chủ quyền VN
TILE_URL = "https://tiles.mattech.vn/styles/basic/{z}/{x}/{y}.png"
output_folder = "vn_map_tiles"
zoom_levels = range(6, 11)  # Zoom từ 6 đến 10

# Biên độ lãnh thổ VN (có cả Hoàng Sa, Trường Sa)
LAT_MIN, LAT_MAX = 6.0, 24.0
LON_MIN, LON_MAX = 101.0, 120.0

def latlon_to_tile(lat, lon, z):
    lat_rad = radians(lat)
    n = 2.0 ** z
    xtile = int((lon + 180.0) / 360.0 * n)
    ytile = int((1.0 - log(tan(lat_rad) + 1 / cos(lat_rad)) / pi) / 2.0 * n)
    return xtile, ytile

for z in zoom_levels:
    x_min, y_max = latlon_to_tile(LAT_MIN, LON_MIN, z)  # Góc dưới trái
    x_max, y_min = latlon_to_tile(LAT_MAX, LON_MAX, z)  # Góc trên phải

    print(f"[Zoom {z}] Tải từ x={x_min} đến x={x_max}, y={y_min} đến y={y_max}")

    for x in range(x_min, x_max + 1):
        for y in range(y_min, y_max + 1):
            url = TILE_URL.format(z=z, x=x, y=y)
            save_dir = os.path.join(output_folder, str(z), str(x))
            os.makedirs(save_dir, exist_ok=True)
            file_path = os.path.join(save_dir, f"{y}.png")

            if not os.path.exists(file_path):
                try:
                    r = requests.get(url, timeout=5)
                    if r.status_code == 200:
                        with open(file_path, "wb") as f:
                            f.write(r.content)
                        print(f"✔ Saved: z={z}, x={x}, y={y}")
                    else:
                        print(f"✘ Empty tile: z={z}, x={x}, y={y}")
                except Exception as e:
                    print(f"⚠️ Error: z={z}, x={x}, y={y} | {e}")
