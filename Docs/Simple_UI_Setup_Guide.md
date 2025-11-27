# Simple UI Setup Guide

## What's New:
- ✅ **Start Screen** - Press button to start game
- ✅ **Game Over Screen** - Shows final score and restart button  
- ✅ **Death Mechanic** - Player dies when falling below Y = -10
- ✅ **Restart Functionality** - Cleans up and restarts game

---

## UI Setup (3 Simple Panels):

### **1. Start Panel**
```
Canvas
└── StartPanel
    ├── Title (Text: "DOOFUS")
    └── StartButton
        └── Text: "START"
```

### **2. HUD Panel**
```
Canvas
└── HUDPanel
    └── ScoreText (Your existing score display)
```

### **3. Game Over Panel**
```
Canvas
└── GameOverPanel
    ├── GameOverText (Text: "GAME OVER")
    ├── FinalScoreText (Text: "Final Score: 0")
    └── RestartButton
        └── Text: "RESTART"
```

---

## Component Setup:

### **On Canvas GameObject:**
1. Add `GameUIManager` component
2. Assign references:
   - **Start Panel** → StartPanel GameObject
   - **Game Over Panel** → GameOverPanel GameObject  
   - **HUD Panel** → HUDPanel GameObject
   - **Start Button** → Start button
   - **Restart Button** → Restart button
   - **Final Score Text** → Final score text

---

## How It Works:

### **Game Flow:**
1. **Start** → Shows Start Screen
2. **Press START** → Hides start screen, shows HUD, spawns first pulpit
3. **Play game** → Score updates as you visit pulpits
4. **Fall below Y = -10** → Game Over screen appears
5. **Press RESTART** → Destroys all pulpits, resets player, starts fresh

### **Death Mechanic:**
- Player automatically dies when `transform.position.y < -10`
- Customizable in Inspector: `Fall Death Threshold`
- Movement stops on death
- Death event triggers Game Over

---

## Animations Included:

- ✅ **Panels** - Pop in with OutBack ease (bouncy entrance)
- ✅ **Buttons** - Punch scale on click (satisfying feedback)
- ✅ **Score** - Already has your dynamic punch effect

---

## Testing:
1. Play scene
2. Should see START screen
3. Click START → Game begins
4. Walk off platform edge → Game Over appears
5. Click RESTART → Everything resets

---

## Customization:

### Change Death Height:
```csharp
// In PlayerMovementhandler
[SerializeField] private float fallDeathThreshold = -10f;
```

Set to `-5f` for earlier death, `-20f` for later death.

### Change Button Animations:
Modify in `GameUIManager.cs`:
```csharp
startButton.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 5, 0.5f); // Bigger punch
```

---

That's it! Super simple, fully functional game loop with restart! 🎮
