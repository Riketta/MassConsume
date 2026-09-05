"""Regenerates Textures/UI/Commands/MassConsume.png (32x32 RGBA icon).

Draws a plate with a fork - the shared "consume from pockets" command icon.
No external dependencies: hand-rolled PNG chunks with zlib.
Usage:  py -3 make_icon.py   (from Source/MassConsume/, or adjust the output path)
"""
import os
import struct
import zlib

W = H = 32
OUT = (0, 0, 0, 0)
OUTLINE = (44, 44, 54, 255)
PLATE = (172, 172, 182, 255)
RIM = (140, 140, 152, 255)
FOOD = (216, 168, 108, 255)
FORK = (206, 206, 214, 255)
STEAM = (206, 206, 214, 170)

px = [[OUT] * W for _ in range(H)]


def rect(x0, y0, x1, y1, c):
    for y in range(int(y0), int(y1) + 1):
        for x in range(int(x0), int(x1) + 1):
            if 0 <= x < W and 0 <= y < H:
                px[y][x] = c


def ellipse(cx, cy, rx, ry, c):
    for y in range(H):
        for x in range(W):
            dx = (x - cx) / rx
            dy = (y - cy) / ry
            if dx * dx + dy * dy <= 1.0:
                px[y][x] = c


# Plate: dark outline ring, fill, inner rim, food mound.
ellipse(20, 22, 11.5, 6.4, OUTLINE)
ellipse(20, 22, 10.5, 5.5, PLATE)
ellipse(20, 22, 7.0, 3.6, RIM)
ellipse(20, 21.5, 4.8, 2.4, FOOD)

# Fork: shadow pass then fill, tines up.
rect(4, 4, 4, 13, OUTLINE)
rect(6, 4, 6, 13, OUTLINE)
rect(8, 4, 8, 13, OUTLINE)
rect(4, 12, 8, 14, OUTLINE)
rect(5, 14, 7, 27, OUTLINE)
rect(4, 4, 4, 12, FORK)
rect(6, 4, 6, 12, FORK)
rect(8, 4, 8, 12, FORK)
rect(4, 12, 8, 13, FORK)
rect(5, 13, 7, 26, FORK)

# Steam above the meal.
rect(15, 13, 15, 11, STEAM)
rect(16, 10, 16, 9, STEAM)
rect(15, 8, 15, 7, STEAM)
rect(24, 13, 24, 11, STEAM)
rect(23, 10, 23, 9, STEAM)
rect(24, 8, 24, 7, STEAM)

raw = b''
for y in range(H):
    raw += b'\x00' + b''.join(bytes(px[y][x]) for x in range(W))


def chunk(tag, data):
    c = struct.pack('>I', len(data)) + tag + data
    return c + struct.pack('>I', zlib.crc32(tag + data) & 0xffffffff)


png = b'\x89PNG\r\n\x1a\n'
png += chunk(b'IHDR', struct.pack('>IIBBBBB', W, H, 8, 6, 0, 0, 0))
png += chunk(b'IDAT', zlib.compress(raw, 9))
png += chunk(b'IEND', b'')

out = os.path.join(os.path.dirname(__file__), '..', '..', 'Textures', 'UI', 'Commands', 'MassConsume.png')
with open(out, 'wb') as f:
    f.write(png)
print('written', len(png), 'bytes to', os.path.normpath(out))
