# Sunleaf Level One parallax pilot assets

Date: 18 September 2026  
Target: isolated Gate 4 pilot only

These are original Sunleaf pilot assets generated from the palette and general mood of `Backgrounds/Trailhead.png`. The linked Asset Store package and the other five Sunleaf backdrops were not used as image inputs.

All four images are 1774 by 887 pixels. Unity imports them at 73.916664 pixels per unit, producing an exact 24 by 12 world-unit rectangle. Mipmaps and fallback physics shapes are disabled. The sprite mesh is Full Rect so every pooled segment has identical measured bounds.

| File | Alpha | Screen drift | Opacity | Order | Pilot status | SHA-256 |
| --- | --- | ---: | ---: | ---: | --- | --- |
| `Sunleaf_L01_L00_Sky_Pilot.png` | Opaque RGB | 0.00 | 1.00 | -260 | Accepted for pilot | `262756d9cdd5079530663e9ccd91ed2ff8b8b764c7014e6056eea0a26b6da00f` |
| `Sunleaf_L01_L10_FarRuins_Pilot.png` | Native RGBA | 0.08 | 0.58 | -240 | Provisional; dominant ruin requires live art review | `79de4276970180ed19fbb62f44184edb4ba306fddfe6f9a0222fce96f71b4953` |
| `Sunleaf_L01_L20_DistantForest_Pilot.png` | Native RGBA | 0.18 | 0.82 | -220 | Accepted for pilot | `6d78b2fb139241fbda4cf591d9e5275be1b4b213d84f1e997b606f5ee0129ef3` |
| `Sunleaf_L01_L30_NearFoliage_Pilot.png` | Native RGBA | 0.42 | 0.72 | -200 | Accepted for pilot | `e1988e7945deb8ba3b432b3de373da9d3e5ac0d911a9b66320132715d1693569` |

The runtime must alternate normal and horizontally mirrored segments. This makes both types of shared boundary pixel-identical and is the pilot's explicit symmetry contract. These images have no colliders and do not replace any gameplay sprite.

`Sunleaf_L01_BoulderDecor_Pilot.png` is a 1536 by 1024 native-RGBA decorative cutout imported at 512 PPU (3 by 2 world units). Its SHA-256 is `c5b074c2fce9c7165bbbe7560286b1e5e89ddaee7820ac1a12827da06ed1e19b`. The pilot builder places it at sorting order 3, behind Logan and the terrain lip, with no collider or rigidbody.
