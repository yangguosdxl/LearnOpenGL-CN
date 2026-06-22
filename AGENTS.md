# AGENTS.md

## 项目背景

这个仓库用于 LearnOpenGL 深度学习。

- 教材内容在 `docs/`。
- 示例代码在 `demo-code/`。
- 跨会话学习计划和进度记录在 `docs/learnopengl-8h-study-plan.md`。

## 用户偏好

- 默认使用中文回复，除非用户明确要求使用其他语言。
- 默认使用中文编写文档、计划、学习笔记、总结和最终输出，除非用户明确要求使用其他语言。
- 用户希望流程直接、低摩擦。除非受沙箱、安全策略、信息缺失或高风险/破坏性操作限制，否则不要反复询问确认。
- 把当前任务视为“深度学习指导”，不是普通文档浏览。
- 继续 LearnOpenGL 学习时，先阅读 `docs/learnopengl-8h-study-plan.md`，再从其中的“当前进度”和“下一步”继续。

## LearnOpenGL 教学模式

把 `docs/` 当作教材，把 `demo-code/` 当作实验室。

学习目标不是按顺序读完每一行，而是建立强力的 OpenGL 心智模型：

1. OpenGL 状态机和渲染管线。
2. CPU 到 GPU 的数据流：VAO、VBO、EBO、Shader、Uniform、纹理和 Draw Call。
3. 变换、坐标系统、相机和 MVP 矩阵。
4. 光照模型、材质、光照贴图和多光源。
5. 使用 Assimp 加载模型，以及 Mesh、Model、纹理和绘制调用之间的关系。
6. 高级 OpenGL 状态与阶段：深度测试、模板测试、混合、帧缓冲、Cubemap 和 Instancing。
7. 高级光照与 PBR：Gamma Correction、Shadow Mapping、Normal Mapping、HDR、Bloom、Deferred Shading、SSAO、微表面理论、Fresnel 和 IBL。

讲解每个重要主题时，优先使用这个结构：

```text
本节解决什么问题：
核心概念：
关键 API / GLSL：
CPU 到 GPU 的数据流：
对应 demo-code：
我能如何修改它：
更深层原理：
检查问题：
```

## 续接协议

当新会话开始，并且用户要求继续 LearnOpenGL 学习时：

1. 读取 `docs/learnopengl-8h-study-plan.md`。
2. 找到当前进度和下一步动作。
3. 使用中文继续指导。
4. 优先基于本地文件做具体讲解，不要只给泛泛的 OpenGL 解释。
5. 当学习取得有意义进展时，更新 `docs/learnopengl-8h-study-plan.md`，尤其是完成一个学习小时或需要为下次会话留下新的续接点时。

## 在新环境重建项目

**给 AI 的指令**：当用户在新机器（Linux 或 Windows）上要求"重建这个项目环境"或类似请求时，**严格按本章操作**，不要自由发挥。本章是从零拉起这套学习仓库的唯一权威步骤。

### 1. 必备工具

| 工具 | 用途 | Windows | Linux |
|---|---|---|---|
| git | 主仓库 + submodule | `winget install Git.Git` | 发行版包管理器（`apt install git` / `pacman -S git` / 等） |
| GitHub CLI (`gh`) | fork、auth、PR | `winget install GitHub.cli` | 发行版包或参考 https://cli.github.com |
| .NET SDK 8.0+ | 跑 `demo-code-cs/` | `winget install Microsoft.DotNet.SDK.8` | 发行版 dotnet-sdk 包 |
| C/C++ 工具链 | 跑 `demo-code/`、PortableGL、w64devkit-src | 仓库已含 `tools/w64devkit/` 选项；或装 MSVC / MinGW | gcc/clang + make + cmake |

### 2. 克隆项目（含 submodule）

```bash
gh auth login                                                # 登录 GitHub
git clone --recurse-submodules https://github.com/yangguosdxl/LearnOpenGL-CN.git
cd LearnOpenGL-CN
```

如果已经 clone 但没拉 submodule：`git submodule update --init --recursive`

mesa 体积很大（几 GB），如果只想浅拉：`git -c submodule.external/mesa.update=none submodule update --init --recursive`，需要时再 `git submodule update --init external/mesa --depth 1`。

### 3. Submodule 一览

| 路径 | origin（fork URL） | upstream | 跟踪分支 |
|---|---|---|---|
| `demo-code` | `yangguosdxl/LearnOpenGL` | `JoeyDeVries/LearnOpenGL` | master |
| `external/PortableGL` | `yangguosdxl/PortableGL` | `rswinkle/PortableGL` | master |
| `external/mesa` | `gitlab.freedesktop.org/mesa/mesa`（无 fork） | 同左 | main |
| `tools/w64devkit-src` | `yangguosdxl/w64devkit` | `skeeto/w64devkit` | master |

每个 submodule 内部 `origin` 指 fork，`upstream` 指原作者。新 clone 出来的 submodule 默认只有 `origin`，如果要同步上游需手动 `git remote add upstream <upstream-url>`。

### 4. 更新到上游最新

```bash
# 一键把所有 submodule 推进到各自跟踪分支的最新
git submodule update --remote --merge

# 主仓库会看到 submodule pin 移动，提交并 push
git add demo-code external/PortableGL external/mesa tools/w64devkit-src
git commit -m "Bump submodules to upstream HEAD"
git push origin new-theme
```

### 5. 推送改动

**只改主仓库内容（AGENTS.md、demo-code-cs、docs 等）**：

```bash
git add <files> && git commit -m "..." && git push origin new-theme
```

**改了某个 submodule 的内容**（demo-code / PortableGL / w64devkit-src）：

```bash
cd <submodule-path>
git checkout master                  # submodule 默认 detached HEAD，先切到分支
git add <files> && git commit -m "..."
git push origin master               # 推到 fork
cd <repo-root>
git add <submodule-path>             # 主仓库记录新的 pin commit
git commit -m "Bump <submodule> to <short-sha>"
git push origin new-theme
```

mesa 没有 fork，原则上不在本地改它。

### 6. 被忽略的本地工具（需自行下载，不入库）

| 路径 | 内容 | 来源 |
|---|---|---|
| `tools/w64devkit/` | 解压后的 w64devkit 工具链二进制（仅 Windows） | https://github.com/skeeto/w64devkit/releases |
| `tools/winlibs-mingw/` | winlibs MinGW-w64 工具链（仅 Windows） | https://winlibs.com/ |
| `tools/portablegl-labs/`、`tools/portablegl-smoke/` | PortableGL 实验沙盒 | 自行创建，不必恢复 |
| `tools/w64devkit-src-openssl-test/` | w64devkit 源码副本，已废弃 | 不必恢复 |

Linux 下 `tools/w64devkit/` 和 `winlibs-mingw/` 用不到，跳过。
