# SwirlChart

A modern cross-platform (.NET 10 MAUI) engineering utility for calculating combustion gas swirl angles and diagnosing exhaust temperature spreads on **GE Frame 9E (MS9001E)** heavy-duty gas turbines.

---

## What This App Is

**SwirlChart** is a desktop and mobile application designed for field engineers, operators, and commissioning technicians working on GE Frame 9E gas turbines. It replaces the legacy desktop utility with a modern, high-DPI, touch-friendly interface running on Windows and Android.

---

## What It Does

1. **Calculates Swirl Angle:** Computes the hot gas swirl angle ($\theta$, in degrees) dynamically from generator active load ($0$ to $120\text{ MW}$) using the exact 6-segment piecewise linear interpolation model:
   * $0 \text{ to } 17\text{ MW}: 160.0^\circ$ (FSNL plateau)
   * $17 \text{ to } 41\text{ MW}: 160.0^\circ \to 143.0^\circ$ ($-0.7083^\circ/\text{MW}$)
   * $41 \text{ to } 57\text{ MW}: 143.0^\circ \to 84.0^\circ$ ($-3.6875^\circ/\text{MW}$)
   * $57 \text{ to } 80\text{ MW}: 84.0^\circ \to 68.0^\circ$ ($-0.6957^\circ/\text{MW}$)
   * $80 \text{ to } 114\text{ MW}: 68.0^\circ \to 0.0^\circ$ ($-2.0000^\circ/\text{MW}$)
   * $114 \text{ to } 120\text{ MW}: 0.0^\circ$ (Base load, axial flow)

2. **Interactive Circular Visualizer:** Renders a 2D circular cross-section of the Frame 9E turbine looking in the direction of flow:
   * **14 Combustor Cans** arranged in a fixed outer circle.
   * **24 Exhaust Thermocouples** whose indicator spokes rotate dynamically in real time according to the calculated swirl angle.
   * Enables immediate visual back-tracing from an anomalous thermocouple reading (hot/cold spot) to the offending combustion chamber can.

3. **2D Curve Plotter (Chart):** Displays a 2D Cartesian graph plotting Swirl Angle ($^\circ$) versus Active Power (MW).

4. **Built-in Engineering Reference:** Includes technical guides on:
   * Statistical baseline variations at base load ($\pm 41^\circ\text{F}$ normal average spread).
   * Root-cause troubleshooting for cold spots (fuel blockage, leaking check valves) and hot spots (nozzle wear, liner damage).
   * Step-by-step spread tracing procedures and operational limits ($125^\circ\text{F}$ allowable spread).

---

## Based On Whose Work

* **Original Tool:** Based on **Active Swirl v1.2 (August 2000)**, developed in Delphi 2 by **Nathan Spence** (`nathan.spence@ge.com`) at General Electric Contractual Services.
* **Engineering Methodology:** Derived from the technical paper *"Exhaust Spreads and Troubleshooting"* (June 7, 2000) by GE engineer **Roointon Pavri**.

---

## Why It Was Created

The original `ActSwirl2.exe` utility was compiled in 2000 for legacy 32-bit Windows and was only distributed as an old desktop executable. 

**SwirlChart** was created to:
* Bring this critical diagnostic capability to modern mobile devices (**Android** phones and tablets) so field personnel can perform spread back-tracing directly on site at the turbine deck without carrying a PC.
* Provide clean, responsive, high-DPI graphics and touch-friendly controls.
* Package the calculation, visualizer, and reference documentation into a single offline, portable tool.

---

## Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download) (version `10.0.100` or later)
* Workloads: `maui-windows` and `android`
  ```bash
  dotnet workload install maui-windows android
  ```

---

## How to Build and Run

All commands should be executed from the project root directory (`K:\Erwin-coder\SwirlChart`):

### Windows

#### 1. Build:
```powershell
dotnet build -f net10.0-windows10.0.19041.0 -c Debug SwirlChart\SwirlChart.csproj
```

#### 2. Run:
```powershell
dotnet run -f net10.0-windows10.0.19041.0 -c Debug --project SwirlChart\SwirlChart.csproj
```

---

### Android

#### 1. Build (Debug APK):
```powershell
dotnet build -f net10.0-android -c Debug SwirlChart\SwirlChart.csproj
```
The output APK is generated at:
`SwirlChart\bin\Debug\net10.0-android\SwirlChart.SwirlChart-Signed.apk`

#### 2. Run on Connected Device / Emulator:
Ensure your Android device has USB Debugging enabled (or an emulator is running):
```powershell
dotnet build -t:Run -f net10.0-android SwirlChart\SwirlChart.csproj
```

#### 3. Build Optimized Release APK (arm64, AOT-compiled & trimmed):
```powershell
dotnet publish -f net10.0-android -c Release SwirlChart\SwirlChart.csproj
```
The optimized production APK is generated at:
`SwirlChart\bin\Release\net10.0-android\android-arm64\SwirlChart.SwirlChart-Signed.apk`
