# Quick Customer System Test Setup

## Immediate Testing Steps (5 minutes)

### 1. Create CustomerOrderData Asset
1. Right-click in Project → `Create > ScriptableObjects > CustomerOrderData`
2. Name: "TestCustomerOrderData"
3. **Available Recipes**: Add at least one Recipe ScriptableObject
4. Keep default values for now

### 2. Test Customer Manager Setup
1. Find your existing GameManager in the scene
2. Add a new empty GameObject named "CustomerManager"
3. Add **CustomerManager** script
4. Add **NetworkObject** component
5. Assign your CustomerOrderData asset

### 3. Create Simple Position Markers
```
Create 3 empty GameObjects:
- CustomerSpawnPoint (position: entrance area)
- CustomerOrderPosition (position: near cooking station)  
- CustomerExitPosition (position: exit area)
```

### 4. Create Basic Customer Prefab
1. Create empty GameObject → "Customer"
2. Add components:
   - **Customer** script
   - **NetworkObject**
   - **Capsule Collider**
   - **Rigidbody** (IsKinematic = true)
3. Add to Network Prefabs list in NetworkManager
4. Make prefab

### 5. Link Everything
In CustomerManager Inspector:
- Customer Prefab → your Customer prefab
- Order Data → your CustomerOrderData asset
- Spawn/Order/Exit positions → your position GameObjects

### 6. Test Basic Functionality
1. Play game
2. Watch console for spawn messages
3. Customer should appear and move to order position
4. Test cooking system with ingredients

## Quick Debug Tips
- Enable "Enable Debug Logs" in CustomerManager
- Console will show customer spawn/movement messages
- If customers don't spawn, check Network Prefabs list

## Next Steps After Basic Test
- Follow full setup guide for UI system
- Add proper customer models/animations
- Configure UI for EyePlayer display

This minimal setup will let you test the core customer spawning and order delivery system!
