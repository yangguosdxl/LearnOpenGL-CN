# VS Code 调试指南

本项目的三个代码目录均可通过 VS Code 图形化调试。

**关于路径。** 当前项目路径为 `G:\Home\MySyncthing\MyCloudData\tmp\LearnOpenGL-CN`，不含中文字符，w64devkit 的 GDB 可正常识别，**无需再用 `subst P:` 映射驱动器**。`.vscode/` 下的配置已全部改用 `${workspaceFolder}` 相对路径，不依赖任何映射盘。

> 仅当项目被移动到含中文字符的路径、导致 GDB 报路径错误时，才需要用 `subst` 把根目录映射为纯英文盘符（如 `subst P: "<项目路径>"`），并相应修改配置中的路径。正常情况下忽略本提示。

## 前置要求

- **VS Code** 已安装
- **VS Code 扩展**：
  - `ms-vscode.cpptools` — C/C++ 调试
  - `ms-dotnettools.csharp` — C# 调试 demo-code-cs

## portablegl 调试（GDB）

使用 w64devkit 自带的 GDB。

### 编译 debug 版本

终端是 git-bash。w64devkit 的 gcc **必须先把自己的 bin 目录加入 `PATH`**，否则会报 `cannot execute 'as'`（找不到汇编器），或产出不含 C 源码调试信息的 exe，导致断点无法命中。

```bash
# 在项目根目录下执行
export PATH="$PWD/tools/w64devkit/w64devkit/bin:$PATH"
gcc -std=c99 -g -O0 -ffp-contract=off -w -o xxx.exe xxx.c -lm
```

关键参数：`-g` 生成调试信息，`-O0` 关闭优化。

> VS Code 的编译任务（`tasks.json`）已通过 `options.env.PATH` 自动注入该 bin 目录，无需手动设置。

### VS Code 启动调试

1. 用 VS Code 打开项目根目录 `LearnOpenGL-CN`
2. 侧边栏 **Run and Debug** (Ctrl+Shift+D)
3. 选择配置：
   - **"GDB: portablegl-smoke"** — F5 会先经 `preLaunchTask` 自动重新编译出带符号的 exe，再调试 smoke 测试
   - **"GDB: portablegl-labs (当前文件)"** — 先打开要调试的 `.c` 文件，F5 自动编译并调试

### 手动 GDB

```bash
export PATH="$PWD/tools/w64devkit/w64devkit/bin:$PATH"
gcc -std=c99 -g -O0 -ffp-contract=off -w -o xxx.exe xxx.c -lm
gdb ./xxx.exe
```

## C++ demo-code 调试

已通过 CMake + VS2022 编译出 Debug 版本，exe 位于 `demo-code/bin/<章节>/Debug/`。

### VS Code 启动调试

1. 选择 **"C++ demo-code (选择 exe)"**
2. 下拉选择章节目录（如 `1.getting_started`）
3. 输入 exe 名称（如 `1.getting_started__1.1.hello_window.exe`）
4. F5 启动

### 使用 Visual Studio 调试

直接打开解决方案：

```
demo-code/out/LearnOpenGL.sln
```

VS2022 支持 **Graphics Debugging**（Debug > Graphics > Start Graphics Debugging），可逐帧查看 OpenGL draw call、shader 变量、纹理和帧缓冲。

## C# demo-code-cs 调试

### 命令行运行

```powershell
cd demo-code-cs
dotnet run --project LearnOpenGL.OpenTK.csproj -- <demo名>
dotnet run --project LearnOpenGL.OpenTK.csproj -- --list
dotnet run --project LearnOpenGL.OpenTK.csproj -- 2.lighting/6.multiple_lights --frames 3 --capture verification/manual.png
```

### VS Code 调试

1. 用 VS Code 打开 `demo-code-cs` 文件夹
2. 安装 C# Dev Kit 扩展
3. F5 启动调试

## RenderDoc（图形帧调试）

已安装：`C:\Program Files\RenderDoc\qrenderdoc.exe`

适合所有 OpenGL 程序的渲染问题排查：

1. 打开 RenderDoc
2. File > Launch Executable，选择要调试的 exe
3. Working Dir 设为 exe 所在目录
4. 抓帧后查看：draw call、顶点数据、纹理、shader 变量值

## 配置文件

- `.vscode/launch.json` — 3 个调试预设，所有路径使用 `${workspaceFolder}` 相对路径；smoke 预设带 `preLaunchTask` 自动重新编译
- `.vscode/tasks.json` — gcc 编译任务（`gcc-build-current` 编译当前文件、`gcc-build-smoke` 编译 smoke），均通过 `options.env.PATH` 注入 w64devkit/bin
- `.vscode/settings.json` — C/C++ IntelliSense 配置

## 常见问题

**Q: 调试后未命中断点（最常见）**
A: 多半是 exe 没有 C 源码的调试信息。用以下命令自查：
```bash
tools/w64devkit/w64devkit/bin/gdb.exe --batch -ex "info line main" xxx.exe
```
若提示 `No line number information available for address <main>`，说明 exe 是无符号编译的。根因通常是编译时 gcc 没加入 PATH 报 `cannot execute 'as'`，或用了优化/缺 `-g`。重新用上文“编译 debug 版本”的命令编译即可。VS Code 里直接用带 `preLaunchTask` 的 "GDB: portablegl-smoke" 预设最稳妥。

**Q: portablegl 编译报 `cannot execute 'as'`**
A: gcc 找不到同目录的汇编器 `as.exe`。把 w64devkit 的 bin 加入 PATH：`export PATH="$PWD/tools/w64devkit/w64devkit/bin:$PATH"`。VS Code 任务已自动注入，无需手动设置。

**Q: VS Code 提示找不到 GDB**
A: 确认 `tools/w64devkit/w64devkit/bin/gdb.exe` 存在且可访问。

**Q: GDB 报路径错误 / 无法切换目录**
A: 仅当项目被移到含中文字符的路径时才会发生。用 `subst` 将根目录映射为纯英文盘符，并相应修改 `.vscode/` 配置中的路径。

**Q: demo-code-cs 的 SDK 版本不匹配**
A: `global.json` 已设为 `9.0.313`，与本机 dotnet SDK 匹配。

