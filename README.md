# RG2 - Computer Graphics 2 - Homework Assignments

This repository contains solutions for the homework assignments from the Computer Graphics 2 course at the School of Electrical Engineering, University of Belgrade. The projects cover various aspects of computer graphics, ranging from low-level OpenGL programming to development within the Unity3D game engine.

---

## Project 1: Mandelbrot Set Viewer

An interactive viewer for the Mandelbrot set implemented using the OpenGL 4+ graphics library. All calculations and rendering are performed entirely on the GPU via GLSL shaders, enabling smooth zooming and panning through the complex plane.

### Key Features:
*   **Interactive Visualization:** Zooming (`Scroll wheel`) and panning (`Mouse drag`) through the Mandelbrot set.
*   **GPU Acceleration:** Full computation and coloring within the fragment shader for high performance.
*   **Adjustable Iterations:** Ability to change the maximum number of iterations (`+/-` keys) for a more detailed view.
*   **HSV Coloring:** Visual representation of the "escape speed" of points from the set using beautiful gradients.
*   **Black Color** for points belonging to the Mandelbrot set.

### Technologies Used:
*   Java
*   LWJGL 3.3.3 (Lightweight Java Game Library)
*   OpenGL 4.0+
*   GLSL (OpenGL Shading Language)
*   Gradle
*   IntelliJ IDEA

### Challenges and Lessons Learned:
The primary challenge during implementation was a persistent compiler issue within IntelliJ IDEA, which refused to recognize Java 8+ language features (such as lambda expressions for `MemoryStack.use()`) despite correct JDK 23 and language level 21 configuration in both Gradle and IntelliJ settings. The solution was eventually found by modifying the code to use manual memory management (`MemoryUtil.memAllocDouble()` and `MemoryUtil.memFree()`) within `try-finally` blocks, effectively bypassing the compiler's stubborn behavior.

---

## Project 2: Marching Cubes Algorithm

An implementation of the Marching Cubes algorithm within the Unity3D environment. This project focuses on generating 3D meshes from implicit functions (voxel data), commonly used for creating terrain, blob objects, or other procedural geometries.

### Key Features:
*   **Procedural Generation:** Utilizes the Marching Cubes algorithm to generate 3D geometry from volumetric (voxel) data.
*   **Unity3D Integration:** Full implementation and visualization within the Unity game engine.
*   *(To be completed with specific details of Assignment 2 once implemented, e.g., "Terrain generation with Perlin noise," "Collision detection," "Interactive voxel modification," etc.)*

### Technologies Used:
*   Unity3D (Game Engine)
*   C# (Programming Language)
*   JetBrains Rider (IDE)
*   .NET SDK

---

## Getting Started

### Cloning the Repository:
```bash
git clone https://github.com/Vanjy/[your-repo-name].git
cd [your-repo-name]