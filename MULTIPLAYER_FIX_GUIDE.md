# Multiplayer Camera and Input System Fix Guide - Unity 6

This guide addresses the two critical multiplayer issues in Unity 6:
1. **Camera Rendering Issue**: Both screens showing the same camera view
2. **Input System Issue**: Both players responding to WASD keys

### Netcode for GameObjects
- Enhanced network ownership validation
- Improved performance and memory management
- Better debugging capabilities with additional network state information

### Input System
- Enhanced input state reporting
- Improved keyboard state detection
- Better action map management

## Root Causes Identified

### Camera Issue
- Both Eye Player and Hand Player cameras were using conflicting output channels
- Camera priority and isolation not properly managed based on network ownership

### Input System Issue
- Both prefabs had references to multiple action maps in their input events
- Lack of proper input isolation based on network ownership
- Missing ownership validation in Update() methods

## Solution Implementation

### Step 1: Add PlayerSetupManager to Prefabs

1. **Eye Player Prefab**:
   - Add `PlayerSetupManager` component
   - Set `Is Eye Player` = true
   - Set `Camera Output Channel` = 1

2. **Hand Player Prefab**:
   - Add `PlayerSetupManager` component  
   - Set `Is Eye Player` = false
   - Set `Camera Output Channel` = 2

### Step 2: Fix EyePlayerController Ownership

The `EyePlayerController.Update()` method has been updated to include proper ownership checks:

```csharp
private void Update()
{
    // Only process input for the owner of this network object
    if (!IsOwner) return;
    if (!Application.isFocused) return;

    HandleMovement();
    HandleLook();
}
```

### Step 3: Verify HandController (Already Fixed)

The `HandController` already has proper ownership validation:
- `OnNetworkSpawn()` properly disables components for non-owners
- `Update()` method includes `if (!IsOwner) return;`

### Step 4: Debug HandPlayer Movement Issue

If HandPlayer still doesn't move after implementing the fixes:

1. **Add Diagnostic Component**:
   - Temporarily add `HandPlayerMovementDiagnostic` to Hand Player prefab
   - Enable verbose logging
   - Use context menu options to check status

2. **Common Issues to Check**:
   - CharacterController component enabled and configured
   - Move speed > 0 in HandController settings
   - Input action map correctly assigned
   - Network ownership properly established

### Step 5: Testing the Fixes

1. **Start Host**:
   - Should spawn as Eye Player
   - Camera should use output channel 1
   - Only WASD input should affect Eye Player

2. **Connect Client**:
   - Should spawn as Hand Player  
   - Camera should use output channel 2
   - Only WASD input should affect Hand Player

3. **Verify Split-Screen**:
   - Two different camera views should be visible
   - Each player should only control their own character

## Expected Behavior After Fix

### Camera System
- Eye Player camera renders to output channel 1 (Unity 6: `OutputChannels.Channel1`)
- Hand Player camera renders to output channel 2 (Unity 6: `OutputChannels.Channel2`)
- Each player only sees their own camera view
- No more "both screens choose the same camera" issue

### Input System
- Eye Player uses "EyePlayer" action map exclusively
- Hand Player uses "HandPlayer" action map exclusively
- WASD input only affects the local player
- No cross-player input interference
- Unity 6: Enhanced input state monitoring and debugging

### Network Ownership
- Each player only processes input for their owned objects
- Non-owners have input components disabled
- Proper component isolation based on player type
- Unity 6: Additional network state validation and reporting

## Troubleshooting

### If Camera Issue Persists
1. Check Cinemachine Camera priorities (Unity 6: use CinemachineCamera instead of CinemachineVirtualCamera)
2. Verify output channel assignments in inspector (should show OutputChannels enum values)
3. Ensure only owner's cameras are enabled
4. Unity 6: Check for conflicts with new camera stack system

### If Input Issue Persists  
1. Use diagnostic script to check input system status
2. Verify action map assignments
3. Check for conflicting PlayerInput components
4. Unity 6: Monitor enhanced input state information in diagnostics

### If HandPlayer Still Won't Move
1. Enable diagnostic logging
2. Check CharacterController settings
3. Verify move speed is not zero
4. Test input system directly with diagnostic tools
5. Unity 6: Use enhanced network state debugging information

### Unity 6 Specific Issues
1. **Cinemachine Migration**: If you see deprecated warnings, update CinemachineVirtualCamera references to CinemachineCamera
2. **OutputChannel Type**: Ensure you're casting int values to OutputChannels enum: `(Unity.Cinemachine.OutputChannels)channelNumber`
3. **Namespace Updates**: Use `using Unity.Cinemachine;` instead of `using Cinemachine;`
4. **Performance**: Unity 6 has improved performance monitoring - check the Profiler for any network or input bottlenecks

## File Locations

- `PlayerSetupManager.cs`: `/Assets/Scripts/Manager/`
- `HandPlayerMovementDiagnostic.cs`: `/Assets/Scripts/Debug/`
- `CameraAndInputFix.cs`: `/Assets/Scripts/Manager/` (alternative approach)

## Notes

- The `PlayerSetupManager` provides the most comprehensive solution
- The diagnostic script should be removed after fixing issues
- Both solutions are compatible and can be used together if needed
- All scripts include detailed debug logging for troubleshooting
