<img width="636" height="141" alt="logo" src="https://github.com/user-attachments/assets/27d1abe7-695c-4c6c-a296-23fded8f0eb7" />

## 💻 .NET Game Development Framework

Xirface is a Vulkan-based lightweight .NET game development framework, built on Silk.NET, featuring mesh-based rendering, GLSL shader support, mesh-based custom text rendering, a JSON driven UI system.

## ⚠️  [0.1.0] THIS VERSION ONLY FEATURES RENDERING AND INPUT
```csharp
dotnet add package Xirface --version 0.1.0
```
-----
```csharp
protected override void Load()     
```
```csharp
protected override void Update(double delta)
```
```csharp
protected override void Draw(double delta, CommandBuffer cmd, uint imageIndex)
```
-----
### 📂 AssetManager
Code sample on how to load Shader (.spv)
```csharp
AssetManager.Load<Shader>("PATH_TO_FILE.vert.spv", "PATH_TO_FILE.frag.spv", typeof(VertexPositionColor));        
```
Code sample on how to load Textures2D
```csharp
AssetManager.Load<Texture2D>("PATH_TO_FILE.png");
```
### 🔴 ClearColor
Code sample on how to set the color of ClearColor
```csharp
ClearColor = new ClearColorValue(1, 1, 1, 1);
```
