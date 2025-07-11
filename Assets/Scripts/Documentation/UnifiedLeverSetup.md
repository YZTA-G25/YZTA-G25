# Two-Lever Head Control System - Setup Guide

## Overview
This system provides independent control over head rotation using two separate levers:
- **Horizontal Lever**: Controls left/right head movement
- **Vertical Lever**: Controls up/down head movement

The system uses a single, flexible `LeverController` script that can be configured for different control types through a simple dropdown menu.

## Why This Approach?
✅ **One Script for Everything**: Single `LeverController.cs` handles all lever types  
✅ **Super Easy Setup**: Just change a dropdown to switch between horizontal/vertical/roll  
✅ **Copy & Paste Friendly**: Perfect for rapid prototyping and level design  
✅ **Auto-Configuration**: Built-in optimal settings for each lever type  
✅ **Network Ready**: Full multiplayer support out of the box  
✅ **Future-Proof**: Easy to extend with new control types  

## Quick Setup (5 Minutes)

### Step 1: Create Your Levers
1. **Create GameObject**: Right-click in Hierarchy → Create Empty
2. **Name It**: Give descriptive names like "HorizontalLever" or "VerticalLever"
3. **Add Collider**: Add Box Collider or Mesh Collider for interaction
4. **Add Scripts**: 
   - Add `LeverController` component
   - Add `NetworkObject` component (for multiplayer)

### Step 2: Configure Lever Types

#### For Horizontal (Left/Right) Control:
```
LeverController Settings:
├── Control Type: Horizontal
├── Click "Apply Recommended Settings" button
└── Result: Head turns left/right when you move mouse left/right
```

#### For Vertical (Up/Down) Control:
```
LeverController Settings:
├── Control Type: Vertical  
├── Click "Apply Recommended Settings" button
└── Result: Head looks up/down when you move mouse left/right
```

### Step 3: Test and Adjust
- **Start the game** and test each lever
- **Adjust Input Sensitivity** if too fast/slow (typical range: 0.5 to 2.0)
- **Use negative sensitivity** to reverse direction if needed

## Detailed Configuration

### Lever Control Types

| Control Type | What It Does | Head Axis | When to Use |
|--------------|--------------|-----------|-------------|
| **Horizontal** | Left/Right head turning | Y-axis | Main head rotation control |
| **Vertical** | Up/Down head looking | X-axis | Looking up at ceiling/down at floor |
| **Roll** | Head tilting | Z-axis | Special effects, confusion simulation |
| **Custom** | User-defined axis | Any axis | Special use cases |

### Settings Explained

#### Essential Settings:
- **Control Type**: Choose what this lever controls (Horizontal/Vertical/Roll/Custom)
- **Min/Max Angle**: How far the lever (and head) can rotate
- **Input Sensitivity**: How responsive the lever is to mouse movement
- **Lever Rotation Axis**: Which axis the lever itself rotates around visually

#### Recommended Settings (Auto-Applied):

**Horizontal Lever:**
- Min Angle: -45°, Max Angle: 45°
- Lever Rotation Axis: (0, 0, 1) - rotates around Z-axis
- Good for: Main head turning left/right

**Vertical Lever:**
- Min Angle: -30°, Max Angle: 30°  
- Lever Rotation Axis: (1, 0, 0) - rotates around X-axis
- Good for: Looking up/down

**Roll Lever:**
- Min Angle: -30°, Max Angle: 30°
- Lever Rotation Axis: (0, 1, 0) - rotates around Y-axis
- Good for: Head tilt effects

## Example Scene Setup

### Basic Two-Lever Setup:
```
Scene Hierarchy:
├── Player (with EyePlayerController)
├── HeadController (attached to head object)
├── Levers/
│   ├── HorizontalLever
│   │   ├── LeverController (Type: Horizontal)
│   │   ├── NetworkObject
│   │   └── BoxCollider
│   └── VerticalLever
│       ├── LeverController (Type: Vertical)
│       ├── NetworkObject
│       └── BoxCollider
```

### Advanced Multi-Control Setup:
```
Scene Hierarchy:
├── ControlStation/
│   ├── MainHorizontalLever (Type: Horizontal)
│   ├── MainVerticalLever (Type: Vertical)
│   ├── RollLever (Type: Roll)
│   └── CustomLever (Type: Custom)
```

## Workflow for Multiple Levers

### The Copy & Paste Method:
1. **Create Master Lever**: Set up one lever perfectly
2. **Duplicate**: Select lever → Ctrl+D (duplicate)
3. **Rename**: Give it a clear name
4. **Change Type**: Switch Control Type dropdown
5. **Auto-Configure**: Click "Apply Recommended Settings"
6. **Position**: Move to desired location
7. **Repeat**: For as many levers as needed

### Pro Tips:
- **Use Parent Objects**: Group related levers under empty GameObjects
- **Consistent Naming**: Use clear names like "Station1_HorizontalLever"
- **Test Frequently**: Check each lever works before adding more
- **Document Setup**: Add comments in inspector for complex setups

## Multiplayer Considerations

### Network Setup Checklist:
- ✅ Each lever has `NetworkObject` component
- ✅ Levers are added to NetworkManager's spawnable prefabs list
- ✅ HeadController is also a NetworkBehaviour with NetworkObject
- ✅ Only the owner of EyePlayer can control levers (handled automatically)

### Spawning Levers:
- **Static Levers**: Place in scene, they'll be networked automatically
- **Dynamic Levers**: Spawn via NetworkManager.Singleton.SpawnManager.SpawnAsPlayerObject()

## Troubleshooting

### Common Issues & Solutions:

**🔧 Lever Not Responding:**
- Check: Collider is present and not set to trigger
- Check: NetworkObject component is attached
- Check: EyeInteractor raycast range covers the lever

**🔧 Wrong Direction:**
- Solution: Try negative Input Sensitivity value
- Example: If sensitivity is 1.0, try -1.0

**🔧 Too Fast/Slow:**
- Solution: Adjust Input Sensitivity
- Fast: Try 0.5 or 0.25
- Slow: Try 2.0 or 3.0

**🔧 Lever Visual Wrong:**
- Solution: Click "Apply Recommended Settings"
- Or manually set Lever Rotation Axis

**🔧 Head Not Moving:**
- Check: HeadController is in scene and has NetworkObject
- Check: HeadController's headToRotate is assigned

**🔧 Multiplayer Issues:**
- Check: All components have NetworkObject
- Check: Components are in NetworkManager spawn list
- Check: Only one player controlling at a time

## Advanced Usage

### Custom Control Types:
```csharp
// In LeverController, you can extend the enum:
public enum LeverControlType
{
    Horizontal,
    Vertical, 
    Roll,
    Custom,
    // Add your own:
    Tilt,        // Custom tilt control
    Spin,        // Continuous rotation
    Combined     // Multi-axis control
}
```

### Input Sensitivity Tricks:
- **Positive values**: Normal direction
- **Negative values**: Inverted direction  
- **Large values (5.0+)**: Very sensitive, small movements
- **Small values (0.1)**: Very smooth, large movements needed

### Visual Customization:
- Lever rotates around `Lever Rotation Axis`
- Completely separate from what axis the head moves
- Can create unique visual feedback per lever type

## Summary

This unified system gives you:
1. **Easy Setup**: One dropdown choice determines everything
2. **Flexible Control**: Horizontal, vertical, roll, or custom axes
3. **Network Ready**: Full multiplayer support
4. **Scalable**: Add dozens of levers easily
5. **Maintainable**: Single script to update and debug

Perfect for creating control panels, machinery interfaces, or any scenario where you need precise, independent control over different aspects of head rotation!
