# Unity Setup

Project Settings → Version Control = Visible Meta Files.

Layers:
- Lead (terminals/leads)
- Interactable (optional)

Materials:
- M_Wire_Unlit (Shader: Unlit/Color)
- Glass lens materials (Standard → Transparent), LED emission colors.

Scene:
- Main Camera (Tag: MainCamera)
- _Managers/WireConnectorManager (assign wire prefab + materials)
- Place PowerSource and LED prefabs for the sandbox.
