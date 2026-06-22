# LearnOpenGL OpenTK 移植版

这个目录是 `demo-code` 的 C# / OpenTK 移植工作区。

## 运行

```powershell
dotnet run --project LearnOpenGL.OpenTK.csproj -- --list
dotnet run --project LearnOpenGL.OpenTK.csproj -- 1.getting_started/2.1.hello_triangle
```

自动退出并截图：

```powershell
dotnet run --project LearnOpenGL.OpenTK.csproj -- 2.lighting/6.multiple_lights --frames 3 --capture verification/manual.png
```

## 批量验证

```powershell
powershell -ExecutionPolicy Bypass -File .\Verify-Demos.ps1 -Frames 3 -OutputDir verification/screenshots-colorcheck
```

验证脚本会：

- 构建项目。
- 读取 `--list` 中已注册的 demo。
- 逐个运行 demo。
- 保存 PNG 截图。
- 检查截图文件大小和颜色范围，防止只验证到“进程退出成功”。
- 写出 `verification-results.csv`。

## 当前已迁移并验证

已迁移并逐个运行截图验证：

- `1.getting_started`：24 个示例。
- `2.lighting`：13 个 CMake 主线示例。
- `3.model_loading`：1 个示例，使用 AssimpNet 加载 backpack 模型。
- `4.advanced_opengl`：5 个示例：
  - `1.1.depth_testing`
  - `1.2.depth_testing_view`
  - `2.stencil_testing`
  - `3.1.blending_discard`
  - `3.2.blending_sort`

验证命令最后一次通过：

```powershell
powershell -ExecutionPolicy Bypass -File .\Verify-Demos.ps1 -Frames 3 -OutputDir verification/screenshots-colorcheck
```

结果清单：

```text
demo-code-cs/verification/screenshots-colorcheck/verification-results.csv
```

## 尚未迁移

后续章节仍需继续移植：

- `3.model_loading`
- `4.advanced_opengl` 剩余：framebuffers、cubemaps、UBO、geometry shader、instancing、anti-aliasing。
- `5.advanced_lighting`
- `6.pbr`
- `7.in_practice`
- `8.guest`

这些章节需要继续补充模型加载、帧缓冲、Cubemap、Instancing、AssimpNet、HDR/Bloom、Deferred Shading、SSAO、PBR/IBL 等共享能力。
