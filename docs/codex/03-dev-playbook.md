# Dev Playbook

- Prefer prefab variants for FBX-based models.
- Tests should call PowerSourceController.TogglePower() or LEDDevice.Recompute() — never LEDLight.Toggle().
- Useful debug:
  - DebugHotkeys (P = toggle all power, L = recompute all LEDs)
  - DebugOverlayHUD
  - LeadConnector gizmos / Handles
