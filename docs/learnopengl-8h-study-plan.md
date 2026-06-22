# LearnOpenGL 8 小时深度学习计划与进度

> 本文档用于跨会话续接学习。新会话继续学习时，先阅读 `AGENTS.md`、`docs/learning-science-for-fast-technical-learning.md` 和本文档，再从“当前进度”和“下一步”继续。

## 学习对象

- 教材：`docs/`
- 示例代码：`demo-code/`
- OpenGL 3.3 Core 规范：`external/glspec33.core.pdf`
- 简化底层实现对照：`external/PortableGL/`
- 真实 Linux OpenGL 实现对照：`external/mesa/` 中的 Mesa/Gallium/softpipe
- PortableGL 本地验证工具：`tools/portablegl-smoke/portablegl_smoke.c`
- PortableGL 主题实验区：`tools/portablegl-labs/`
- 本地 GCC 工具链：`tools/w64devkit/w64devkit/bin/gcc.exe`

## 学习目标

8 小时内不追求逐字读完整本书，也不承诺“精通 OpenGL”。目标是建立可解释、可运行、可修改、可复习的 OpenGL 核心知识骨架，并为后续长期记忆和项目实践留下清晰入口。

完成后应达到：

1. 能闭卷画出 OpenGL 渲染管线。
2. 能解释一个 3D 物体从 C++ 数据到屏幕像素的完整路径。
3. 能看懂并修改入门、光照、模型加载、高级 OpenGL 的核心示例。
4. 能把 `docs/`、`demo-code/`、PortableGL 和 Mesa softpipe 中的同类概念建立对应关系。
5. 能用主动回忆和间隔复习，把 8 小时内建立的短期理解继续转成长期记忆。

### 验收标准

学完 8 小时后，至少满足以下标准：

- 能讲：不用看文档，用 5 分钟讲清 `CPU -> OpenGL 状态 -> GPU/软件管线 -> framebuffer -> 屏幕`。
- 能画：画出“模型文件/顶点数组到屏幕像素”的数据流。
- 能改：选择一个 `demo-code` 示例和一个 PortableGL 小实验，各改一个参数并预测画面/像素变化。
- 能查：知道 VAO/VBO、shader、texture、depth、blend、framebuffer 在 PortableGL 和 Mesa softpipe 中大致看哪些文件。
- 能排错：能按“资源路径 -> 对象绑定 -> shader/uniform -> texture -> matrix -> depth/blend/framebuffer”顺序定位常见问题。
- 能复习：写下第 1/3/7/14/30 天的复习问题和遗忘点。

## 学习科学依据

本文的学习方式以 [learning-science-for-fast-technical-learning.md](G:/Home/MySyncthing/个人工作/tmp/LearnOpenGL-CN/docs/learning-science-for-fast-technical-learning.md) 为依据。

执行原则：

1. 控制认知负荷：先主干，后细节；不要一次塞入过多高级主题。
2. 示例学习：每个核心主题先看可运行示例，再做小改动。
3. 主动回忆：每节结束都要闭卷回答问题，而不是只重读。
4. 反馈修正：用编译错误、画面错误、像素输出和调试路径暴露模型缺口。
5. 间隔复习：8 小时只是起点，长期记忆靠第 1/3/7/14/30 天复习维护。
6. 迁移练习：同一概念至少连接 `demo-code`、PortableGL 和 Mesa softpipe 三个层次。

证据边界：

- Practice testing、distributed practice、worked examples 的证据较强，适合普通学习者。
- 刻意练习只采用“目标明确、反馈明确、修正弱点”的结构，不把它当作万能公式。
- 元分析中的有效性主要支持保持、提取和近迁移；OpenGL 的深层迁移还需要项目改造、调试练习和跨主题连接。

## 学习单元协议

每次开始章节或主题前，必须先给出：

```text
本节阅读方式：
为什么这样读：
你阅读时重点看：
可以跳过或快速扫过：
读完后必须能回答：
对应 demo-code：
OpenGL 规范对照：
底层调试 Demo：
Mesa softpipe 对照：
本节长期记忆钩子：
```

每次结束章节或主题时，必须输出：

```text
闭卷检查：
数据流图：
最小实验：
规范依据：
底层实现印证：
常见错误：
下次复习点：
```

### 阶段确认规则

学习过程必须保留阶段确认点，不能默认用户已经读完、理解或完成实验。

- 要求用户阅读某一章、某一节或某段源码后，先暂停并询问：`你读完了吗？哪些地方卡住了？` 等用户确认后再继续讲解。
- 讲完一个核心概念后，给出 2-3 个检查问题，等待用户回答或说明疑问，再进入下一概念。
- 要求用户修改 `demo-code`、PortableGL 小实验或运行命令后，先确认运行结果、报错或观察到的画面变化，再继续扩展。
- 每完成一个学习小时或一个主题模块后，再更新“已完成内容”“当前遗忘风险”“下一步”和“下次复习问题”。

每个学习小时结束时，更新本文档中的：

- 已完成内容。
- 当前遗忘风险。
- 下一步。
- 下次复习问题。
- 如新增 PortableGL 小实验，记录其路径和验证命令。

### 每小时学习记录

每个学习小时的内容必须记录到独立文档中，放在 `docs/EveryHours/` 目录下，文件名格式为 `hour-N-主题.md`（如 `hour-2-hello-triangle.md`）。

每小时学习记录应包含：

- 本节阅读方式和重点
- OpenGL 3.3 Core 规范中的关键定义、状态、约束或管线阶段说明
- 完整的知识讲解（含闭卷检查、数据流图、常见错误等）
- 自此文档是该小时的完整学习记录，后续复习时直接查阅此文

### OpenGL 规范讲解规则

学习过程中必须把 `external/glspec33.core.pdf` 作为权威语义来源之一。LearnOpenGL 用来建立直观模型，`demo-code` 用来观察用法，OpenGL 规范用来确认 API、状态机、对象、管线阶段和错误条件的精确定义。

讲解每个重要主题时，至少补充：

- 该主题在 OpenGL 3.3 Core 规范中的概念位置，例如对象模型、状态变量、顶点处理、纹理、光栅化、片段操作或 framebuffer。
- 关键 API 的规范语义：它读写哪些 OpenGL 状态，依赖哪些当前绑定对象，可能产生哪些约束或错误。
- 教程说法与规范说法的差异：教程负责易懂，规范负责精确；如果二者表达粒度不同，以规范语义校准心智模型。
- 涉及规范原文时，必须把核心内容翻译成中文并解释清楚，翻译要做到信、达、雅；不要要求用户直接阅读英文原文。必要时可附极短英文关键词用于定位，但正文讲解以中文为主。
- 规范不要求逐页精读，只在当前主题相关处精读；避免提前陷入完整规范细节。

### 底层实现讲解规则

讲解底层实现印证时，必须做到以下三点：

1. **数据流图（mermaid flowchart）**：从 API 调用到帧缓冲像素输出的完整路径，标注每个阶段的输入输出和关键数据结构。
2. **时序图（mermaid sequenceDiagram）**：展示初始化阶段和绘制阶段中，C++ 应用代码与 OpenGL 状态机之间的交互序列。
3. **管线流程图（mermaid flowchart）**：展示 draw call 触发后管线的逐步下沉逻辑，包括裁剪判断、面剔除、光栅化逐像素循环、片段着色器调用、深度/混合测试和像素写入。

此外还需：

4. **程序思路详解**：逐函数拆解 PortableGL（或 Mesa）源码中的关键实现，说明每个参数、每行关键逻辑的目的。不能只给函数名和行号，必须用中文解释"这段代码在做什么、为什么这样做"。
5. **对照表**：将 OpenGL 概念映射到 PortableGL 实现和 Mesa softpipe 入口，三列对齐。
6. **关系图**：对 VAO/VBO/EBO 等 OpenGL 对象间的关系，用 mermaid 图示意引用和绑定关系。

## 阅读分层

不逐字精读所有章节。每个章节按三类处理：

- 精读：主干章节。必须读文档、对照 demo、画数据流、改代码、做主动回忆、看底层调试 Demo。
- 中读：重要扩展。理解问题、管线位置、核心 API/GLSL、典型场景和底层大致位置。
- 速读：背景、索引或暂时非主线内容。知道用途和以后何时回查。

默认分层：

- 精读：Hello Triangle、Shaders、Textures、Transformations、Coordinate Systems、Camera、Basic Lighting、Materials、Lighting Maps、Multiple Lights、Model Loading、Depth Testing、Blending、Framebuffers、Cubemaps、Shadow Mapping、Normal Mapping、HDR、Bloom、Deferred Shading、PBR 主线。
- 中读：Stencil Testing、Face Culling、Instancing、Advanced GLSL、Uniform Buffer Objects、Point Shadows、Parallax Mapping、SSAO、Anti Aliasing。
- 速读：环境配置细节、重复性代码解释、Guest Articles、暂时不影响主干心智模型的数学推导。

## 底层实现扩展方案

学习路径固定为：

```text
LearnOpenGL docs + demo-code
  -> PortableGL 简化软件实现
  -> Mesa softpipe 真实 Linux 开源实现对照
```

使用原则：

1. 主学习目标仍然是 OpenGL 和图形学心智模型，不把学习重点转成驱动工程。
2. PortableGL 是第一底层参照：用来调试顶点处理、shader-like 函数、光栅化、片段处理、深度缓冲和颜色缓冲。
3. Mesa softpipe 是真实实现位置对照：用来知道 Linux Mesa/Gallium 中同类概念落在哪些层。
4. 每个主题只看当前概念相关路径，避免提前陷入完整 Mesa 架构。

本地验证命令：

```powershell
$env:PATH = (Resolve-Path 'tools\w64devkit\w64devkit\bin').Path + ';' + $env:PATH
gcc -std=c99 -O2 -ffp-contract=off -w -o tools\portablegl-smoke\portablegl_smoke.exe tools\portablegl-smoke\portablegl_smoke.c -lm
tools\portablegl-smoke\portablegl_smoke.exe
```

期望输出：

```text
red_pixels=968
```

## 底层调试映射表

说明：

- `已有` 表示当前仓库已经有可读/可运行入口。
- `派生` 表示学习到该主题时，从 `tools/portablegl-smoke/portablegl_smoke.c` 复制出一个小实验，保存在 `tools/portablegl-labs/<topic>.c`，用于单点调试。
- 派生实验的命名、编译和输出规则见 `tools/portablegl-labs/README.md`。
- Mesa 路径用于对照真实实现位置，不要求完整编译 Mesa。

| 主题 | 阅读 | LearnOpenGL 示例 | PortableGL 底层调试 Demo | PortableGL 关键实现 | Mesa softpipe 对照 |
|---|---|---|---|---|---|
| Window / Context / Clear | 精读 | `demo-code/src/1.getting_started/1.1.hello_window/`, `1.2.hello_window_clear/` | 已有：`tools/portablegl-smoke/portablegl_smoke.c` 中 `init_glContext`, `glClearColor`, `glClear` | `portablegl.h`: `glContext`, `glFramebuffer`, `glClear`, `pglClearScreen` | `src/mesa/main/context.c`, `src/mesa/state_tracker/st_context.c`, `src/gallium/drivers/softpipe/sp_clear.c` |
| Hello Triangle / Draw Call | 精读 | `demo-code/src/1.getting_started/2.1.hello_triangle/` | 已有：`tools/portablegl-smoke/portablegl_smoke.c`; `external/PortableGL/testing/hello_triangle.c` | `glDrawArrays -> run_pipeline -> draw_triangle_* -> fragment_shader -> draw_pixel` | `src/mesa/vbo/vbo_exec_draw.c`, `src/mesa/state_tracker/st_draw.c`, `src/gallium/drivers/softpipe/sp_draw_arrays.c` |
| VAO / VBO / EBO / Attribute | 精读 | `2.1.hello_triangle/`, `2.2.hello_triangle_indexed/` | 已有：`testing/hello_indexing.c`; 派生：`tools/portablegl-labs/vao_vbo_ebo.c` | `glBufferData`, `glVertexAttribPointer`, vertex array/buffer storage | `src/mesa/main/bufferobj.c`, `src/mesa/main/arrayobj.c`, `src/mesa/state_tracker/st_atom_array.cpp`, `src/gallium/drivers/softpipe/sp_state_vertex.c` |
| Shaders / Uniform | 精读 | `demo-code/src/1.getting_started/3.3.shaders_class/` | 已有：`tools/portablegl-smoke/portablegl_smoke.c`; `examples/original/ex1_std_shaders.c`; 派生：`tools/portablegl-labs/shader_uniform.c` | `pglCreateProgram`, `glUseProgram`, `pglSetUniform`, `vert_func`, `frag_func` | `src/mesa/main/shaderapi.c`, `src/mesa/program/`, `src/mesa/state_tracker/st_glsl_to_nir.cpp`, `src/mesa/state_tracker/st_program.c`, `src/compiler/nir/` |
| Interpolation / Varying | 精读 | Shaders 章节 | 已有：`external/PortableGL/testing/hello_interpolation.c` | `vs_output`, interpolation mode, fragment shader input | `src/mesa/state_tracker/st_atom_shader.c`, `src/gallium/auxiliary/draw/`, `src/gallium/drivers/softpipe/sp_quad_fs.c` |
| Textures / Sampler | 精读 | `demo-code/src/1.getting_started/4.2.textures_combined/` | 已有：`external/PortableGL/testing/texturing.cpp`, `testing/texture_perf.cpp`; 派生：`tools/portablegl-labs/texture_sampling.c` | `glTexImage2D`, `pglTexImage2D`, texture sampling helpers | `src/mesa/main/teximage.c`, `src/mesa/state_tracker/st_atom_texture.c`, `st_sampler_view.c`, `src/gallium/drivers/softpipe/sp_tex_sample.c`, `sp_texture.c` |
| Transformations / MVP | 精读 | `demo-code/src/1.getting_started/5.2.transformations/` | 派生：`tools/portablegl-labs/mvp_transform.c` | `make_v4`, matrix helpers, vertex shader 修改 `gl_Position` | `src/mesa/program/prog_statevars.c`, `src/mesa/state_tracker/st_atom_constbuf.c`, `src/compiler/nir/` |
| Coordinate Systems / Depth | 精读 | `demo-code/src/1.getting_started/6.2.coordinate_systems_depth/` | 已有：`testing/clipping.c`; 派生：`tools/portablegl-labs/clip_depth.c` | clip space, viewport transform, `draw_triangle_clip`, depth buffer | `src/mesa/state_tracker/st_atom_viewport.c`, `st_atom_depth.c`, `src/gallium/drivers/softpipe/sp_quad_depth_test.c` |
| Camera / View Matrix | 精读 | `demo-code/src/1.getting_started/7.4.camera_class/` | 派生：`tools/portablegl-labs/view_matrix_camera.c` | 在 vertex shader 中改变 view matrix uniform | `src/mesa/state_tracker/st_atom_constbuf.c`, `src/compiler/nir/` |
| Basic Lighting | 精读 | `demo-code/src/2.lighting/2.1.basic_lighting_diffuse/`, `2.2.basic_lighting_specular/` | 派生：`tools/portablegl-labs/basic_lighting.c` | fragment shader-like 函数中计算 normal/light/view/specular | `src/compiler/nir/`, `src/mesa/state_tracker/st_program.c`, `src/gallium/drivers/softpipe/sp_fs_exec.c` |
| Materials / Lighting Maps | 精读 | `demo-code/src/2.lighting/3.1.materials/`, `4.2.lighting_maps_specular_map/` | 派生：`tools/portablegl-labs/lighting_maps.c` | texture sampling + fragment shader 材质参数 | `st_atom_texture.c`, `st_atom_sampler.c`, `sp_tex_sample.c`, `sp_quad_fs.c` |
| Multiple Lights | 精读 | `demo-code/src/2.lighting/6.multiple_lights/` | 派生：`tools/portablegl-labs/multiple_lights.c` | uniform struct/array + fragment shader 循环累加 | `st_atom_constbuf.c`, `st_program.c`, `sp_fs_exec.c` |
| Model Loading / Mesh Draw | 精读 | `demo-code/src/3.model_loading/1.model_loading/`, `includes/learnopengl/mesh.h`, `model.h` | 已有：`external/PortableGL/demos/modelviewer.c`; 派生：`tools/portablegl-labs/mesh_draw.c` | 多 mesh 的 vertex/index/texture/draw 调用组织 | `src/mesa/vbo/`, `st_draw.c`, `sp_draw_arrays.c`, `sp_buffer.c` |
| Depth Testing | 精读 | `demo-code/src/4.advanced_opengl/1.depth_testing/` | 已有：`testing/expected_output/zbuf_*`; 派生：`tools/portablegl-labs/depth_test.c` | `glDepthFunc`, `glDepthMask`, `fragment_processing`, zbuf | `st_atom_depth.c`, `sp_quad_depth_test.c` |
| Stencil Testing | 中读 | `demo-code/src/4.advanced_opengl/2.stencil_testing/` | 派生：`tools/portablegl-labs/stencil_test.c` | stencil buffer、stencil op、fragment processing | `st_atom_depth.c`, `sp_quad_depth_test.c`, stencil op helpers |
| Blending | 精读 | `demo-code/src/4.advanced_opengl/3.2.blending_sort/` | 已有：`external/PortableGL/testing/blending.cpp` | `glBlendFunc`, `glBlendEquation`, `blend_pixel`, `put_pixel_blend` | `st_atom_blend.c`, `sp_state_blend.c`, `sp_quad_blend.c` |
| Face Culling / Rasterizer State | 中读 | `demo-code/src/4.advanced_opengl/4.face_culling/` | 派生：`tools/portablegl-labs/face_culling.c` | front/back mode, winding, `draw_triangle_front/back` | `st_atom_rasterizer.c`, `sp_state_rasterizer.c`, `sp_prim_vbuf.c` |
| Framebuffers / Render Target | 精读 | `demo-code/src/4.advanced_opengl/5.1.framebuffers/` | 派生：`tools/portablegl-labs/framebuffer_postprocess.c` | `glFramebuffer`, back_buffer, color/depth attachment 思路；PortableGL 部分 FBO API 是 stub，重点看 framebuffer 结构和后处理思路 | `st_atom_framebuffer.c`, `st_cb_clear.c`, `sp_state_surface.c`, `u_framebuffer.c` |
| Cubemaps / Skybox | 精读 | `demo-code/src/4.advanced_opengl/6.1.cubemaps_skybox/` | 已有：`external/PortableGL/demos/cubemap.cpp`, `testing/skybox_clipping.cpp` | texture lookup + cube/skybox draw | `st_atom_texture.c`, `sp_tex_sample.c`, `st_atom_depth.c` |
| Instancing | 中读 | `demo-code/src/4.advanced_opengl/10.3.asteroids_instanced/` | 已有：`external/PortableGL/testing/instancing.cpp`, `glinstanceid.cpp`, `baseinstance.cpp` | `glDrawArraysInstanced`, `gl_InstanceID`, base instance | `st_draw.c`, `sp_draw_arrays.c`, `u_split_draw.c` |
| Gamma / HDR / Bloom | 精读其中 HDR/Bloom，Gamma 中读 | `demo-code/src/5.advanced_lighting/6.hdr/`, `7.bloom/` | 派生：`tools/portablegl-labs/hdr_tonemap_bloom.c` | fragment shader-like 后处理、float color 到 framebuffer 的映射 | `st_atom_framebuffer.c`, `st_program.c`, `sp_quad_fs.c`, `sp_quad_blend.c` |
| Shadow Mapping | 精读 | `demo-code/src/5.advanced_lighting/3.1.1.shadow_mapping_depth/`, `3.1.3.shadow_mapping/` | 派生：`tools/portablegl-labs/shadow_depth_map.c` | 两遍渲染：先写深度，再采样深度比较；PortableGL 重点模拟流程 | `st_atom_framebuffer.c`, `st_atom_depth.c`, `sp_quad_depth_test.c`, `sp_tex_sample.c` |
| Normal Mapping | 精读 | `demo-code/src/5.advanced_lighting/4.normal_mapping/` | 派生：`tools/portablegl-labs/normal_mapping.c` | tangent space、normal texture sampling、fragment lighting | `st_atom_array.cpp`, `st_atom_texture.c`, `sp_tex_sample.c`, `sp_fs_exec.c` |
| Deferred Shading | 精读概念，中读实现 | `demo-code/src/5.advanced_lighting/8.deferred_shading/` | 派生：`tools/portablegl-labs/deferred_gbuffer.c` | 多 render target / G-buffer 思路；PortableGL 中重点模拟数据流 | `st_atom_framebuffer.c`, `st_atom_array.cpp`, `st_atom_texture.c`, `sp_quad_fs.c` |
| SSAO | 中读 | `demo-code/src/5.advanced_lighting/9.ssao/` | 派生：`tools/portablegl-labs/ssao_kernel.c` | 屏幕空间采样思想，不深挖优化 | `st_atom_texture.c`, `sp_tex_sample.c`, `sp_quad_fs.c` |
| PBR / IBL | 精读 PBR Theory 和 Lighting，IBL 中读 | `docs/07 PBR/...`, 对应 PBR demo | 派生：`tools/portablegl-labs/pbr_brdf.c` | BRDF 函数、Fresnel、roughness/metallic 作为 fragment shader-like 计算 | `src/compiler/nir/`, `st_program.c`, `sp_fs_exec.c`, `sp_tex_sample.c` |

## 全书主线地图

```text
CPU 程序
  -> OpenGL 状态机
  -> 顶点数据 VAO/VBO/EBO
  -> 顶点着色器
  -> 图元装配
  -> 裁剪 / 透视除法 / viewport
  -> 光栅化
  -> 片段着色器
  -> 深度/模板/混合测试
  -> 帧缓冲
  -> 屏幕输出
```

实现对照地图：

```text
LearnOpenGL API 调用
  -> PortableGL: portablegl.h 中的状态结构和软件管线
  -> Mesa: src/mesa/main + src/mesa/vbo + src/mesa/state_tracker
  -> Gallium: pipe_context / pipe_resource / state objects
  -> softpipe: 软件光栅化、depth/blend/texture/sample
```

## 8 小时路线

### 第 1 小时：OpenGL 总览与窗口闭环

目标：建立 OpenGL 规范、状态机、上下文、窗口清屏和学习地图。

章节：

- 精读：`docs/01 Getting started/01 OpenGL.md`
- 精读：`docs/01 Getting started/02 Creating a window.md`
- 速读：`docs/index.md`
- 速读：`docs/intro.md`

对应代码：

- `demo-code/src/1.getting_started/1.1.hello_window/`
- `demo-code/src/1.getting_started/1.2.hello_window_clear/`

底层调试 Demo：

- `tools/portablegl-smoke/portablegl_smoke.c` 中 `init_glContext`、`glClearColor`、`glClear`
- Mesa：`st_context.c`、`sp_clear.c`

必须讲清：

- OpenGL 为什么是状态机？
- OpenGL、GLFW、GLAD、GLM 分别负责什么？
- `glClearColor` 设置状态，`glClear` 使用状态，这个模式为什么重要？

### 第 2-3 小时：入门主干

目标：掌握最小可运行 3D 渲染闭环。

章节：

- 精读：`docs/01 Getting started/04 Hello Triangle.md`
- 精读：`docs/01 Getting started/05 Shaders.md`
- 精读：`docs/01 Getting started/06 Textures.md`
- 精读：`docs/01 Getting started/07 Transformations.md`
- 精读：`docs/01 Getting started/08 Coordinate Systems.md`
- 精读：`docs/01 Getting started/09 Camera.md`

对应代码：

- `demo-code/src/1.getting_started/2.1.hello_triangle/`
- `demo-code/src/1.getting_started/3.3.shaders_class/`
- `demo-code/src/1.getting_started/4.2.textures_combined/`
- `demo-code/src/1.getting_started/6.2.coordinate_systems_depth/`
- `demo-code/src/1.getting_started/7.4.camera_class/`

底层调试 Demo：

- Hello Triangle：`tools/portablegl-smoke/portablegl_smoke.c`
- Indexing：`external/PortableGL/testing/hello_indexing.c`
- Interpolation：`external/PortableGL/testing/hello_interpolation.c`
- Textures：`external/PortableGL/testing/texturing.cpp`
- Clip/depth：`external/PortableGL/testing/clipping.c`
- 派生实验：`tools/portablegl-labs/mvp_transform.c`、`view_matrix_camera.c`

必须讲清：

- VAO、VBO、EBO 各自保存什么？
- `glVertexAttribPointer` 解释的是哪段 buffer 数据？
- vertex shader 和 fragment shader 的输入输出是什么？
- uniform 如何把 CPU 状态传给 shader？
- Model、View、Projection 三个矩阵分别解决什么问题？
- 相机为什么本质上是 View 矩阵？

### 第 4 小时：光照核心

目标：理解经典实时光照模型，把光照看成“几何方向关系 + 材质参数 + shader 计算”。

章节：

- 精读：`docs/02 Lighting/01 Colors.md`
- 精读：`docs/02 Lighting/02 Basic Lighting.md`
- 精读：`docs/02 Lighting/03 Materials.md`
- 精读：`docs/02 Lighting/04 Lighting maps.md`
- 中读：`docs/02 Lighting/05 Light casters.md`
- 精读：`docs/02 Lighting/06 Multiple lights.md`

对应代码：

- `demo-code/src/2.lighting/2.1.basic_lighting_diffuse/`
- `demo-code/src/2.lighting/2.2.basic_lighting_specular/`
- `demo-code/src/2.lighting/4.2.lighting_maps_specular_map/`
- `demo-code/src/2.lighting/6.multiple_lights/`

底层调试 Demo：

- 派生实验：`tools/portablegl-labs/basic_lighting.c`
- 派生实验：`tools/portablegl-labs/lighting_maps.c`
- 派生实验：`tools/portablegl-labs/multiple_lights.c`
- Mesa：`sp_fs_exec.c`、`sp_quad_fs.c`、`st_program.c`、`st_atom_texture.c`

必须讲清：

- 法线为什么决定明暗？
- 漫反射和高光分别模拟什么？
- 贴图如何把“常量材质”变成“空间变化的材质”？
- 多光源为什么本质上是多次光照贡献累加？

### 第 5 小时：模型加载

目标：从手写立方体过渡到真实模型，理解 Assimp、Mesh、Model、Texture 和 Draw 的关系。

章节：

- 精读：`docs/03 Model Loading/01 Assimp.md`
- 精读：`docs/03 Model Loading/02 Mesh.md`
- 精读：`docs/03 Model Loading/03 Model.md`

对应代码：

- `demo-code/includes/learnopengl/mesh.h`
- `demo-code/includes/learnopengl/model.h`
- `demo-code/src/3.model_loading/1.model_loading/`

底层调试 Demo：

- `external/PortableGL/demos/modelviewer.c`
- 派生实验：`tools/portablegl-labs/mesh_draw.c`
- Mesa：`src/mesa/vbo/`、`st_draw.c`、`sp_draw_arrays.c`、`sp_buffer.c`

必须讲清：

```text
.obj/.dae 文件
  -> Assimp Scene
  -> Node
  -> Mesh
  -> Vertex / Index / Texture
  -> OpenGL VAO/VBO/EBO
  -> Draw
```

### 第 6 小时：高级 OpenGL 主干

目标：理解渲染状态和后续管线阶段。

章节：

- 精读：`docs/04 Advanced OpenGL/01 Depth testing.md`
- 中读：`docs/04 Advanced OpenGL/02 Stencil testing.md`
- 精读：`docs/04 Advanced OpenGL/03 Blending.md`
- 中读：`docs/04 Advanced OpenGL/04 Face culling.md`
- 精读：`docs/04 Advanced OpenGL/05 Framebuffers.md`
- 精读：`docs/04 Advanced OpenGL/06 Cubemaps.md`
- 中读：`docs/04 Advanced OpenGL/10 Instancing.md`

对应代码：

- `demo-code/src/4.advanced_opengl/1.depth_testing/`
- `demo-code/src/4.advanced_opengl/2.stencil_testing/`
- `demo-code/src/4.advanced_opengl/3.2.blending_sort/`
- `demo-code/src/4.advanced_opengl/5.1.framebuffers/`
- `demo-code/src/4.advanced_opengl/6.1.cubemaps_skybox/`
- `demo-code/src/4.advanced_opengl/10.3.asteroids_instanced/`

底层调试 Demo：

- Depth：`external/PortableGL/testing/expected_output/zbuf_*` + `tools/portablegl-labs/depth_test.c`
- Blending：`external/PortableGL/testing/blending.cpp`
- Cubemap：`external/PortableGL/demos/cubemap.cpp`
- Instancing：`external/PortableGL/testing/instancing.cpp`
- Mesa：`st_atom_depth.c`、`sp_quad_depth_test.c`、`st_atom_blend.c`、`sp_quad_blend.c`、`st_atom_framebuffer.c`

必须讲清：

- 深度测试为什么能解决遮挡？
- 模板测试为什么能做描边和遮罩？
- 透明物体为什么通常需要排序？
- 帧缓冲为什么让后处理成为可能？
- Instancing 为什么能减少大量 draw call？

### 第 7 小时：高级光照与 PBR 地图

目标：建立现代实时渲染大图，但不在一小时内假装精通所有高级主题。

本小时修正原则：

- 精读 2 个代表主题：Shadow Mapping、Normal Mapping。
- 中读 HDR/Bloom/Deferred/PBR Theory，建立主线地图。
- SSAO、IBL 只建立索引和后续学习入口。

章节：

- 中读：`docs/05 Advanced Lighting/02 Gamma Correction.md`
- 精读：`docs/05 Advanced Lighting/03 Shadows/01 Shadow Mapping.md`
- 精读：`docs/05 Advanced Lighting/04 Normal Mapping.md`
- 中读：`docs/05 Advanced Lighting/06 HDR.md`
- 中读：`docs/05 Advanced Lighting/07 Bloom.md`
- 中读：`docs/05 Advanced Lighting/08 Deferred Shading.md`
- 速读：`docs/05 Advanced Lighting/09 SSAO.md`
- 精读：`docs/07 PBR/01 Theory.md`
- 中读：`docs/07 PBR/02 Lighting.md`
- 速读：`docs/07 PBR/03 IBL/01 Diffuse irradiance.md`
- 速读：`docs/07 PBR/03 IBL/02 Specular IBL.md`

底层调试 Demo：

- Shadow：`tools/portablegl-labs/shadow_depth_map.c`
- Normal：`tools/portablegl-labs/normal_mapping.c`
- HDR/Bloom：`tools/portablegl-labs/hdr_tonemap_bloom.c`
- Deferred：`tools/portablegl-labs/deferred_gbuffer.c`
- PBR：`tools/portablegl-labs/pbr_brdf.c`
- Mesa：`st_atom_framebuffer.c`、`st_atom_depth.c`、`st_atom_texture.c`、`sp_tex_sample.c`、`sp_quad_fs.c`、`src/compiler/nir/`

必须讲清：

```text
传统光照：经验模型，让画面看起来像。
Shadow Mapping：先把光源视角的深度存下来，再从相机视角比较深度。
Normal Mapping：用贴图改变每个片段的法线，而不是增加几何面数。
PBR：用能量守恒、微表面、Fresnel 等约束提高材质稳定性。
```

### 第 8 小时：输出、测试、补洞

目标：确认不是“看过”，而是“可提取、可调试、可复习”。

输出任务：

1. 用 100 字解释 LearnOpenGL 的主线。
2. 用 5 分钟讲清 OpenGL 渲染管线。
3. 画出“模型文件到屏幕像素”的流程图。
4. 选择一个 `demo-code` 示例，解释 C++、shader、纹理/模型资源如何协作。
5. 选择一个 PortableGL 小实验，解释同一概念在软件实现里如何发生。
6. 选 3 个 Mesa 文件，说明它们分别对应 OpenGL 哪一层。
7. 列出后续 30 天最值得深入的 5 个主题。

检查问题：

- 为什么需要 VAO？
- 为什么 shader 需要 uniform？
- 为什么相机不是移动眼睛，而是构造 View 矩阵？
- 为什么透明物体要排序？
- 为什么 Shadow Mapping 会出现 shadow acne？
- 为什么 Gamma Correction 会影响光照观感？
- 为什么 PBR 比 Phong 更稳定？
- PortableGL 中 `glDrawArrays` 后如何进入 `run_pipeline`？
- Mesa 中 OpenGL draw call 大致如何从 `vbo` 到 `state_tracker` 再到 `softpipe`？

## 30 天长期记忆维护

8 小时结束后必须执行轻量复习。每次复习都先闭卷回忆，再回看文档和代码修正。

| 复习点 | 日期 | 闭卷任务 | 代码任务 | 状态 | 遗忘点 | 下次处理 |
|---|---|---|---|---|---|---|
| 第 1 天 | 待填 | 画 OpenGL 管线；解释 VAO/VBO/shader/uniform/draw call | 跑 `portablegl_smoke`，解释 `red_pixels` 来源 | 未开始 | 待填 | 待填 |
| 第 3 天 | 待填 | 写出 Hello Triangle 数据流 | 改三角形颜色或顶点位置 | 未开始 | 待填 | 待填 |
| 第 7 天 | 待填 | 复述 MVP、相机、纹理、基础光照 | 改一个 `demo-code` 和一个 PortableGL lab | 未开始 | 待填 | 待填 |
| 第 14 天 | 待填 | 复述模型加载、高级状态、framebuffer | 做 depth/blend/framebuffer 小实验 | 未开始 | 待填 | 待填 |
| 第 30 天 | 待填 | 5 分钟讲清 LearnOpenGL 主线 | 解释一个 Mesa softpipe 路径 | 未开始 | 待填 | 待填 |

## 当前进度

最后更新：2026-05-27

已完成：

- 明确 `docs/` 是教材，`demo-code/` 是示例代码。
- 建立 8 小时学习路线，并按学习科学文档重构为”主干 + 示例 + 主动回忆 + 间隔复习 + 底层实现对照”。
- 下载并验证 PortableGL：`external/PortableGL/`。
- 下载 Mesa sparse checkout：`external/mesa/`，重点包含 `src/mesa/`、`src/gallium/drivers/softpipe/`、`src/compiler/`。
- 安装便携 GCC 工具链：`tools/w64devkit/w64devkit/bin/gcc.exe`。
- 编译并运行 PortableGL smoke test，输出 `red_pixels=968`。
- 完成第 1 小时主要内容：OpenGL 概念、规范与实现、状态机、上下文、Hello Window / Clear Window。
- 建立第一版 OpenGL 总体心智模型：

```text
C++ 程序
  -> 调用 OpenGL 函数
  -> 驱动或软件实现这些函数
  -> GPU/软件管线执行渲染工作
  -> 结果写入 framebuffer
  -> 窗口显示出来
```

- 明确 OpenGL 是规范而不是单一实现。
- 明确 `glClearColor` 是状态设置函数，`glClear` 是状态使用函数。
- 对照了本地示例：
  - `demo-code/src/1.getting_started/1.1.hello_window/hello_window.cpp`
  - `demo-code/src/1.getting_started/1.2.hello_window_clear/hello_window_clear.cpp`
- 建立 GLFW / GLAD / GLM / OpenGL 的职责边界。
- **完成 Hello Triangle 精读**：渲染管线全景、VBO/VAO/EBO 机制、shader 编译链接、draw call 触发管线。
- **完成底层实现深层对照**：
  - 逐函数追踪 PortableGL 源码：`glGenBuffers`/`glBindBuffer`/`glBufferData`/`glVertexAttribPointer`/`glEnableVertexAttribArray`/`glBindVertexArray`/`glDrawArrays` 在 PortableGL 中的完整实现。
  - 建立了从 `glDrawArrays` → `run_pipeline()` → `vertex_stage()`（含 `get_v_attrib` 指针运算详解）→ `draw_triangle()` → `draw_triangle_final()`（裁剪+视口变换+面剔除）→ `draw_triangle_fill()`（光栅化+插值）→ `fragment_shader` → `draw_pixel()` 的完整调用链。
  - 对照了 Mesa softpipe 的 draw call 入口：`vbo_exec_draw_arrays` → `st_draw_vbo` → `softpipe_draw_vbo` → `draw_vbo`。
  - 建立了完整数据流图、初始化+绘制时序图、管线流程图、VAO/VBO/EBO 关系图。
  - 完整追踪了 `red_pixels=968` 的来历：从 3 个 NDC 顶点 → 顶点着色器直通 → 视口变换 → 光栅化覆盖 968 像素 → 片段着色器输出红色 → `draw_pixel` 写入帧缓冲。

当前遗忘风险：

- `glVertexAttribPointer` 记录的不仅是格式，还记录当时绑定的 VBO ID——这个隐式关联容易被遗忘。
- VAO 和 EBO 的绑定关系：解绑 VAO 前不能解绑 EBO。
- shader 编译-链接-使用-删除的顺序和原因。

下一步：

1. 继续第 2-3 小时：学习 `docs/01 Getting started/05 Shaders.md`。
2. 重点：GLSL 语法、uniform 传值、顶点着色器和片段着色器之间的数据流（in/out）、`pglSetUniform` 在 PortableGL 中的实现。
3. 对照 `demo-code/src/1.getting_started/3.3.shaders_class/`。
4. PortableGL 对照：`portablegl_smoke.c` 中的 `pass_vs`/`solid_fs` 和 `pglSetUniform`；
5. Mesa 对照：`st_atom_constbuf.c`（uniform 如何作为常量缓冲传递给 shader）。
6. 主动回忆问题：
   - vertex shader 的 `in` 和 `out` 分别连接什么？
   - fragment shader 的 `in` 从哪来？`out` 到哪去？
   - uniform 和顶点属性的区别是什么？
   - PortableGL 中 `pglSetUniform` 做了什么？

## 下次会话恢复指令

可以直接对 Codex 说：

```text
继续 LearnOpenGL 8 小时学习计划。先读取 AGENTS.md、docs/learning-science-for-fast-technical-learning.md 和 docs/learnopengl-8h-study-plan.md，从“当前进度”的下一步开始，用中文指导我学习。
```
