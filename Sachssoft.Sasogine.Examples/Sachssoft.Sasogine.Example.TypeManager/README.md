# Beispielprojekt: AOT-freundlicher Type-Manager

von **Sasolib** für **Sasogine**

## Überblick

Dieses Repository zeigt ein **Beispielprojekt** für einen AOT-freundlichen Type-Manager in **C#**.

Der Manager wurde für Plattformen entwickelt, die nur **Ahead-Of-Time (AOT)**-Kompilierung unterstützen, z. B.:

* **iOS**
* **WebGL**
* andere AOT-Umgebungen

Da **Reflection** auf diesen Plattformen eingeschränkt oder nicht verfügbar ist, verwaltet der Type-Manager alle Typinformationen **ohne Reflection-Aufrufe**.

## Ziele

* Demonstration einer **AOT-kompatiblen Lösung**
* Bereitstellung einer **einfachen Type-Registry**
* **Kein Reflection** zur Laufzeit
* Gut verständlicher, leicht anpassbarer Beispielcode

## Vorteile

* Vollständig **AOT-kompatibel**
* **Einfach erweiterbar** für eigene Klassen, Komponenten oder Spielobjekte
* Ideal als **Grundlage** für AOT-Plattformen wie **Sasogine**

## Installation & Nutzung

1. Repository klonen oder als ZIP herunterladen
2. Projekt in **Visual Studio** oder **Rider** öffnen
3. Kompilieren & ausführen

## Lizenz

MIT-Lizenz – frei einsetzbar für Tests, Beispiele und eigene Erweiterungen.
