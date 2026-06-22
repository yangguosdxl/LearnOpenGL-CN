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
