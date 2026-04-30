# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 4

### Phase 4.3 - InputManager

please implement
- [x] AI: implement [systems/input-manager.md](systems/input-manager.md)

**Files created:**
- `Assets/Scripts/Systems/Input/MovementInput.cs` - readonly struct (Throttle, Steering)
- `Assets/Scripts/Systems/Input/IInputManager.cs` - interface
- `Assets/Scripts/Systems/Input/InputManager.cs` - MonoBehaviour implementation
- `Assets/Tests/EditMode/InputManagerTests.cs` - 8 EditMode tests for ApplyDeadzone

**Files modified:**
- `Assets/Settings/InputActions.inputactions` - removed StickDeadzone processor (normalization now done exclusively in InputManager.cs to avoid double-processing)
- `docs/tracking.md` - marked step complete

**Wiring note (HUMAN task in Phase 5):**
- Assign `Assets/Settings/InputActions.inputactions` to `InputManager._inputActions` in the Inspector when wiring the Persistent scene.
