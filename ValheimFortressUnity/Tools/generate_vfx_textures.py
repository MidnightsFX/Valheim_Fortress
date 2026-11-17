"""Generates Valheim Fortress's particle textures and their materials, so the portal and creature-marker effects need
no third-party art. Output: Assets/Custom/Textures/VFX. Rerunning keeps every GUID, so prefabs stay linked.

    python -I Tools/generate_vfx_textures.py            (from ValheimFortressUnity)

Sprite sheets are laid out the way Unity's texture sheet animation reads them: frame 0 top left, then along the row.
"""
import os
import re
import uuid

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "Custom", "Textures", "VFX")
MOCK_BLENDED = "b02aafe0c15dbc4479670717401b6aa1"  # Assets/Custom/Mocks/Shaders/Mock_Legacy_Blended.shader
BUILTIN_ADDITIVE = 200                             # Unity's built-in Legacy Shaders/Particles/Additive


def grid(size):
    c = (np.arange(size) + 0.5) / size * 2.0 - 1.0
    x, y = np.meshgrid(c, -c)
    return x, y


def fbm(size, seed, octaves=5, base=4, offset=(0.0, 0.0)):
    """Fractal value noise in 0..1, made by upscaling random grids (smooth enough for smoke)."""
    rng = np.random.default_rng(seed)
    total = np.zeros((size, size), np.float32)
    amp, norm = 1.0, 0.0
    for o in range(octaves):
        cells = base * (2 ** o)
        pad = cells + 4
        layer = rng.random((pad, pad)).astype(np.float32)
        img = Image.fromarray((layer * 255).astype(np.uint8)).resize((size * pad // cells,) * 2, Image.BICUBIC)
        arr = np.asarray(img, np.float32) / 255.0
        ox = int(offset[0] * size) % (arr.shape[1] - size)
        oy = int(offset[1] * size) % (arr.shape[0] - size)
        total += amp * arr[oy:oy + size, ox:ox + size]
        norm += amp
        amp *= 0.5
    return total / norm


def save(name, rgb, alpha):
    rgb = np.clip(rgb, 0, 1)
    alpha = np.clip(alpha, 0, 1)
    if rgb.ndim == 2:
        rgb = np.stack([rgb] * 3, -1)
    data = np.dstack([rgb, alpha[..., None]])
    Image.fromarray((data * 255 + 0.5).astype(np.uint8), "RGBA").save(os.path.join(OUT, name + ".png"))


def white(shape):
    return np.ones(shape, np.float32)


def sheet(cols, rows, frame, render):
    out_rgb = np.zeros((rows * frame, cols * frame, 3), np.float32)
    out_a = np.zeros((rows * frame, cols * frame), np.float32)
    n = cols * rows
    for i in range(n):
        rgb, a = render(i, n, frame)
        if rgb.ndim == 2:
            rgb = np.stack([rgb] * 3, -1)
        r, c = divmod(i, cols)
        out_rgb[r * frame:(r + 1) * frame, c * frame:(c + 1) * frame] = rgb
        out_a[r * frame:(r + 1) * frame, c * frame:(c + 1) * frame] = a
    return out_rgb, out_a


def edge_fade(x, y, start=0.85):
    r = np.sqrt(x * x + y * y)
    return np.clip((1.0 - r) / (1.0 - start), 0, 1)


# ---- single sprites ----

def point(size, width):
    x, y = grid(size)
    r2 = x * x + y * y
    return white(x.shape), np.exp(-r2 / (2 * width * width)) * edge_fade(x, y)


def halo(size):
    x, y = grid(size)
    r = np.sqrt(x * x + y * y)
    ring = np.exp(-((r - 0.62) / 0.07) ** 2)
    inner = 0.18 * np.exp(-(r * r) / 0.18)
    return white(x.shape), (ring + inner) * edge_fade(x, y)


def flare(size, angle=0.0, strength=1.0, length=0.9):
    x, y = grid(size)
    r2 = x * x + y * y
    a = np.exp(-r2 / (2 * 0.05 ** 2)) + 0.35 * np.exp(-r2 / (2 * 0.22 ** 2))
    for theta, weight in ((0, 1.0), (90, 1.0), (45, 0.35), (135, 0.35)):
        t = np.radians(theta + angle)
        along = x * np.cos(t) + y * np.sin(t)
        across = -x * np.sin(t) + y * np.cos(t)
        a = a + weight * strength * np.exp(-(across / 0.012) ** 2) * np.exp(-(along / length) ** 2)
    return white(x.shape), a * edge_fade(x, y, 0.9)


def flame(size, seed, tongues):
    x, y = grid(size)
    v = (y + 1.0) / 2.0                                    # 0 at the bottom, 1 at the top
    wobble = (fbm(size, seed, 4, 3) - 0.5) * 0.5 * v
    breakup = fbm(size, seed + 1, 5, 6)
    density = np.zeros_like(x)
    for offset, height in tongues:
        radius = 0.42 * np.clip(1.0 - v / height, 0, 1) ** 0.7 * np.clip((v + 0.12) / 0.25, 0, 1)
        d = np.abs(x + wobble - offset) / np.maximum(radius, 1e-4)
        density = np.maximum(density, np.clip(1.0 - d, 0, 1) * (radius > 0))
    density = density ** 0.8 * (0.55 + 0.45 * breakup)
    heat = np.clip(density * 1.4, 0, 1)
    red, yellow = np.array([1.0, 0.32, 0.06]), np.array([1.0, 0.86, 0.45])
    rgb = red + (yellow - red) * heat[..., None]
    rgb = rgb + (1.0 - rgb) * np.clip(heat - 0.75, 0, 1)[..., None] * 3.0
    return rgb, np.clip(density * 1.25, 0, 1) * edge_fade(x, y, 0.92)


# ---- sprite sheets ----

def billow(x, y, radius, noise):
    """A cloud: a round falloff whose edge is cut by the noise, so it reads as puffs rather than a blurred dot."""
    r = np.sqrt(x * x + y * y) / radius
    body = np.clip(1.0 - r, 0, 1) ** 0.6
    return np.clip((body * 1.35 + noise * 0.9 - 0.75) * 1.8, 0, 1) * np.clip(1.0 - r, 0, 1) ** 0.25


def smoke_frame(i, n, size, seed=11):
    p = i / (n - 1)
    x, y = grid(size)
    noise = fbm(size, seed, 5, 3, offset=(0.1 + p * 0.35, 0.2 + p * 0.5))
    density = billow(x, y, 0.62 + 0.34 * p, noise)
    fade = (1.0 - p) ** 1.2 * np.clip(p / 0.06, 0.4, 1)
    shade = 0.95 - 0.3 * (1.0 - noise) * density
    return shade, np.clip(density * fade, 0, 1) * edge_fade(x, y, 0.92)


def puff_frame(i, n, size):
    x, y = grid(size)
    rng = np.random.default_rng(100 + i)
    sx, sy = rng.uniform(0.8, 1.0, 2)
    noise = fbm(size, 200 + i, 4, 3)
    density = billow(x / sx, y / sy, 0.85, noise)
    return 0.95 - 0.3 * (1.0 - noise) * density, density * edge_fade(x, y, 0.92)


def wisp_frame(i, n, size):
    x, y = grid(size)
    rng = np.random.default_rng(300 + i)
    freq, phase, amp = rng.uniform(1.5, 3.0), rng.uniform(0, 6.3), rng.uniform(0.15, 0.3)
    curve = amp * np.sin(freq * y * np.pi + phase) + 0.1 * (fbm(size, 400 + i, 3, 2) - 0.5)
    d = np.abs(x - curve)
    along = np.clip(1.0 - np.abs(y) / 0.9, 0, 1)
    return white(x.shape), np.exp(-(d / 0.06) ** 2) * along ** 0.7 * edge_fade(x, y, 0.9)


def explosion_frame(i, n, size):
    p = i / (n - 1)
    x, y = grid(size)
    noise = fbm(size, 21, 5, 3, offset=(p * 0.3, 0.1 + p * 0.4))
    density = billow(x, y, 0.4 + 0.55 * np.sqrt(p), noise)
    heat = np.clip(density * (1.0 - p) ** 1.2 * (0.6 + 0.8 * noise) * 1.6, 0, 1)
    ramp = np.array([[0.18, 0.16, 0.15], [0.8, 0.12, 0.03], [1.0, 0.5, 0.08], [1.0, 0.85, 0.4], [1.0, 1.0, 0.9]])
    pos = heat * (len(ramp) - 1)
    lo = np.floor(pos).astype(int).clip(0, len(ramp) - 2)
    f = (pos - lo)[..., None]
    rgb = ramp[lo] * (1 - f) + ramp[lo + 1] * f
    alpha = density * (1.0 - p ** 2) * 1.4
    return rgb, np.clip(alpha, 0, 1) * edge_fade(x, y, 0.9)


def flare_frame(i, n, size):
    p = i / (n - 1)
    rgb, a = flare(size, angle=p * 45.0, strength=0.6 + 0.4 * np.sin(p * np.pi), length=0.55 + 0.35 * np.sin(p * np.pi))
    return rgb, a * (0.5 + 0.5 * np.sin(p * np.pi) + 0.2)


# ---- assets ----

TEXTURE_META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 1
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 1
  lightmap: 0
  compressionQuality: 50
  spriteMode: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 0
  textureShape: 1
  userData:
  assetBundleName:
  assetBundleVariant:
"""

MATERIAL = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {name}
  m_Shader: {shader}
  m_Parent: {{fileID: 0}}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 0
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {{}}
  disabledShaderPasses: []
  m_LockedProperties:
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
    - _MainTex:
        m_Texture: {{fileID: 2800000, guid: {texture}, type: 3}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    m_Ints: []
    m_Floats:
    - _InvFade: {inv_fade}
    m_Colors:
    - _Color: {{r: 1, g: 1, b: 1, a: 1}}
    - _TintColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_BuildTextureStacks: []
  m_AllowLocking: 1
"""

MATERIAL_META = """fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 2100000
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def guid_of(path):
    meta = path + ".meta"
    if os.path.exists(meta):
        m = re.search(r"^guid: ([0-9a-f]{32})", open(meta, encoding="utf-8").read(), re.M)
        if m:
            return m.group(1)
    return uuid.uuid4().hex


def write_texture(name, rgb, alpha):
    path = os.path.join(OUT, name + ".png")
    guid = guid_of(path)
    save(name, rgb, alpha)
    if not os.path.exists(path + ".meta"):
        open(path + ".meta", "w", encoding="utf-8", newline="\n").write(TEXTURE_META.format(guid=guid))
    return guid


def write_material(name, texture_guid, inv_fade=1.0, builtin_shader=None):
    path = os.path.join(OUT, name + ".mat")
    guid = guid_of(path)
    shader = (f"{{fileID: {builtin_shader}, guid: 0000000000000000f000000000000000, type: 0}}" if builtin_shader
              else f"{{fileID: 4800000, guid: {MOCK_BLENDED}, type: 3}}")
    open(path, "w", encoding="utf-8", newline="\n").write(
        MATERIAL.format(name=name, shader=shader, texture=texture_guid, inv_fade=inv_fade))
    if not os.path.exists(path + ".meta"):
        open(path + ".meta", "w", encoding="utf-8", newline="\n").write(MATERIAL_META.format(guid=guid))
    return guid


def main():
    os.makedirs(OUT, exist_ok=True)
    tex = {
        "vf_vfx_point": write_texture("vf_vfx_point", *point(256, 0.32)),
        "vf_vfx_glow_large": write_texture("vf_vfx_glow_large", *point(512, 0.42)),
        "vf_vfx_halo": write_texture("vf_vfx_halo", *halo(512)),
        "vf_vfx_flare": write_texture("vf_vfx_flare", *flare(1024)),
        "vf_vfx_flare_small": write_texture("vf_vfx_flare_small", *flare(256)),
        "vf_vfx_fire_a": write_texture("vf_vfx_fire_a", *flame(512, 7, [(0.0, 1.0)])),
        "vf_vfx_fire_b": write_texture("vf_vfx_fire_b", *flame(512, 8, [(-0.14, 0.75), (0.12, 0.95)])),
        "vf_vfx_smoke_sheet": write_texture("vf_vfx_smoke_sheet", *sheet(8, 8, 256, smoke_frame)),
        "vf_vfx_smoke_puffs": write_texture("vf_vfx_smoke_puffs", *sheet(4, 4, 128, puff_frame)),
        "vf_vfx_wisps": write_texture("vf_vfx_wisps", *sheet(2, 2, 128, wisp_frame)),
        "vf_vfx_explosion_sheet": write_texture("vf_vfx_explosion_sheet", *sheet(8, 8, 256, explosion_frame)),
        "vf_vfx_flare_sheet": write_texture("vf_vfx_flare_sheet", *sheet(8, 8, 128, flare_frame)),
    }
    materials = {
        "vf_vfx_point": write_material("vf_vfx_point", tex["vf_vfx_point"]),
        "vf_vfx_glow_large": write_material("vf_vfx_glow_large", tex["vf_vfx_glow_large"]),
        "vf_vfx_halo": write_material("vf_vfx_halo", tex["vf_vfx_halo"]),
        "vf_vfx_flare": write_material("vf_vfx_flare", tex["vf_vfx_flare"]),
        "vf_vfx_flare_small": write_material("vf_vfx_flare_small", tex["vf_vfx_flare_small"]),
        "vf_vfx_fire_a": write_material("vf_vfx_fire_a", tex["vf_vfx_fire_a"]),
        "vf_vfx_fire_b": write_material("vf_vfx_fire_b", tex["vf_vfx_fire_b"]),
        "vf_vfx_smoke_sheet": write_material("vf_vfx_smoke_sheet", tex["vf_vfx_smoke_sheet"], 0.5),
        "vf_vfx_smoke_sheet_add": write_material("vf_vfx_smoke_sheet_add", tex["vf_vfx_smoke_sheet"], 0.5, BUILTIN_ADDITIVE),
        "vf_vfx_smoke_puffs": write_material("vf_vfx_smoke_puffs", tex["vf_vfx_smoke_puffs"], 0.5),
        "vf_vfx_wisps": write_material("vf_vfx_wisps", tex["vf_vfx_wisps"]),
        "vf_vfx_explosion_sheet": write_material("vf_vfx_explosion_sheet", tex["vf_vfx_explosion_sheet"]),
        "vf_vfx_flare_sheet": write_material("vf_vfx_flare_sheet", tex["vf_vfx_flare_sheet"]),
    }
    for name, guid in materials.items():
        print(f"{name} {guid}")


if __name__ == "__main__":
    main()
