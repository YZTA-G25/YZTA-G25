# Customer System Setup Guide

This guide will walk you through setting up the complete customer system in your Unity cooking game.

## Step 1: Create Required ScriptableObjects

### 1.1 Create Customer Order Data
1. Right-click in your Project window
2. Go to `Create > ScriptableObjects > CustomerOrderData`
3. Name it "CustomerOrderData"
4. In the Inspector, configure:
   - **Available Recipes**: Drag all your Recipe ScriptableObjects here
   - **Min Patience Time**: 30 seconds
   - **Max Patience Time**: 90 seconds
   - **Min Spawn Interval**: 5 seconds
   - **Max Spawn Interval**: 15 seconds
   - **High Speed Tip Percent**: 20
   - **Medium Speed Tip Percent**: 5

## Step 2: Create Customer Prefab

### 2.1 Create Customer GameObject
1. Create an empty GameObject in the scene
2. Name it "CustomerPrefab"
3. Add the following components:
   - **Customer** script
   - **NetworkObject** component
   - **Rigidbody** (set IsKinematic = true)
   - **Capsule Collider** (for basic physics)
   - A visual mesh (cube, capsule, or your customer model)

### 2.2 Configure Customer Script
- **Move Speed**: 2
- **Order Position**: Leave empty (will be set by CustomerManager)
- **Exit Position**: Leave empty (will be set by CustomerManager)

### 2.3 Configure NetworkObject
- Make sure the Customer prefab is added to the **Network Prefabs List** in your NetworkManager

### 2.4 Create Prefab
1. Drag the configured CustomerPrefab from Hierarchy to your Prefabs folder
2. Delete the GameObject from the scene

## Step 3: Setup Scene Positions

### 3.1 Create Position Markers
Create empty GameObjects for customer positions:

1. **Customer Spawn Point**
   - Position: Where customers first appear (e.g., entrance door)
   - Name: "CustomerSpawnPoint"

2. **Customer Order Position**
   - Position: Where customers stop to place orders (e.g., counter)
   - Name: "CustomerOrderPosition"

3. **Customer Exit Position**
   - Position: Where customers go when leaving (e.g., exit door)
   - Name: "CustomerExitPosition"

## Step 4: Setup CustomerManager

### 4.1 Add CustomerManager to Scene
1. Create a **new empty GameObject** in your scene (separate from GameManager)
2. Name it "CustomerManager"
3. Add the **CustomerManager** script
4. Add a **NetworkObject** component

### 4.2 Configure CustomerManager
In the Inspector:
- **Customer Prefab**: Drag your Customer prefab here
- **Order Data**: Drag your CustomerOrderData ScriptableObject here
- **Customer Spawn Point**: Drag the CustomerSpawnPoint GameObject
- **Customer Order Position**: Drag the CustomerOrderPosition GameObject
- **Customer Exit Position**: Drag the CustomerExitPosition GameObject
- **Enable Debug Logs**: Check this for testing

### 4.3 Add to Network Prefabs
- Add the CustomerManager GameObject to your NetworkManager's Network Prefabs list

## Step 5: Setup Customer UI (EyePlayer Only)

### 5.1 Create UI Canvas
1. Create a Canvas in your scene (or use existing UI Canvas)
2. Set Canvas to **Screen Space - Overlay**
3. Set Canvas Scaler to **Scale With Screen Size**

### 5.2 Create Customer Order Panel Container
1. Under the Canvas, create an empty GameObject
2. Name it "CustomerOrderContainer"
3. Add **Horizontal Layout Group** component:
   - Spacing: 10
   - Child Alignment: Upper Left
   - Control Child Size: Width = false, Height = false
4. Add **Content Size Fitter**:
   - Horizontal Fit: Preferred Size
   - Vertical Fit: Preferred Size
5. Position in top-left corner of screen

### 5.3 Create Customer Order Panel Prefab
1. Create a UI Panel under CustomerOrderContainer
2. Name it "CustomerOrderPanel"
3. Set size to approximately 180x120
4. Add the following UI elements as children:

   **Recipe Image**:
   - Add Image component
   - Name: "RecipeImage"
   - Set size to 80x80, position at top

   **Recipe Name Text**:
   - Add TextMeshPro - Text (UI)
   - Name: "RecipeNameText"
   - Position below image

   **Timer Background**:
   - Add Image component
   - Name: "TimerBackground"
   - Set as horizontal bar at bottom
   - Color: Dark gray

   **Timer Bar**:
   - Add Image as child of TimerBackground
   - Name: "TimerBar"
   - Set Image Type to "Filled"
   - Fill Method: Horizontal
   - Color: Green

   **Timer Text**:
   - Add TextMeshPro - Text (UI)
   - Name: "TimerText"
   - Position over timer bar
   - Center alignment

### 5.4 Add CustomerOrderPanel Script
1. Add the **CustomerOrderPanel** script to the CustomerOrderPanel
2. Configure the script references:
   - **Recipe Image**: Drag the RecipeImage
   - **Recipe Name Text**: Drag the RecipeNameText
   - **Timer Bar**: Drag the TimerBar (filled image)
   - **Timer Text**: Drag the TimerText
   - **Green/Yellow/Red Colors**: Set your preferred colors

### 5.5 Create Prefab and Setup CustomerUI
1. Create the CustomerOrderPanel prefab
2. Delete from scene
3. Add **CustomerUI** script to CustomerOrderContainer
4. Configure CustomerUI:
   - **Customer Order Panel Prefab**: Drag your CustomerOrderPanel prefab
   - **Order Panel Container**: Reference itself (the container)
   - **Panel Slide Speed**: 5

## Step 6: Integration Testing

### 6.1 Test Spawning
1. Play the game
2. Check console for CustomerManager debug messages
3. Customers should spawn at intervals and move to order position

### 6.2 Test Orders
1. Place ingredients on CookingStation
2. Use ValidateAndCook (you may need to add a button or key binding)
3. Check if orders are delivered to customers
4. Verify scoring includes tips

### 6.3 Test UI (EyePlayer)
1. Make sure CustomerUI only shows for EyePlayer
2. Check that order panels appear when customers reach order position
3. Verify timer colors change (green → yellow → red)
4. Confirm panels slide left when customers leave

## Step 7: Optional Enhancements

### 7.1 Add Customer Animations
- Add Animator component to Customer prefab
- Create animations for waiting, happy, angry states
- Trigger animations based on CustomerState changes

### 7.2 Add Sound Effects
- Add CUSTOMER_HAPPY and CUSTOMER_ANGRY to SoundType enum
- Update Customer.PlayCustomerSoundClientRpc() method

### 7.3 Visual Polish
- Add customer models/sprites
- Improve UI design
- Add particle effects for happy/angry customers

## Troubleshooting

### Common Issues:
1. **Customers not spawning**: Check Network Prefabs list includes Customer prefab
2. **UI not showing**: Verify IsEyePlayer() logic in CustomerUI
3. **Orders not delivering**: Ensure Recipe references match exactly
4. **Network errors**: Make sure all customer GameObjects have NetworkObject components

### Debug Tips:
- Enable debug logs in CustomerManager
- Check console for error messages
- Use Unity's Network Profiler for network debugging
- Test in both Host and Client modes

## File Structure Summary
```
Assets/
├── Scripts/
│   ├── Customer/
│   │   ├── Customer.cs ✓
│   │   └── CustomerUI.cs ✓
│   ├── Manager/
│   │   └── CustomerManager.cs ✓
│   ├── ScriptableObjects/
│   │   └── CustomerOrderData.cs ✓
│   └── Interactable/
│       └── CookingStation.cs (modified) ✓
├── Prefabs/
│   ├── CustomerPrefab ✓
│   └── CustomerOrderPanel ✓
└── ScriptableObjects/
    ├── CustomerOrderData.asset ✓
    └── [Your Recipe Assets] ✓
```

Follow these steps in order, and you'll have a fully functional customer system integrated with your existing cooking game!
