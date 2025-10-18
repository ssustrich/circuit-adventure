# Circuit Adventure (@codex)

Interactive wiring sandbox in Unity (Built-in RP). Place a PowerSource, wire to an LED, and watch it light.

## Quick Start
1) Unity 6.x (Built-in 3D).  
2) Clone the repo and open in Unity.  
3) Open `Assets/Scenes/Sandbox.unity` (or your main scene).  
4) Play:
   - `1/2` = select prefab
   - `R` = rotate
   - LMB = place
   - Aim + LMB on terminals to connect wires
   - Click the Power LED button to toggle power

## Core Systems
- **PlacementController**: ghost preview, grid snap.
- **LeadConnector + WireConnectorManager**: click-to-wire with LineRenderer.
- **PowerSourceController**: single source of truth; drives the power LED.
- **LEDDevice + LEDLight**: evaluates wiring (pos/neg to same powered source) and toggles emission.

## Docs
See `/docs/codex/*`.

## License
Apache-2.0
