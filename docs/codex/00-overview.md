# @codex Overview

Goal: modular circuit sandbox with placement, wiring, and simple power logic.

### Components
- PowerSourceController — authoritative power state, drives LED button.
- LEDDevice — checks both leads → same PowerSource and powered → SetOn(true).
- LEDLight — per-instance emission control (lazy material init).
- WireConnectorManager + LeadConnector — user connections + LineRenderer wires.
- PlacementController — ghost, grid, lift offsets.

### Design rules
- One source of truth for power. No tester calls LEDLight.Toggle().
- Save wire endpoints as local offsets → wires stay glued to click points.
