# Scene Aufbau & Layer-Regeln

## 1. Backend (Optional)
- **Runtime / Engine-Backend**
- Kamera, Rendering-Pipeline, Post-Processing
- Muss nicht jede Szene haben
- **Kennt andere Layer nicht** → Scene & Presentation ❌
- Interface: `ISceneWithRuntime`
- Verantwortlich für:
  - Komponenten-Update / Draw
  - RenderTargets / Post-Processing

## 2. Core Scene (Zentral)
- **State & GameObjects**
- Immer vorhanden
- Kennt Backend ✅
- Kennt Presentation optional ✅
- Interface: `IScene`
- Methoden:
  - `Load()`, `Unload()`
  - `OnEnter(SceneContext)`, `OnExit()`
  - `Update(GameContext)`, `Draw(GameContext)`

## 3. Frontend (Optional)
- **Presentation / UI Layer**
- HUD, Overlays, UI
- Kennt Core Scene ✅
- Kennt Backend ✅
- Interface: `ISceneWithPresentation`
- Methoden:
  - `DrawUI(GameContext)`

## Update / Draw Reihenfolge
1. Backend (Runtime)  
2. Scene (Core)  
3. Frontend (Presentation)

## Zugriffsregeln Übersicht

| Layer        | Kennt Layer				      |
|--------------|----------------------------------|
| Backend      | –							      |  
| Zentral      | Backend & Frontend (je optional) |  
| Frontend     | Zentral & Backend			      |  