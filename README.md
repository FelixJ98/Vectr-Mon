# Vectr-Mon

**Scan QR codes to summon Vector-based monsters and battle in a shared mixed reality experience!**

> **Created for INIT Build 2025**

## Overview

Vectr-Mon is a multiplayer mixed reality experience developed during **INIT Build 2025** for the **Meta Quest** platform using **Unity**. The project explores how multiple VR headsets can share the same physical space through **colocation**, allowing players to see and interact with the same virtual objects in real time.

The core idea behind Vectr-Mon is combining **shared mixed reality**, **real-time networking**, and **QR code scanning** into a unique multiplayer experience. Players scan physical QR codes using their Meta Quest headsets, which Unity interprets to summon unique Vector-based monsters into the shared environment. Once summoned, players battle using a simple strategy system inspired by Pokémon, featuring **Attack**, **Defend**, and **Grab** mechanics.

---

## Features

* **Shared Mixed Reality**

  * Two Meta Quest headsets connect to the same multiplayer session.
  * Colocation ensures both players experience the same same world with models and entities in the same physical location for both players at once.

* **Real-Time Networking**

  * Multiplayer synchronization powered through Unity Networking.
  * Monster spawning, player interactions, and gameplay remain synchronized across both headsets across the same physical space.

* **QR Code Monster Summoning**

  * Uses the Meta Quest's built-in QR code scanning capabilities.
  * Each QR code spawns a different Vector-based monster directly into the shared world depending on player input using hand tracking.

* **Battle System**

  * Simple strategy gameplay inspired by rock-paper-scissors mechanics.
  * Players choose between:

    * **Attack**
    * **Defend**
    * **Grab**

---

## Technical Highlights

The primary objective of Vectr-Mon was demonstrating how multiple emerging XR technologies could work together in a seamless multiplayer experience.

### Networking & Colocation

The largest technical focus of this project was **networking**.

Using **Unity Networking Services**, both Meta Quest headsets connect to a shared multiplayer LAN session where every spawned monster, player action, and interaction is synchronized in real time and tied to the same physical space.

### Meta Quest Development

* Mixed Reality application development
* QR code scanning using Meta Quest hardware and experimental features from the SDK
* Shared spatial alignment through colocation

### Unity Integration

* object spawning from scanned QR codes
* Multiplayer gameplay systems
* Shared world synchronization
* C# gameplay scripting

---

## Technologies Used

* Unity
* Unity Networking Services
* Meta Quest SDK
* Colocation
* QR Code Scanning
* C#

---

## Future Improvements

* Additional monsters
* Expanded battle mechanics
* Improved animations, visual effects, and UI

---

## Project Goal

Vectr-Mon was created as part of **INIT Build 2025 (Fall)** to showcase the possibilities of combining **Meta Quest mixed reality**, **Unity Networking**, and **QR code interaction** into a collaborative multiplayer experience through the use of these very experimental features.
