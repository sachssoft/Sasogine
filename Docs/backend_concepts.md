# Sasogine 2.x – Vergleich und Planung der Grafik-Backends

Stand der Quellenprüfung: 17. Oktober 2026.

## Zielsetzung

Sasogine 2.x soll beim möglichen Wechsel von MonoGame eine kleine, eigene Grafikabstraktion erhalten. Im Renderpfad stehen drei Operationen im Mittelpunkt:

- `Clear`: Farb- und gegebenenfalls Tiefenpuffer löschen.
- `DrawMesh`: vorbereitete Geometrie mit einer festgelegten Pipeline zeichnen.
- `Present`: den Frame abschließen und präsentieren.

Initialisierung, Ressourcenverwaltung, Größenänderungen und Freigabe gehören zusätzlich zum Lebenszyklus. Eine kleine öffentliche Schnittstelle reduziert die sichtbare Komplexität; das Backend muss GPU-Synchronisation und Ressourcenlebenszeiten trotzdem korrekt behandeln.

## Gesamtbewertung: Vorteile, Nachteile und Performance

Bewertungsziel ist ein nativer C#-Renderer für Sasogine mit `Clear`, `DrawMesh` und `Present`. Alle 14 Kandidaten sind hier nach denselben Kriterien zusammengefasst.

**Die Zahlen sind subjektive Planungseinschätzungen, keine gemessenen Geschwindigkeitswerte.** „Performance-Potenzial“ bewertet den Spielraum für einen gut optimierten Renderer: Einfluss auf Befehlsaufzeichnung, Speicherzugriffe, Ressourcenverwaltung und Synchronisation. Die Bewertung sagt nicht voraus, welche Bibliothek in einer konkreten Szene die meisten FPS erreicht. Die Backend- und Binding-Eigenschaften stammen aus den jeweils unten verlinkten Projektquellen; die Zahlen sind meine daraus abgeleitete Einschätzung.

- **5 / 5:** Sehr großer Optimierungsspielraum bei entsprechendem Implementierungsaufwand.
- **4,5 / 5:** Hoher Spielraum mit einer gemeinsamen Abstraktion oder zusätzlichen Integrationsbedingungen.
- **4 / 5:** Hohes Potenzial für den vorgesehenen Mesh-Renderer, mit stärker vorgegebenem API-Modell.
- **3,5 / 5:** Für einen einfachen Renderer gut geeignet; begrenzter Spielraum für eine spätere stark explizite GPU-Architektur.
- **Sasogine-Eignung:** Separate Gesamtwertung für C#-Integration, Minimalismus, Plattformabdeckung und Entwicklungsaufwand. Die vier ursprünglichen Eignungswerte bleiben als vorläufige Ausgangseinschätzungen erhalten.

| Bibliothek / Ansatz | Vorteile | Nachteile | Performance-Potenzial | Eignung für Sasogine |
| --- | --- | --- | --- | --- |
| **Silk.NET mit eigener Vulkan-/Direct3D-Implementierung** | Direkter API-Zugriff; Speicher, Queues und Befehle selbst optimierbar; eigene schlanke Engine-Abstraktion möglich | Sehr hoher Aufwand; Synchronisation und Ressourcenlebenszeiten selbst lösen; separate API-Implementierungen und Plattformtests | **5 / 5** – bei fachgerecht optimierter eigener Implementierung | **4,2 / 5** |
| **Zenith.NET** | Gemeinsame C#-RHI über moderne Backends; native Metal-Option; Shader-Compilerpaket | Explizite Synchronisation bleibt erforderlich; API-Stabilität und Geräteanforderungen prüfen; geringe Integrationsarbeit nicht voraussetzen | **4,5 / 5** – großer expliziter Gestaltungsspielraum; tatsächlicher Overhead noch zu messen | **4,5 / 5** |
| **Veldrid** | Gemeinsame Geräte-, Ressourcen- und Pipeline-API; mehrere Desktop-Backends; C#-Nutzung | Kein dokumentiertes Direct3D 12; gewünschte Funktionen und Wartung der Version prüfen; Abstraktion begrenzt native Sonderwege | **4 / 5** – gut für den geplanten Renderer; konkrete Backend-Implementierung entscheidend | **4 / 5** |
| **WebGPU über wgpu** | Portables API-Modell; gemeinsame Ressourcen- und Pipeline-Konzepte; mehrere native Backends | Validierungs- und Abstraktionsarbeit kann CPU-Zeit kosten; native Erweiterungen nicht überall verfügbar; passende .NET-/Native-Versionen nötig | **4 / 5** – hohes Potenzial im WebGPU-Modell; native Spezialoptimierungen eingeschränkt | **3,8 / 5** |
| **SDL3 GPU** | Kleine gemeinsame GPU-API; Vulkan, Direct3D 12 und Metal; passende Fensterintegration über SDL | Shaderformate und Uploads selbst planen; GPU-Funktionen im C#-Binding prüfen; weniger native Detailkontrolle als mit direkten Bindings | **4,5 / 5** – guter Spielraum für einen kompakten Renderer; Interop und Uploads messen | **4,5 / 5** |
| **bgfx** | Breite Backend-Auswahl; Integration in eigene Engine vorgesehen; Handle- und Submission-Modell | Anpassung an Views und Submission nötig; eigener Shader-Workflow; C#-Binding und native Version koppeln | **4,5 / 5** – hohes Potenzial bei passender Nutzung des vorgegebenen Rendermodells | **4 / 5** |
| **DiligentCore** | Breite RHI; Zugriff auf mehrere Grafik-APIs; .NET-Integration im Projekt | Größere native Abhängigkeit; Paketierung und .NET-Funktionsabdeckung prüfen; mehr Integrationsumfang als eine kleine Binding-Schicht | **4,5 / 5** – großer Gestaltungsspielraum; Ressourcenbindung und Interop im Prototyp prüfen | **4 / 5** |
| **LLGL** | Gemeinsame Low-Level-API; mehrere Backends; C#-Wrapper im Hauptprojekt | Beta-Stand; Wrapper-Abdeckung und native Builds prüfen; Verhalten pro Backend validieren | **4 / 5** – vorläufige Einschätzung mit hoher Unsicherheit ohne C#-Messungen | **3,5 / 5** |
| **sokol_gfx** | Kleine API; kompakte Integration; getrennte Grafik- und Fensterbausteine | C# über zusätzliche Bindings; Backend-Auswahl an Build gebunden; Vulkan experimentell; keine direkte D3D12-Option in der hier geprüften Auswahl | **4 / 5** – gut für einen einfachen Renderer; weniger Spielraum für explizite Spezialpfade | **4,5 / 5** |
| **Vortice.Windows** | Direkte .NET-Bindings für Direct3D und DXGI; eigene Windows-Pipeline frei gestaltbar | Windows-Fokus; kein gemeinsamer Metal-/Vulkan-Pfad; Direct3D-12-Verwaltung selbst implementieren | **5 / 5** – für einen eigenen optimierten Direct3D-12-Pfad; Direct3D 11 separat bewerten | **3,5 / 5** plattformübergreifend; **4,5 / 5** bei Windows-Fokus |
| **OpenTK** | Direkter OpenGL-Zugriff aus C#; überschaubarer Einstieg; Fenster- und Eingabefunktionen | Keine gemeinsame moderne Mehrbackend-RHI; OpenGL-Zustandsmodell; Geräte- und Plattformgrenzen prüfen | **3,5 / 5** – für einfache Szenen ausreichend; geringer bewertet wegen des expliziten Optimierungsspielraums, nicht wegen gemessener FPS | **3,5 / 5** |
| **Dawn** | WebGPU-Modell mit mehreren nativen Backends; Alternative zu wgpu | C#-Binding separat wählen; Native-Auslieferung und Versionskompatibilität; gleiche grundsätzlichen WebGPU-Abstraktionsgrenzen | **4 / 5** – ohne Vergleichsmessung kein Geschwindigkeitsvorsprung gegenüber wgpu angenommen | **3,8 / 5** |
| **NVRHI** | Gemeinsame explizite RHI; optionale automatische Statusverwaltung; mehrere Queues | Native C++-/C#-Brücke einplanen; kein dokumentiertes Metal; zusätzliche Build- und Auslieferungsarbeit | **4,5 / 5** – hoher nativer Spielraum; Qualität der C#-Brücke entscheidend | **3 / 5** |
| **The Forge** | Umfangreicher nativer Renderer-Unterbau; mehrere Grafik-APIs; eigene Shader-Werkzeuge | Hoher Integrationsaufwand für C#; für Minimalziel großer Umfang; Projektfortsetzung und Paketierung prüfen | **4,5 / 5** – hohes natives Potenzial, ohne Nachweis eines Vorteils für diesen kleinen Renderer | **2,5 / 5** |

**Einordnung:** Ein gut gebauter OpenGL- oder Direct3D-11-Renderer kann in einer kleinen Szene schneller sein als eine schlecht abgestimmte Vulkan- oder Direct3D-12-Implementierung. Bei GPU-limitierter Darstellung bestimmen Shader, Auflösung und Speicherzugriffe häufig mehr als die Binding-Bibliothek. Unterschiede von 0,5 Punkten sind hier Auswahlhilfen und keine belastbaren Leistungsabstände.

Für die aktuelle Planung würde ich **SDL3 GPU und Zenith.NET zuerst vergleichen**. **sokol_gfx** ist besonders interessant, wenn geringer Umfang Vorrang hat. **Silk.NET und Vortice.Windows** passen, wenn der Aufwand für eine eigene native API-Implementierung ausdrücklich gewünscht ist.

### Performance im Prototyp überprüfen

Für belastbare Werte dieselbe Hardware, Auflösung, Shader, Geometrie und Qualität verwenden. Wenn möglich, auch dasselbe native Grafik-Backend wählen. Release-Build, Runtime, VSync, Frames in Flight und Validierungsmodus festhalten; Start- und Shader-Kompilierungszeiten getrennt von der laufenden Darstellung messen.

| Messfall | Was er sichtbar machen soll |
| --- | --- |
| Viele kleine Draw-Aufrufe bei geringer GPU-Last | CPU-Kosten für C#-Interop, Befehlsaufzeichnung und Submission |
| Wenige große Draw-Aufrufe mit identischer Shaderlast | GPU-Zeit des eigentlichen Renderpfads |
| Wiederholte Mesh- und Textur-Uploads | Transferkosten, Allokationen und unerwünschte Wartezeiten |
| Längerer Lauf mit mehreren Frames in Flight | Ressourcenwiederverwendung, Speicherverbrauch und Frame-Time-Ausreißer |

CPU- und GPU-Zeiten getrennt ausweisen und neben dem Median auch langsame Frames betrachten. Erst daraus eine gemessene Performance-Wertung ableiten. Die Tabelle oben bleibt bis dahin eine Potenzialeinschätzung.

## NativeAOT: Unterstützung, Nachteile und Bewertung

Mit AOT ist hier **.NET NativeAOT für die C#-Anwendung** gemeint. Eine bereits native C-/C++-Bibliothek macht ihr C#-Binding nicht automatisch AOT-kompatibel. Fensterintegration, Callbacks, Ressourcenlader und optionale Erweiterungen gehören zur Prüfung dazu.

NativeAOT übersetzt den verwalteten Anwendungscode beim Veröffentlichen in nativen Code. GPU-Shader sind ein separater Kompilierungsschritt. AOT kann Startzeit und Speicherbedarf verbessern; höhere FPS folgen daraus nicht automatisch. Deshalb bleiben Performance-Potenzial und AOT-Bewertung getrennt. Grundlagen: [Microsoft NativeAOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

### Bewertung aller 14 Kandidaten

Die Punkte bewerten **den derzeit belegbaren Einstieg in NativeAOT für Sasogine**, einschließlich Bindings und Integrationsaufwand. Sie sind subjektive Planungshilfen, keine Zertifizierung:

- **5 / 5:** Das betrachtete Binding dokumentiert NativeAOT ausdrücklich als vorgesehenen Einsatzweg.
- **4 / 5:** Es gibt konkrete Unterstützungshinweise oder Nutzungserfahrungen; Version und eingesetzte Module bleiben zu prüfen.
- **3 / 5:** Technisch plausibel oder experimentell, aber die konkrete Paketkombination ist nicht abgesichert.
- **2 / 5:** Kein ausreichender Nachweis für den vorgesehenen C#-Weg oder zusätzlicher Aufwand für eine native Brücke.
- **1 / 5:** Ein belegter Blocker verhindert den gewählten Weg ohne größere Änderungen. Diese Einstufung wird hier mangels eines solchen Nachweises nicht vergeben.

**Nicht verifiziert bedeutet nicht inkompatibel.** Die Bewertung gilt für den dokumentierten oder vorgeschlagenen Integrationsweg, nicht für jede denkbare eigene Binding-Implementierung. Kein Kandidat wurde im Rahmen dieser Dokumentüberarbeitung selbst als NativeAOT-Anwendung veröffentlicht oder ausgeführt.

| Kandidat | NativeAOT-Status | Vorteil für AOT | Nachteil / erforderliche Prüfung | AOT-Einstieg x/5 |
| --- | --- | --- | --- | --- |
| **Silk.NET** | Unterstützung im Projekt bestätigt; konkrete Module prüfen [A1] | Direkte Bindings; explizite Registrierung kann automatische Plattformfindung ersetzen | Fenster-/Input-Backend registrieren, falls die Version dies benötigt; Trimming und Zusatzpakete testen | **4 / 5** |
| **Zenith.NET** | In den geprüften Hauptquellen kein ausdrücklicher NativeAOT-Nachweis gefunden | Gemeinsame C#-API erlaubt einen begrenzten AOT-Prototyp | Geräteerzeugung, Backend-Laden, Shader-Compiler und Abhängigkeiten separat veröffentlichen und testen | **3 / 5**, unbestätigt |
| **Veldrid** | Kein dokumentierter Support laut Microsofts Bibliotheksübersicht [A2] | Klare Grafikabstraktion ermöglicht einen abgegrenzten Test | Erst AOT-Publish und Rendering nachweisen; Fenster- und Shader-Zusatzpakete gesondert prüfen | **2 / 5**, unbestätigt |
| **WebGPU über wgpu** | Vom gewählten .NET-Binding abhängig; hier kein konkretes Binding AOT-verifiziert | Native C-Anbindung grundsätzlich als Interop-Weg geeignet | Native wgpu-Version, C-ABI, Callbacks und Marshalling müssen zum Binding passen | **3 / 5**, bedingt |
| **SDL3 GPU über SDL3-CS** | SDL3 ist nativ; NativeAOT der konkreten SDL3-CS-/GPU-Kombination hier nicht nachgewiesen | C-API bietet einen direkten Interop-Weg | GPU-API-Abdeckung, Callback-Lebenszeiten und native Bereitstellung testen; kein pauschales Supportversprechen | **3 / 5**, bedingt |
| **bgfx** | Vom konkreten C#-Binding abhängig; hier nicht AOT-verifiziert | Native C-API als mögliche Integrationsgrenze | Binding kann eigene Einschränkungen haben; Speicherübergaben, Callbacks und native Versionsgleichheit prüfen | **3 / 5**, bedingt |
| **DiligentCore** | .NET-Integration vorhanden; hier kein bestätigter NativeAOT-Pfad | Vorhandene .NET-Schicht als Ausgangspunkt | Prüfen, ob die verwendete Integration mit NativeAOT funktioniert; ein C++/CLI-Weg wäre ungeeignet | **2 / 5**, unbestätigt |
| **LLGL** | C#-Wrapper vorhanden; NativeAOT hier nicht nachgewiesen | C99-Schnittstelle als möglicher direkter Interop-Weg | Wrapper, Rückrufe und native Objektlebenszeiten prüfen; Wrapper-Existenz belegt keine AOT-Fähigkeit | **3 / 5**, unbestätigt |
| **sokol_gfx über Sokol.NET** | NativeAOT für Desktop und mobile Zielwege ausdrücklich dokumentiert [A3] | AOT ist Bestandteil des Binding-/Build-Konzepts | An die dokumentierten Zielplattformen und Backends halten; zusätzliche Module gesondert testen; WASM nicht mit Desktop-NativeAOT gleichsetzen | **5 / 5** für den dokumentierten Weg |
| **Vortice.Windows** | Historische erfolgreiche NativeAOT-Nutzung, aber auch versionsabhängige Trimming-Probleme dokumentiert [A4] | Direkte Windows-Grafikanbindung ohne eigenen C++-Renderer möglich | DXGI-/COM-Wrapper und SharpGen-Version prüfen; keine pauschale Garantie für alle aktuellen Pakete | **4 / 5**, versionsabhängig |
| **OpenTK** | Von Microsoft als experimentell mit Community-Konfigurationen eingeordnet [A2] | NativeAOT-Einsatz ist grundsätzlich berichtet | Keine dort dokumentierte Herstellerzusage; Fenster-, Binding- und Ladepfade der gewählten Version testen | **3 / 5**, experimentell |
| **Dawn** | Native Bibliothek; konkretes .NET-Binding hier nicht AOT-verifiziert | C-Schnittstelle erlaubt einen begrenzten Interop-Adapter | Callback-/Future-Verarbeitung, ABI und native Paketierung passend zum Binding prüfen | **3 / 5**, bedingt |
| **NVRHI** | Natives C++; kein fertiger AOT-geprüfter C#-Weg in dieser Auswahl | Eigene kleine C-Schnittstelle kann kontrollierbar gestaltet werden | Native C-Brücke und C#-Interop zusätzlich entwickeln und warten; C++/CLI ist kein NativeAOT-Ausweg | **2 / 5** wegen Integrationsaufwand |
| **The Forge** | Natives Framework; kein fertiger AOT-geprüfter C#-Weg in dieser Auswahl | Nativer Kern kann hinter einer eigenen C-Schnittstelle verbleiben | Brücke, Build, Speicherbesitz und Rückrufe zusätzlich umsetzen; hoher Umfang für Sasogines Minimalziel | **2 / 5** wegen Integrationsaufwand |

Die Angaben „technisch plausibel“ und „bedingt“ sind Ableitungen aus der jeweiligen C-/Interop-Architektur, keine bestätigten Supportaussagen der Projekte. Die Projektquellen der einzelnen Bibliotheken stehen in deren Abschnitten weiter unten.

### Quellen zum AOT-Status

- **[A1]** [Silk.NET: beantwortete NativeAOT-Frage](https://github.com/dotnet/Silk.NET/discussions/2500). Das Projekt bestätigt Unterstützung und nennt explizite GLFW-Registrierung. Ergänzend dokumentieren die [Releases](https://github.com/dotnet/Silk.NET/releases) Verbesserungen für NativeAOT. Microsoft nennt in seiner Übersicht einen anderen Versionsrahmen; deshalb die tatsächlich verwendete Version prüfen.
- **[A2]** [Microsoft: NativeAOT für Spiele, Bibliotheksunterstützung](https://learn.microsoft.com/en-us/gaming/gdk/docs/gdk-dev/pc-dev/tutorials/get-started-with-custom-engine/native-aot-for-gaming?view=gdk-2604). OpenTK wird als experimentell, Veldrid ohne dokumentierten Support aufgeführt.
- **[A3]** [Sokol.NET: Projektbeschreibung und Plattformmatrix](https://github.com/elix22/Sokol.NET). NativeAOT wird unter anderem für Windows, macOS und Linux ausdrücklich genannt. Das gilt für Sokol.NET und die dort dokumentierten Backends, nicht automatisch für jedes Sokol-Binding.
- **[A4]** [Vortice.Windows: NativeAOT-/Trimming-Diskussion](https://github.com/amerkoleci/Vortice.Windows/discussions/379). Historischer Erfahrungsnachweis mit späteren Problemberichten, kein aktueller umfassender Kompatibilitätstest. Der dort erwähnte alternative Binding-Weg Vortice.Win32 ist ein separates Projekt.

### Konsequenz für die Backend-Auswahl

Wenn NativeAOT eine feste Voraussetzung ist, würde ich **Sokol.NET und Silk.NET zuerst als vollständige AOT-Anwendungen testen**. Sokol.NET hat einen ausdrücklich dokumentierten AOT-Weg; Silk.NET bietet mehr direkte API-Kontrolle mit größerem Eigenaufwand. Für einen Windows-Fokus ist Vortice.Windows nach Prüfung der konkreten Paketversion ebenfalls ein Kandidat.

SDL3 GPU und Zenith.NET bleiben interessant, erhalten aber vor einer Festlegung einen eigenen AOT-Nachweis. Die allgemeine Empfehlung weiter unten gilt deshalb unter dem Vorbehalt dieser zusätzlichen Anforderung. Fehlender Dokumentationsnachweis ist ein offener Prüfpunkt und kein automatischer Ausschluss.

### Minimaler NativeAOT-Nachweis für Sasogine

Für ein ausführbares .NET-Projekt, beispielsweise mit `net10.0`, wird NativeAOT in der Projektdatei aktiviert:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

Beispiel für Windows x64, ausgeführt im Projektordner mit installierter nativer Build-Toolchain:

```powershell
dotnet publish -c Release -r win-x64
```

Die erzeugte Anwendung direkt aus dem Publish-Ordner starten. Für andere Zielsysteme einen eigenen unterstützten Build-Weg verwenden. Diese Befehle wurden hier nicht ausgeführt, da kein Sasogine-Projekt vorliegt.

Der Nachweis umfasst Geräte-/Fenstererzeugung, Clear, Mesh-Upload, Draw, Present, Resize, Callbacks und sauberes Beenden. AOT- und Trimming-Warnungen auflösen; nicht lediglich unterdrücken. Zusätzlich native Bibliotheken auf einem Zielsystem ohne installierte .NET-Runtime prüfen.

Für die Engine-Architektur bedeutet das: Backend-Factories direkt referenzieren, benötigte generische Instanziierungen statisch erreichbar halten und Reflection auf erhaltene Metadaten begrenzen. Dynamisches Nachladen verwalteter Backend-Assemblies und Laufzeit-Codegenerierung sind kein geeigneter NativeAOT-Pfad. Das Laden nativer Bibliotheken ist davon zu unterscheiden. GPU-Shader-Kompilierung bleibt grundsätzlich möglich, sofern die verwendete native Compiler-Integration funktioniert.

NativeAOT setzt Trimming voraus und unterstützt weder C++/CLI noch die eingebaute Windows-COM-Interop; eine geeignete Binding-Implementierung kann COM dennoch explizit anbinden. Die Auslieferung kann weiterhin zusätzliche native Bibliotheken benötigen. Quelle für Publish und Einschränkungen: [Microsoft NativeAOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

## Vergleich der ursprünglichen vier Kandidaten

Die Bewertungen sind subjektive Einschätzungen aus dem Ausgangstext, keine Benchmarks. Sie dienen der Vorauswahl und müssen durch einen Prototyp bestätigt werden.

| Bibliothek / Ansatz | Grafik-APIs / Backends | Vorteile für Sasogine | Aufwand und Grenzen | Vorläufige Bewertung |
| --- | --- | --- | --- | --- |
| Silk.NET mit eigener nativer Implementierung | Unter anderem Vulkan, DirectX und OpenGL über separate Bindings | Direkter API-Zugriff; eigene Entscheidungen über Speicher, Befehlsaufzeichnung und Synchronisation | Hoher Eigenaufwand; Vulkan und Direct3D 12 brauchen eigene Implementierungen hinter der gemeinsamen Engine-Schnittstelle | 4,2 / 5 – wenn maximale Kontrolle den Aufwand rechtfertigt |
| Zenith.NET | DirectX 12, Vulkan 1.4, Metal 4 | Gemeinsame C#-RHI; native Metal-Implementierung; Slang-Compilerpaket | Explizite Barrieren, Übergänge und Queue-Koordination bleiben Teil der Arbeit; Paketstand, Geräteanforderungen und API-Stabilität prüfen | 4,5 / 5 – Kandidat für einen gezielten Prototyp |
| Veldrid | Direct3D 11, Vulkan, Metal, OpenGL, OpenGL ES | Gemeinsame Abstraktionen für Geräte, Ressourcen, Pipelines und Befehlslisten | Kein Direct3D-12-Backend in der dokumentierten Backend-Liste; Wartung und benötigte Funktionen vor Einsatz prüfen | 4,0 / 5 – Alternative, wenn Direct3D 11 ausreicht |
| WebGPU über eine native Implementierung wie wgpu und passende .NET-Bindings | Bei wgpu unter anderem Vulkan, Direct3D 12 und Metal; abhängig von Plattform und Build | Gemeinsames API-Modell; gute Ausgangsbasis für einen portablen Renderpfad | Native Bibliotheken und Bindings müssen zusammenpassen; Features und Limits abfragen; herstellerspezifische Funktionen nicht voraussetzen | 3,8 / 5 – wenn Portabilität im Vordergrund steht |

Belege zu den API- und Backend-Angaben: [Silk.NET](https://github.com/dotnet/Silk.NET), [Zenith.NET](https://github.com/qian-o/Zenith.NET), [Veldrid GraphicsBackend](https://veldrid.dev/api/Veldrid.GraphicsBackend.html) und [wgpu](https://github.com/gfx-rs/wgpu).

### Silk.NET

Silk.NET stellt Bindings bereit. Eine gemeinsame Sasogine-RHI muss darüber selbst entstehen. OpenAL gehört zum Audiobereich und ist deshalb aus der Grafik-Backend-Liste entfernt. Für Vulkan auf macOS ist MoltenVK als gesonderte Übersetzungsschicht zu evaluieren.

Die Projektseite weist auf die Entwicklung von Silk.NET 3.0 und begrenzte Investitionen in 2.x hin. Daher die gewählte Paketversion ausdrücklich festhalten. Aussagen wie „ohne Overhead“ oder pauschale Performance-Versprechen ersetzen keine Messung im eigenen Renderpfad. Quelle: [Silk.NET-Projektseite](https://github.com/dotnet/Silk.NET).

### Zenith.NET

Die gemeinsame API umfasst Ressourcen, Pipelines, Befehlsaufzeichnung und Präsentation. Das Projekt beschreibt explizite Synchronisation mit Barrieren, Texturübergängen und Timelines. Die ursprüngliche Aussage, die RHI nehme Synchronisationsarbeit vollständig ab, ist deshalb zu weitgehend.

Die dokumentierten Backend-Pakete sind `Zenith.NET.DirectX12`, `Zenith.NET.Vulkan` und `Zenith.NET.Metal`; `Zenith.NET.Compiler` kompiliert Slang-Shader. Automatische Auswahl des „besten“ Backends darf Sasogine ohne Prüfung der konkreten API nicht voraussetzen. Quelle: [Zenith.NET-Projektseite](https://github.com/qian-o/Zenith.NET).

### Veldrid

Die dokumentierte Backend-Auswahl enthält Direct3D 11, Vulkan, Metal, OpenGL und OpenGL ES. Direct3D 12 wurde im Ausgangstext fälschlich aufgeführt. Ob Veldrid für Sasogine geeignet ist, hängt insbesondere davon ab, ob die angebotenen Funktionen und die Wartung der gewählten Version ausreichen. Quelle: [Veldrid GraphicsBackend](https://veldrid.dev/api/Veldrid.GraphicsBackend.html).

### WebGPU und wgpu

WebGPU bezeichnet das API-Modell; wgpu ist eine Implementierung mit mehreren nativen Backends. Für eine C#-Anwendung muss zusätzlich eine passende Binding- und Native-Paketkombination ausgewählt werden. Native Erweiterungen können über den portablen WebGPU-Funktionsumfang hinausgehen; ihre Verfügbarkeit ist gesondert zu prüfen. Eine einzige Engine-Implementierung entbindet nicht von Plattformtests. Quelle: [wgpu-Projektseite](https://github.com/gfx-rs/wgpu).

## Weitere Grafikbibliotheken und Backend-Alternativen

Die folgenden zehn Ergänzungen erweitern die Auswahl. Die Einschätzung der Eignung ist eine architektonische Bewertung für Sasogine, keine gemessene Rangliste. Bei nativen Projekten ist die Verfügbarkeit eines Backends nicht automatisch ein Nachweis, dass alle Funktionen auch über ein bestimmtes C#-Binding erreichbar sind.

### Überblick über zehn zusätzliche Kandidaten

| Bibliothek | Art | Grafik-APIs / Backends | C#-Anbindung | Einschätzung für Sasogine |
| --- | --- | --- | --- | --- |
| SDL3 GPU | Gemeinsame GPU-Abstraktion in C | Vulkan, Direct3D 12, Metal | Beispielsweise SDL3-CS; GPU-API-Abdeckung der gewählten Version prüfen | Sehr interessanter erster Prototyp für eine kleine, portable Renderabstraktion |
| bgfx | Native Renderingbibliothek | Unter anderem Direct3D 11/12, Vulkan, Metal, OpenGL/ES, WebGL und WebGPU über Dawn Native | C-API und auf der Projektseite verlinkte C#-Bindings | Gute Wahl bei breiter Plattformabdeckung und Akzeptanz des bgfx-Rendermodells |
| Diligent Engine / DiligentCore | Native RHI mit optionalen Rendering-Modulen | Direct3D 11/12, Vulkan, OpenGL/ES, Metal und WebGPU gemäß Projektübersicht | .NET-Integration und NuGet-Build im Projekt vorhanden | Interessant für eine umfangreichere RHI; native Paketierung und Backend-Verfügbarkeit prüfen |
| LLGL | Native Low-Level-Abstraktion | Direct3D 11/12, Vulkan, OpenGL und Metal, je nach Plattform | C#-Wrapper im Hauptprojekt | Passender RHI-Kandidat, sofern Wrapper und Beta-API die benötigten Funktionen abdecken |
| sokol_gfx | Kleine C-Grafikabstraktion | OpenGL/ES, WebGL2, Direct3D 11, Metal, WebGPU; Vulkan derzeit experimentell | Community-Bindings, beispielsweise Sokol.NET | Besonders interessant für ein kleines Backend ohne Pflicht zu direktem Direct3D 12 |
| Vortice.Windows | .NET-Bindings für Windows-APIs | Insbesondere Direct3D 11/12 und DXGI | Direkte .NET-Bibliothek | Gute Grundlage für einen eigenen Windows-Renderer; weitere Plattformen separat implementieren |
| OpenTK | .NET-Bindings mit Fenster-/Eingabefunktionen | OpenGL und OpenGL ES | Direkte .NET-Bibliothek | Überschaubarer OpenGL-Einstieg; keine gemeinsame Vulkan-/Direct3D-/Metal-RHI |
| Dawn | Native WebGPU-Implementierung | Unter anderem Direct3D 12, Metal und Vulkan | Passende externe .NET-Bindings oder eigene C-Interop nötig | Alternative Implementierung im bereits betrachteten WebGPU-Ansatz |
| NVRHI | Native C++-RHI | Direct3D 11, Direct3D 12, Vulkan 1.3 | Für diese Auswahl kein fertiges .NET-Paket verifiziert; Interop einplanen | Interessant für Windows/Linux und explizite Renderer; kein dokumentiertes Metal-Backend |
| The Forge | Natives Rendering-Framework | Unter anderem Direct3D 12, Vulkan und Metal | Für diese Auswahl kein fertiges .NET-Paket verifiziert; native Brücke einplanen | Eher für einen größeren nativen Renderer als für einen möglichst kleinen C#-Unterbau |

### SDL3 GPU

SDL3 GPU ist vom 2D-Renderer `SDL_Renderer` zu unterscheiden. Die GPU-API bietet Grafik- und Compute-Funktionen mit Command Buffern, Renderpässen, Pipelines und Ressourcen. Die dokumentierten Treibernamen für die Geräteerzeugung sind `vulkan`, `direct3d12` und `metal`.

Für Sasogine ist dies eine naheliegende Ergänzung: Eine gemeinsame GPU-API und SDL-Fensterintegration lassen sich hinter der kleinen Engine-Schnittstelle bündeln. Shaderformate und Ressourcen-Uploads bleiben bewusst zu planen; „einheitliche API“ bedeutet nicht „alle Shader unverändert auf jeder Plattform“.

Als C#-Ausgangspunkt kommt `ppy.SDL3-CS` infrage. Paketversion, native SDL-Version und benötigte GPU-Funktionen müssen zusammenpassen.

Quellen: [SDL3 GPU](https://wiki.libsdl.org/SDL3/CategoryGPU), [SDL_CreateGPUDevice](https://wiki.libsdl.org/SDL3/SDL_CreateGPUDevice), [SDL3-CS](https://github.com/ppy/SDL3-CS).

### bgfx

bgfx ist ausdrücklich als Renderingbibliothek zur Integration in eine eigene Engine ausgelegt. Die offizielle Übersicht nennt zahlreiche Backends und verlinkt C#-Bindings.

Die eigene API arbeitet unter anderem mit Views, Zuständen, Ressourcen-Handles und Draw-Einreichungen. Für Sasogine wäre deshalb zu prüfen, wie `Clear`, `DrawMesh` und `Present` auf dieses Modell abgebildet werden. Bei eigener Befehlsreihenfolge, Threading und Shader-Builds sollte der Prototyp die tatsächlichen Anforderungen abdecken.

Einschätzung: attraktiv, wenn Sasogine viele Plattformen erreichen soll und sich an das Rendermodell der Bibliothek anpassen kann. Native Bibliothek und C#-Binding sollten auf einen kompatiblen Stand festgelegt werden.

Quelle: [bgfx-Übersicht und Sprachbindungen](https://bkaradzic.github.io/bgfx/overview.html).

### Diligent Engine / DiligentCore

Diligent trennt den Kern der Grafikabstraktion von Tools, Effekten und Beispielen. Für Sasogine wäre zunächst DiligentCore zu evaluieren. Das Core-Projekt dokumentiert einen .NET-/NuGet-Build; die Gesamtprojektseite führt die unterstützten Grafik-APIs auf.

Die breite native RHI kann sinnvoll sein, wenn Sasogine später mehr als einen einfachen Mesh-Renderer benötigt. Zu prüfen sind die konkret verfügbaren nativen Module, ihre Bereitstellung pro Zielsystem und die API-Abdeckung der .NET-Schicht. Insbesondere Metal nicht allein aus einer allgemeinen Plattformliste als bereits ausgelieferte C#-Paketfunktion ableiten.

Quellen: [Diligent Engine](https://github.com/DiligentGraphics/DiligentEngine), [DiligentCore mit .NET-Build](https://github.com/DiligentGraphics/DiligentCore).

### LLGL

LLGL bietet eine gemeinsame Low-Level-API und enthält neben C++ auch einen C#-Wrapper. Die Hauptprojektseite führt Direct3D 11/12, Vulkan, OpenGL und Metal in ihrer Plattformmatrix auf und kennzeichnet den Projektstand als Beta.

Für Sasogine ist LLGL damit eine konkrete Alternative zu einer selbst geschriebenen Mehrbackend-Abstraktion. Vor einer Auswahl sollte ein C#-Prototyp Geräteerzeugung, Swapchain, Ressourcen-Upload und einen vollständigen Mesh-Frame nachweisen. Die Wartung und Vollständigkeit des Wrappers ist dabei gesondert zu bewerten.

Quelle: [LLGL-Hauptprojekt](https://github.com/LukasBanana/LLGL).

### sokol_gfx

`sokol_gfx.h` bietet eine kleine Grafikabstraktion mit Buffern, Bildern, Shadern, Pipelines und Renderpässen. Fenstererzeugung und Kontextinitialisierung gehören nicht zum Grafikheader selbst; dafür gibt es unter anderem das separate `sokol_app.h`. Vulkan wird im aktuellen Hauptprojekt als experimentell geführt.

Dieser Ansatz passt konzeptionell gut zu einem schlanken Sasogine-Backend. Die konkrete Backend-Unterstützung eines C#-Bindings kann jedoch hinter dem nativen Projekt liegen. Beispielsweise Sokol.NET daher als eigenständige Abhängigkeit prüfen. `sokol-shdc` ist die zugehörige Option für den Shader-Build.

Quellen: [Sokol](https://github.com/floooh/sokol), [Sokol.NET](https://github.com/elix22/Sokol.NET).

### Vortice.Windows

Vortice.Windows stellt .NET-Bindings unter anderem für Direct3D 11, Direct3D 12 und DXGI bereit. Es ersetzt damit für einen Windows-Backendpfad die Binding-Schicht, bildet aber keine gemeinsame plattformübergreifende Grafik-API.

Architektonische Option für Sasogine: zuerst ein eigenes Direct3D-11-Backend bauen und weitere Plattformen bei Bedarf hinter derselben Engine-Schnittstelle ergänzen. Direct3D 12 ist eine separate Implementierungsentscheidung mit entsprechendem Aufwand für Synchronisation und Ressourcenverwaltung.

Quelle: [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows).

### OpenTK

OpenTK bietet C#-Zugriff auf OpenGL sowie zusätzliche Fenster-, Eingabe- und Mathematikfunktionen. Ein eigener OpenGL-Renderer kann damit überschaubar beginnen.

Für Sasogine ist dies insbesondere eine Option für einen ersten Renderer oder einen bewusst auf OpenGL zugeschnittenen Funktionsumfang. Die Bibliothek ersetzt keine gemeinsame RHI über Vulkan, Direct3D 12 und Metal. macOS-Geräteabdeckung und gewünschte OpenGL-Funktionen vor einer langfristigen Festlegung gezielt testen.

Quelle: [OpenTK](https://github.com/opentk/opentk).

### Dawn

Dawn ist eine weitere native WebGPU-Implementierung und damit eine Alternative zu wgpu innerhalb desselben grundlegenden API-Ansatzes. Es sollte nicht als vollständig neues Engine-Architekturmodell gezählt werden.

Für Sasogine hängt die Entscheidung zwischen Dawn und wgpu insbesondere von passenden .NET-Bindings, nativer Auslieferung, benötigten Erweiterungen und Diagnosemöglichkeiten ab. Browser-WebGPU und native Nutzung sind getrennt zu prüfen.

Quelle: [Dawn-Hauptprojekt](https://dawn.googlesource.com/dawn).

### NVRHI

NVRHI abstrahiert Direct3D 11, Direct3D 12 und Vulkan 1.3 für Windows und Linux. Die Projektbeschreibung nennt unter anderem optionale automatische Ressourcenstatusverwaltung, verzögerte Ressourcenfreigabe und mehrere Queues.

Für Sasogine ist der hauptsächliche zusätzliche Aufwand die native C++-Integration. Ein Metal-Backend gehört nicht zur dokumentierten Auswahl. Der Name NVIDIA allein ist kein sachlicher Grund, die Bibliothek als ausschließlich für NVIDIA-GPUs geeignet einzustufen; entscheidend sind die Funktionen des gewählten Backends und Geräts.

Quelle: [NVRHI](https://github.com/NVIDIA-RTX/NVRHI).

### The Forge

The Forge ist ein umfangreicheres natives Rendering-Framework mit mehreren Grafik-APIs und einer eigenen Shader-Werkzeugkette. Das bisherige GitHub-Projekt verweist für die Fortsetzung ab Version 1.64 auf Codeberg.

Für Sasogines kleine C#-Schnittstelle wäre der Einstieg mit zusätzlicher nativer Integration verbunden. Dieser Kandidat lohnt sich vor allem dann, wenn der Umfang des Renderers deutlich wächst oder ohnehin ein nativer Renderingkern geplant ist. Die neue Projektquelle und der konkrete Veröffentlichungsstand müssen vor einer Integration geprüft werden.

Quelle: [The Forge mit Verweis auf die Projektfortsetzung](https://github.com/ConfettiFX/The-Forge).

### Welche Kandidaten zuerst prüfen?

Die folgende Priorisierung ist eine Ableitung für das Ziel `Clear`, `DrawMesh`, `Present`:

| Priorität / Ziel | Kandidaten | Entscheidende Frage im Prototyp |
| --- | --- | --- |
| Kleine portable GPU-Abstraktion | SDL3 GPU, sokol_gfx | Wie klein bleiben C#-Adapter, Shader-Build und native Auslieferung? |
| Gemeinsame RHI mit moderner Pipeline | Zenith.NET, LLGL, DiligentCore | Welche Funktionen sind über die konkrete C#-Version auf allen Zielplattformen verfügbar? |
| Viele Renderplattformen | bgfx | Passt das View-/Submission-Modell zum gewünschten Renderer? |
| Portabler WebGPU-Funktionsumfang | wgpu oder Dawn | Welche Binding-/Native-Kombination lässt sich zuverlässig ausliefern? |
| Eigener Windows-Renderer | Vortice.Windows | Reicht zunächst Direct3D 11, oder braucht Sasogine konkret Direct3D 12? |
| Eigene API-Abstraktion mit maximalem Einfluss | Silk.NET | Ist der Aufwand für mehrere native Implementierungen vorgesehen? |
| Einfacher OpenGL-Einstieg | OpenTK | Genügen OpenGL und die tatsächlich verfügbare Geräteabdeckung? |
| Größerer nativer Renderingkern | NVRHI, The Forge | Rechtfertigt der erwartete Funktionsumfang eine C++-/C#-Brücke? |

## Empfehlung für Sasogine 2.x

Nach Erweiterung der Auswahl bietet sich zuerst ein begrenzter Vergleich zwischen SDL3 GPU und Zenith.NET an. SDL3 GPU prüft den Weg über eine kleine portable GPU-API; Zenith.NET prüft eine gemeinsame moderne C#-RHI. Voraussetzung ist jeweils, dass die tatsächlich benötigten Geräte und Betriebssysteme unterstützt werden. Die hohe ursprüngliche Zenith.NET-Bewertung bleibt eine Arbeitshypothese, keine abschließende Freigabe.

Als weitere Vergleichskandidaten sind sokol_gfx für geringe Komplexität, bgfx für breite Backend-Abdeckung sowie LLGL und DiligentCore für eine umfassendere RHI sinnvoll. Für einen bewusst auf Windows begrenzten Einstieg ist Vortice.Windows gesondert zu betrachten.

Silk.NET bleibt die Option für eine vollständig selbst entwickelte API-Abstraktion. Veldrid ist zu prüfen, wenn Direct3D 11 genügt. WebGPU ist besonders interessant, wenn ein gemeinsamer portabler Funktionsumfang wichtiger ist als direkter Zugriff auf jede native API-Funktion.

Die endgültige Auswahl soll nach dem gleichen kleinen Testszenario für die relevanten Kandidaten erfolgen. Eine feste Backend-Wahl allein anhand der Punktwerte ist nicht vorgesehen.

## Vorgeschlagene Engine-Schnittstelle

Das folgende C#-Beispiel ist ein eigener Schnittstellenentwurf für Sasogine. Es ist kein Zenith.NET-, Silk.NET- oder Veldrid-API-Beispiel. Fensterintegration, Mesh-Uploads, Shader und Pipelines werden später ergänzt.

```csharp
using System;
using System.Numerics;

namespace Sasogine.Graphics;

public readonly record struct MeshHandle(int Id);
public readonly record struct PipelineHandle(int Id);

public interface IGraphicsBackend : IDisposable
{
    // true: Ein Frame wurde begonnen und kann beschrieben werden.
    // false: Aktuell kein renderbares Bild, etwa bei minimiertem Fenster.
    bool TryBeginFrame();

    void Clear(Vector4 color, float depth = 1.0f);

    void DrawMesh(
        MeshHandle mesh,
        PipelineHandle pipeline,
        in Matrix4x4 transform);

    void Present();
    void Resize(uint width, uint height);
}
```

`TryBeginFrame` macht den Beginn eines Frames ausdrücklich sichtbar. Damit muss `Clear` keine versteckte Bildakquisition oder zusätzliche GPU-Einreichung auslösen. `Present` schließt die gesammelten Befehle des Frames ab und veranlasst Einreichung und Präsentation.

Beispiel für die Verwendung innerhalb eines bereits initialisierten Renderers:

```csharp
if (backend.TryBeginFrame())
{
    backend.Clear(new Vector4(0.08f, 0.10f, 0.14f, 1.0f));
    backend.DrawMesh(mesh, pipeline, in worldTransform);
    backend.Present();
}
```

Vertrag des Entwurfs:

- `Clear` und `DrawMesh` sind nur innerhalb eines begonnenen Frames gültig.
- `Present` beendet genau diesen Frame.
- `Resize` wird zwischen Frames verarbeitet; bei Größe null wird das Rendering ausgesetzt.
- Handles dürfen nur auf gültige Ressourcen des jeweiligen Backends verweisen.
- Ressourcen werden erst wiederverwendet oder freigegeben, wenn ihre GPU-Nutzung abgeschlossen ist.

## Vergleich des internen Frame-Ablaufs

Beide Abläufe sind konzeptionell. Sie zeigen Verantwortlichkeiten und sind kein ausführbarer Bibliothekscode.

### Gemeinsame RHI, beispielsweise Zenith.NET

```text
Frame-Ressourcen nach GPU-Abschluss wiederverwenden
→ Swapchain-Bild beziehen
→ Befehlsaufzeichnung beginnen
→ erforderliche Barrieren und Texturübergänge aufzeichnen
→ Renderziel löschen und Meshes zeichnen
→ Übergang für Präsentation vorbereiten
→ Befehle abschließen und mit Synchronisation einreichen
→ Bild präsentieren
```

### Eigene Vulkan-Implementierung über Silk.NET

```text
Abschluss der vorherigen Nutzung der Frame-Ressourcen prüfen
→ Swapchain-Bild akquirieren und Ergebnis behandeln
→ Command Buffer nach sicherer Wiederverwendung zurücksetzen
→ Aufzeichnung beginnen und Bildzustand vorbereiten
→ Render Pass oder Dynamic Rendering beginnen
→ Clear und Draw-Befehle aufzeichnen
→ Rendering beenden und Präsentationszustand herstellen
→ Command Buffer abschließen
→ mit passenden Warte-/Signaloperationen an Queue übergeben
→ Präsentation auf Renderabschluss warten lassen
→ Resize- und Swapchain-Ergebnisse behandeln
```

Die konkrete Vulkan-Synchronisation richtet sich nach den verwendeten Queues, Ressourcen und aktivierten Funktionen. Maßgeblich ist die [offizielle Vulkan-Spezifikation](https://registry.khronos.org/vulkan/specs/latest/html/vkspec.html).

## Korrekturen an den ursprünglichen Codebeispielen

Die beiden ursprünglichen Klassen sind als Ausgangsskizzen unvollständig und werden durch den obigen Schnittstellenentwurf und die Ablaufbeschreibungen ersetzt:

1. **Zenith.NET:** Die verwendeten Namespaces, Typen und Methoden wurden nicht gegen eine konkrete Paketversion nachgewiesen. Sie dürfen deshalb nicht als fertige Implementierung übernommen werden. Auch Ressourcenfreigabe, Bildzustände und Synchronisation fehlen.
2. **Vulkan-Einreichung:** Der gezeigte `SubmitInfo` verweist auf keinen aufgezeichneten Command Buffer. Damit wird die beschriebene Renderarbeit nicht eingereicht.
3. **Vulkan-Präsentation:** Swapchain, Bildindex und Synchronisation sind nicht gesetzt; ein tatsächlicher Präsentationsaufruf fehlt.
4. **Frame-Wiederverwendung:** Ein Command Buffer darf nicht zurückgesetzt werden, während die GPU ihn noch nutzt.
5. **Auflösung:** Die Renderfläche muss zur tatsächlichen Zielgröße passen; eine feste Beispielauflösung genügt nicht für Resize.
6. **Rendering-Modell:** Ein klassischer Render Pass ist nicht der einzige Vulkan-Weg; Dynamic Rendering ist abhängig von Version und Funktionen ebenfalls möglich.
7. **Freigabe:** Entscheidend sind abgeschlossene GPU-Zugriffe und Objektabhängigkeiten. Nur das Gerät zu zerstören reicht als vollständige Freigabestrategie nicht aus.

Vulkan-Regeln zu Einreichung, Lebenszeiten und Rendering: [offizielle Vulkan-Spezifikation](https://registry.khronos.org/vulkan/specs/latest/html/vkspec.html). Die Beispiele wurden nicht kompiliert oder auf einer GPU ausgeführt.

## Prototyp und Entscheidungskriterien

Für jeden ernsthaft betrachteten Kandidaten denselben Umfang umsetzen:

1. Fenster, Gerät und Swapchain initialisieren; gewähltes Backend protokollieren.
2. Einen stabilen Clear-/Present-Zyklus ausführen.
3. Ein indiziertes Mesh mit Transformation und Tiefentest zeichnen.
4. Größenänderungen, Minimieren und Wiederherstellen behandeln.
5. Mehrere Frames ohne vorzeitige Wiederverwendung von GPU-Ressourcen verarbeiten.
6. Ressourcen erzeugen und freigeben; verfügbare Validierung aktivieren.
7. Auf den tatsächlich vorgesehenen Windows-, Linux- und macOS-Systemen prüfen.

Danach Integrationsaufwand, Diagnosemöglichkeiten, Shader-Workflow, Geräteabdeckung und gemessene CPU-/GPU-Zeiten dokumentieren. Erst diese Ergebnisse begründen die endgültige Backend-Entscheidung für Sasogine 2.x.
