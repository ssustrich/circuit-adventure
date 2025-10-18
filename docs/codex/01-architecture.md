# Architecture

Runtime "graph v0": implicit lookups.
- LEDDevice reads its LeadConnector.connectedTo → finds PowerSourceController up-chain.

Update triggers:
- On power toggle: FindObjectsOfType<LEDDevice>().Recompute()
- On connect: Recompute LEDs at both ends
- On enable: LEDDevice.Recompute()

Future v1: central registry for connections to avoid scene scans.
