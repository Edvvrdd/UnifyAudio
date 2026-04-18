# UnifyAudio

A lightweight Unity toolkit for inspector-friendly FMOD event instance control. UnifyAudio bridges the gap between FMOD Studio and Unity by providing a ScriptableObject-driven architecture for managing audio instances, parameters, and signals — without writing FMOD API code.

> **Requires** FMOD Studio Unity Integration installed in your project.

---

## Features

- **FMODEventController** — a managed FMOD event instance with full lifecycle control, auto-detected local parameters, and guaranteed cleanup on destroy
- **FMODSignal** — a ScriptableObject broadcast channel for decoupled, cross-scene event triggering
- **FMODLocalParameter** — a reusable parameter handle any script can drive by reference
- **FMODGlobalParameter** — inspector-friendly global parameter control with FMOD dropdown integration
- **UnifyAudioDataSheet** — optional organizational layer for grouping FMOD events into typed SO assets, with a companion FMOD Studio export script

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

Add `FMODEventController` to any GameObject. Select your FMOD event from the dropdown. Configure lifecycle options:

- **Play on Start** — plays when the scene loads
- **Play on Enable / Pause on Disable** — ties audio state to object activation
- **Stop and Release on Destroy** — always on by default, prevents instance leaks

### 2. Drive parameters without code

If your FMOD event has local parameters, they appear automatically in the Parameters section of `FMODEventController`. Create a `FMODLocalParameter` asset via **Create → UnifyAudio → Parameter Value** and slot it into the corresponding parameter row. Any script holding a reference to that asset can call:

```csharp
[SerializeField] private FMODLocalParameter _intensity;

_intensity.SetValue(0.8f);
```

All controllers that have slotted that asset will respond simultaneously.

### 3. Trigger instances with signals

Create a `FMODSignal` asset via **Create → UnifyAudio → Signal**. Slot it into the Play, Pause, or Stop signal slots on `FMODEventController`. Any script can then fire it:

```csharp
[SerializeField] private FMODSignal _onCinematicStart;

_onCinematicStart.Fire();
```

One signal can drive multiple controllers across different scenes simultaneously.

### 4. Control global parameters

Create a `FMODGlobalParameter` asset via **Create → UnifyAudio → Global Parameter**. Select the parameter from the FMOD dropdown. Call from any script:

```csharp
[SerializeField] private FMODGlobalParameter _masterReverb;

_masterReverb.SetValue(0.5f);
```

---

## Event Collections (Optional)

Event Collections are an optional organizational tool for larger projects. They let you group related FMOD events into a single typed SO asset, eliminating scattered `EventReference` fields and raw string references across scripts.

### Unity side

Extend `UnifyEventCollection` with your own fields:

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

1. Copy `UnifyAudioExport.js` from the `FMODScript` folder in this repo
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

## Architecture Overview

```
UnifyAudio
├── Runtime
│   ├── FMODEventController.cs       — instance owner and lifecycle manager
│   ├── FMODParameterBinding.cs      — serializable parameter/SO binding
│   ├── FMODSignal.cs                — broadcast signal ScriptableObject
│   └── Parameters
│       ├── UnifyParameterAsset.cs   — abstract base for parameter SOs
│       ├── FMODParameterValue.cs    — local parameter value broadcaster
│       └── FMODGlobalParameter.cs   — global parameter SO
├── Editor
│   ├── FMODEventControllerEditor.cs
│   ├── FMODGlobalParameterEditor.cs
│   └── FMODParameterValueEditor.cs
├── DataSheets
│   └── UnifyAudioDataSheet.cs       — abstract base for data sheet classes
└── FMODScript
    └── UnifyAudioExport.js          — FMOD Studio export script
```

---

## Design Philosophy

**1. Complement, never compete**

UnifyAudio sets a new standard for FMOD and Unity integration without replacing what already works. Instance lifecycle management, runtime parameter driving, cross-scene signal routing are all new superpowers you can add on top of the existing FMOD components. Use both in the same project without conflict.

**2. A shared language between disciplines**

UnifyAudio treats the Unity inspector as a communication surface for both programmers and sound designers. Flexible and visible Scriptable Objects for managing data flow, auto complete fields to help understand which data goes where. Everything is inspector oriented to bring clarity to the sound design process.

**3. Modular by design**

Every feature is independently usable. Audio wiring lives in the project as assets, not buried in scene hierarchies or hardcoded in scripts. Free and open license to allow users to customize each component without worrying about breaking the whole structure.  

**4. Less boilerplate, more craft**

The time spent typing FMOD API calls, chasing null instance bugs, or hunting mistyped parameter strings is time not spent designing audio systems. UnifyAudio handles the repetitive and error-prone parts so the people building the game can focus on what actually makes the audio interesting.

---

## License

MIT — see [LICENSE](LICENSE) for details.

---

## Acknowledgements

- [Alessandro Famà](https://alessandrofama.com) — FMOD ScriptableObject pattern and FMOD Studio scripting approach
- [Ryan Hipple](http://ryanjboyer.com) — ScriptableObject signal architecture
- [FMOD](https://www.fmod.com) — audio middleware
