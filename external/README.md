# OpenGL 底层实现对照区

本目录用于 LearnOpenGL 学习中的底层实现扩展，不是主教材。

## 目录

- `PortableGL/`：教学友好的 OpenGL-like 软件实现。优先用于理解 VAO/VBO、shader-like 函数、draw call、光栅化、片段处理、深度缓冲和颜色缓冲如何串起来。
- `mesa/`：Mesa 源码的 sparse checkout。重点对照 Linux 主流开源 OpenGL 实现中的 Mesa/Gallium/softpipe 路径。

## 使用顺序

```text
LearnOpenGL docs + demo-code
  -> PortableGL 简化实现
  -> Mesa softpipe 真实实现位置
```

学习时不要直接从 Mesa 全量源码开始。Mesa 是真实工程，层次多、历史包袱多，适合作为“真实实现在哪里”的对照，不适合作为第一解释材料。

## PortableGL 重点入口

- `PortableGL/portablegl.h`
- `PortableGL/examples/`
- `PortableGL/demos/`
- `../tools/portablegl-smoke/portablegl_smoke.c`：本仓库的最小验证程序，在内存 framebuffer 中绘制红色三角形并检查像素。

看 PortableGL 时优先找：

- buffer / vertex array / attribute 如何保存和解释。
- draw call 如何进入顶点处理。
- vertex shader-like 和 fragment shader-like 函数如何被调用。
- triangle 如何被裁剪、光栅化、生成 fragment。
- depth test、blend、framebuffer 如何在软件中发生。

## Mesa softpipe 重点入口

- `mesa/src/mesa/glapi/`：OpenGL API dispatch 相关。
- `mesa/src/mesa/main/`：Mesa 的 OpenGL 状态和对象管理。
- `mesa/src/mesa/vbo/`：OpenGL 顶点数组、buffer、draw 路径相关。
- `mesa/src/mesa/state_tracker/`：OpenGL 状态到 Gallium 状态的桥接层。
- `mesa/src/gallium/auxiliary/draw/`：Gallium 辅助 draw 模块。
- `mesa/src/gallium/drivers/softpipe/`：软件光栅化驱动，对照真实软件渲染路径。
- `mesa/src/compiler/`：shader 编译相关，包括 NIR 等中间表示。

## 学习边界

每个 LearnOpenGL 章节只看当前概念对应的实现路径：

- Hello Triangle：顶点数据、attribute、draw。
- Shaders：shader-like 调用、GLSL/NIR 编译路径概念。
- Textures：texture storage、sampling、sampler state。
- Depth Testing：depth buffer 和 depth test。
- Blending：fragment 输出和 blend state。
- Framebuffers：color/depth attachment 和渲染目标。

不要在入门阶段深入：

- 具体 GPU 指令生成。
- 窗口系统和 DRM/KMS 细节。
- 完整 Mesa 构建系统。
- 所有 Gallium driver 的差异。
- 高性能优化路径。

## 本地工具链

已安装便携 MinGW-w64/GCC 工具链：

- `../tools/w64devkit/w64devkit/`
- GCC：`../tools/w64devkit/w64devkit/bin/gcc.exe`
- 下载包：`../tools/winlibs-mingw/w64devkit-x64-2.8.0.7z.exe`

使用时不需要改系统 PATH，可以在当前 PowerShell 会话临时加入：

```powershell
$env:PATH = (Resolve-Path 'tools\w64devkit\w64devkit\bin').Path + ';' + $env:PATH
```

PortableGL 最小验证命令：

```powershell
$env:PATH = (Resolve-Path 'tools\w64devkit\w64devkit\bin').Path + ';' + $env:PATH
gcc -std=c99 -O2 -ffp-contract=off -w -o tools\portablegl-smoke\portablegl_smoke.exe tools\portablegl-smoke\portablegl_smoke.c -lm
tools\portablegl-smoke\portablegl_smoke.exe
```

期望输出类似：

```text
red_pixels=968
```

这说明 PortableGL 已经在内存 framebuffer 中完成了一次最小三角形绘制。
