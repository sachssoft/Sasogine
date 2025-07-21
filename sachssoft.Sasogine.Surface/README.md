# Sasogine Surface

**Sasogine Surface** is a modern, clean, and lightweight UI component framework for MonoGame, based on [Myra](https://github.com/rds1983/Myra) – but with major improvements in code quality, design, and usability.

---

## 🌐 English

### 🎯 Why Replace Myra?

While Myra is a solid UI toolkit, it had several issues in code quality and design that didn’t meet the standards of Sachssoft/Saso.  
Sasogine Surface addresses these issues and enhances the overall developer and user experience.

### ✅ Key Improvements

- Better **code quality** with a clean, modern structure  
- Improved **readability and maintainability**  
- Visually **refined UI design**  
- Numerous **bug fixes**  
- Removed **unnecessary features**

> Sasogine Surface offers a **streamlined UI core**, derived from Myra, focusing on clarity and reliability.

### 💡 Other Notes

- Free and open-source (MIT License)  
- Ideal for game developers using MonoGame

### 🔭 Future Plans

- New UI components  
- Reliable and easy-to-use **API**  
- Continuous improvements in **performance and usability**

---
## 🇬🇧 English  

### 🎨 ColoredRegion  
- Displays a plain color without any texture.  
- Useful for simple backgrounds, overlays, or UI elements.  

### 🖼 TextureRegion  
- A simple subregion of a texture (sub-rectangle).  
- Uses `UV` coordinates for GPU-friendly rendering.  

### 🧩 TiledRegion  
- Repeats (tiles) a texture across an area.  
- Useful for patterns, backgrounds, or floor textures.  

### 🪟 NinePatchRegion  
- Smart stretching:  
  - Corners remain unscaled  
  - Edges stretch only in one direction  
  - Center area fills flexibly  
- Perfect for buttons, panels, window frames.  

### ➖ ThreePatchRegion  
- Simplified version of NinePatch.  
- Stretches **only horizontally or vertically**.  
- Suitable for status bars, separators, tabs.  

#### Typical Usage  

| Region Type        | Example |
|--------------------|---------|
| `ColoredRegion`    | Background color, overlay |
| `TextureRegion`    | Sprite, icon, single tile |
| `TiledRegion`      | Floor texture, pattern |
| `NinePatchRegion`  | UI panels, window frames |
| `ThreePatchRegion` | Progress bar, tabs, headers |

#### Benefits  
- **Reusability**: Simple textures, displayed flexibly  
- **Performance**: All regions share the same GPU logic  
- **UI-friendly**: Ideal for dynamically scalable surfaces  

---

## 🇩🇪 Deutsch

### 🎯 Warum eine Ablösung von Myra?

Myra ist ein solides UI-Toolkit, zeigte jedoch Schwächen in der Code-Qualität und im Design, die nicht dem Anspruch von Sachssoft/Saso entsprachen.  
Sasogine Surface setzt genau hier an und verbessert die Entwickler- und Nutzererfahrung spürbar.

### ✅ Hauptvorteile

- Höhere **Code-Qualität** durch moderne Struktur  
- Besseres **Verständnis und Wartbarkeit**  
- **Schöneres UI-Design** mit überarbeiteten Komponenten  
- Zahlreiche **Fehlerbehebungen**  
- Entfernung **unnötiger Funktionen**

> Sasogine Surface stellt eine **verschlankte, stabile UI-Basis** dar, die auf Myra aufbaut und gezielt verbessert wurde.

### 💡 Sonstiges

- Kostenlos und quelloffen (MIT-Lizenz)  
- Ideal für Entwickler mit MonoGame

### 🔭 Zukünftige Planungen

- Neue UI-Komponenten  
- Zuverlässige und einfache **API**  
- Stetige Verbesserung von **Performance und Bedienbarkeit**

- # Region-Typen in Sasogine / Myra  

---

## 🇩🇪 Deutsch  

### 🎨 ColoredRegion  
- Zeigt eine reine Farbfläche ohne Textur.  
- Nützlich für einfache Hintergründe, Overlays oder UI-Elemente.  

### 🖼 TextureRegion  
- Einfacher Ausschnitt einer Textur (Sub-Rectangle).  
- Nutzt `UV`-Koordinaten für GPU-gerechtes Rendering.  

### 🧩 TiledRegion  
- Wiederholt (tilet) eine Textur auf einer Fläche.  
- Praktisch für Muster, Hintergründe oder Bodentexturen.  

### 🪟 NinePatchRegion  
- Intelligentes Stretching:  
  - Ecken bleiben unskaliert  
  - Kanten werden nur in eine Richtung gestreckt  
  - Mittlere Fläche wird flexibel gefüllt  
- Ideal für Buttons, Panels, Fensterrahmen.  

### ➖ ThreePatchRegion  
- Vereinfachte Variante von NinePatch.  
- Stretcht **nur horizontal oder vertikal**.  
- Geeignet für Statusleisten, Separatoren, Tabs.  

#### Typische Verwendung  

| Region-Typ         | Beispiel |
|--------------------|----------|
| `ColoredRegion`    | Hintergrundfarbe, Overlay |
| `TextureRegion`    | Sprite, Icon, einzelnes Tile |
| `TiledRegion`      | Bodentextur, Muster |
| `NinePatchRegion`  | UI-Panels, Fensterrahmen |
| `ThreePatchRegion` | Progress-Bar, Tabs, Header |

#### Vorteile  
- **Wiederverwendbarkeit**: Einfache Texturen, flexibel darstellbar  
- **Performance**: Alle Regionen basieren auf derselben GPU-Logik  
- **UI-freundlich**: Ideal für dynamisch skalierbare Oberflächen  
