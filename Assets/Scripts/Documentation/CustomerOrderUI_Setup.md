# Customer Order UI Setup Guide

This guide explains how to set up the customer order UI system in the EyePlayer's canvas.

## Overview

The customer order UI displays active customer orders for the EyePlayer, showing:
- Recipe images
- Recipe names  
- Timer bars with color-coded patience levels
- Sliding panel animations

## Components

### 1. CustomerUI.cs
- Main UI controller
- Handles customer order tracking
- Manages panel creation/destruction
- Only active for EyePlayer

### 2. CustomerOrderPanel.cs
- Individual order panel component
- Handles timer display and colors
- Manages sliding animations

### 3. CustomerOrderCanvasSetup.cs
- Automatic canvas setup utility
- Configures canvas and container
- Sets up component references

## Setup Instructions

### Option 1: Automatic Setup (Recommended)

1. Create an empty GameObject in the scene called "CustomerOrderUI"
2. Add the `CustomerOrderCanvasSetup` component to it
3. Assign the `CustomerOrderPanel` prefab to the "Customer Order Panel Prefab" field
4. The script will automatically:
   - Create and configure the canvas
   - Set up the container
   - Add and configure the CustomerUI component

### Option 2: Manual Setup

1. **Create Canvas:**
   ```
   Create -> UI -> Canvas
   Name: "CustomerOrderCanvas"
   Render Mode: Screen Space - Overlay
   Sorting Order: 1
   ```

2. **Add Canvas Scaler:**
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Screen Match Mode: Match Width Or Height

3. **Create Container:**
   ```
   Create child of Canvas: Empty GameObject
   Name: "OrderPanelContainer"
   Add RectTransform:
   - Anchor: Top (0,1) to (1,1)
   - Position: (0, -50)
   - Size: (0, 100)
   ```

4. **Add CustomerUI Component:**
   - Add `CustomerUI` script to the Canvas or a manager object
   - Assign references:
     - Customer Order Panel Prefab: The prefab you created
     - Order Panel Container: The container transform
     - Customer Order Canvas: The canvas component

## Prefab Structure

The CustomerOrderPanel prefab should have:
```
CustomerOrderPanel (RectTransform + Image + CustomerOrderPanel script)
├── RecipeImage (Image)
├── RecipeNameText (TextMeshPro)
├── TimerBar (Image with Type: Filled)
└── TimerText (TextMeshPro)
```

## Integration Notes

- The UI automatically detects if the player is an EyePlayer
- Only EyePlayers will see the customer order UI
- The system integrates with the existing PlayerSetupManager
- Canvas is disabled for HandPlayers to avoid duplication

## Troubleshooting

### UI Not Showing
- Check if the player is properly identified as EyePlayer
- Verify canvas is enabled in the scene
- Ensure CustomerManager instance exists

### Orders Not Updating
- Check CustomerManager.GetWaitingCustomers() returns correct data
- Verify Customer objects have valid CurrentOrder data
- Check if Recipe objects have proper images assigned

### Performance Issues
- Limit maximum panels (default: 7 customers max)
- Use object pooling for panels if needed
- Consider update frequency for timer displays

## Integration with Existing Systems

The customer order UI works with:
- **CustomerManager**: Gets waiting customer data
- **Customer**: Reads order and patience information  
- **Recipe**: Displays recipe images and names
- **PlayerSetupManager**: Detects EyePlayer vs HandPlayer
- **NetworkManager**: Handles client/host detection
