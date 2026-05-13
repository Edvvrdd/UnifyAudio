# UnifyAudio

A lightweight Unity toolkit for inspector-friendly FMOD event instance control. UnifyAudio bridges the gap between FMOD Studio and Unity by providing a ScriptableObject-driven architecture for managing audio instances, parameters, and signals — without writing FMOD API code.

> **Requires** FMOD Studio Unity Integration installed in your project.

---

## Features

- **UnifyInstanceManager** — managed FMOD event instance with full lifecycle control (play/pause/unpause/stop), auto-detected local parameter bindings with range/type metadata, and always-on stop-and-release on destroy
- **UnifySignal** — ScriptableObject broadcast channel for decoupled, cross-scene event triggering
- **UnifyParameter** — reusable local parameter handle any script can drive by reference; auto-syncs value to newly created FMOD instances
- **UnifyGlobalParameter** — inspector-friendly global parameter control with FMOD dropdown, cached metadata, and OnValueChanged event for reactive UI
- **UnifyEventCollection** — optional organizational layer for grouping FMOD events into typed SO assets, with companion FMOD Studio export script

---

## Installation

### Via Git URL (recommended)

In Unity open **Window → Package Manager**, click **+** and choose **Add package from Git URL**:

```
https://github.com/yourusername/UnifyAudio.git
```

### Manual

Download or clone this repository and place the `UnifyAudio` folder inside your Unity project's `Packages` folder.

---

## Requirements

- Unity 2021.3 or later
- FMOD Studio Unity Integration 2.02 or later
- FMOD Studio 2.03 or later (for the optional FMOD-side export script)

---

## Quick Start

### 1. Control an FMOD event instance

Add `UnifyInstanceManager` to any GameObject. Select your FMOD event from the dropdown. Configure lifecycle options:

- **Play on Start** — plays when the scene loads
- **Play on Enable / Pause on Disable** — ties audio state to object activation
- **Resume if paused** — play signals unpause instead of restarting

### 2. Drive parameters without code

If your FMOD event has local parameters, they appear automatically in the Parameters section of `UnifyInstanceManager`. Create a `UnifyParameter` asset via **Create → UnifyAudio → Parameter Value** and slot it into the corresponding parameter row. Any script holding a reference to that asset can call:

```csharp
[SerializeField] private UnifyParameter _intensity;

_intensity.SetValue(0.8f);
```

All controllers that have slotted that asset will respond simultaneously.

### 3. Trigger instances with signals

Create a `UnifySignal` asset via **Create → UnifyAudio → Signal**. Slot it into the Play, Pause, or Stop signal slots on `UnifyInstanceManager`. Any script can then fire it:

```csharp
[SerializeField] private UnifySignal _onCinematicStart;

_onCinematicStart.Fire();
```

One signal can drive multiple controllers across different scenes simultaneously.

### 4. Control global parameters

Create a `UnifyGlobalParameter` asset via **Create → UnifyAudio → Global Parameter**. Select the parameter from the FMOD dropdown. Call from any script:

```csharp
[SerializeField] private UnifyGlobalParameter _masterReverb;

_masterReverb.SetValue(0.5f);
```

---

## Event Collections (Optional)

Event Collections are an optional organizational tool for larger projects. They let you group related FMOD events into a single typed SO asset, eliminating scattered `EventReference` fields and raw string references across scripts.

### Unity side

**Quick start (no FMOD Studio needed):** Create a collection from **Create → UnifyAudio → Event Collection → 5 / 10 / 20 Events**. Rename the fields in the inspector and drop in FMOD events. Use the generated class as a template if you need a different count.

**Custom collections:** Extend `UnifyEventCollection` with your own fields:

```csharp
[CreateAssetMenu(menuName = "UnifyAudio/Data Sheets/Player")]
public class PlayerAudioSheet : UnifyEventCollection
{
    [Header("Movement")]
    public EventReference Jump;
    public EventReference Land;

    [Header("Combat")]
    public EventReference SwordAttack;
    public EventReference Death;
}
```

Create the asset, fill in event references, then slot the single asset into any script that needs it:

```csharp
[SerializeField] private PlayerAudioSheet _audio;

_audio.PlayOneShot(_audio.Jump, transform.position);
_audio.PlayOneShotAttached(_audio.SwordAttack, gameObject);
```

### Auto-generate sheets from FMOD Studio

For teams where a sound designer manages events in FMOD Studio, use the included FMOD-side JavaScript to generate typed data sheet classes automatically.

**Setup:**

1. Copy `UnifyEventCollectionExport.js` from the `FmodScripts` folder in this repo
2. Place it in the `Scripts` folder at the root of your FMOD Studio project
3. Restart FMOD Studio — the script appears under **Scripts → UnifyAudio → Export Audio Data Sheets**

**Tagging events in FMOD Studio:**

On each event you want to export, add:

| User Property | Value | Required |
|---|---|---|
| `UnifySheet` | Class name e.g. `PlayerAudio` | Yes |
| `UnifyGroup` | Header group e.g. `Movement` | No |
| `UnifyEntry` | Field name e.g. `PlayerJump` | No — defaults to event name |

Then add the tag `UnifyAudio` to the event.

You can multi-select events in the FMOD browser and set `UnifySheet` and `UnifyGroup` in bulk. The entry name defaults to the event name automatically so you only need to set it manually if you want a different field name.

**Exporting:**

Run **Scripts → UnifyAudio → Export Audio Data Sheets**. When prompted, paste the absolute path to your Unity project's Assets folder. One `.cs` file is generated per unique `UnifySheet` value. Drop the files into Unity and create SO assets from the new classes.

---

## Design Philosophy

UnifyAudio is a thin ScriptableObject layer that sits beside FMOD — not on top of it. It complements the existing FMOD components, not replaces them. Parameters and signals live as project assets that survive scene loads, so audio wiring is visible, shareable, and decoupled from scene hierarchies. Event collections keep your FMOD references organized without chasing down GUIDs. The goal is simple: stop reopening FMOD Studio just to check a parameter range, and stop writing the same boilerplate every time you need a sound to react to gameplay.

---

## License

MIT — see [LICENSE](LICENSE) for details.

---

## Acknowledgements

- [Alessandro Famà](https://alessandrofama.com) — FMOD ScriptableObject pattern and FMOD Studio scripting approach
- [Ryan Hipple](http://ryanjboyer.com) — ScriptableObject signal architecture
- [FMOD](https://www.fmod.com) — audio middleware

---

## Version History

### 1.0.2 — Maintenance Update
- **UnifyInstanceManager** — Added `Unpause()` method and "Resume if paused" toggle in Play Signals. Removed stop-and-release toggle (now always on to prevent instance leaks). Inspector reorganized — auto-triggers consolidated into their respective signal sections. Local parameter bindings now show foldout metadata (type, range, default, labels) sourced from the FMOD event.
- **UnifyGlobalParameter** — Now exposes `Value` property and `OnValueChanged` event for reactive UI binding.
- **UnifyParameter** — New subscribers now receive the parameter's current value immediately upon binding.
- **UnifyEventCollection** — Added `UnifyEventCollection5/10/20` — quick manual collections via Create menu, no FMOD Studio export needed.
- Removed misleading thread-safety warning; documented actual thread-safety model.

### 1.0.0 — Initial Release
- Core SO-driven architecture for FMOD event instance management, parameters, signals, and event collections.
