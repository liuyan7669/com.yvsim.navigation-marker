# Runtime Navigation 示例

演示 `NavigationMarker.Initialization()`、`EnableNavigation()`、`DisableNavigation()` 和移动终点时的路径更新。示例在运行时创建平面 NavMesh 和中间不可行走区域，路径会绕开障碍；不需要预先烘焙 NavMesh 或安装 AI Navigation 包。

## 导入与运行

1. 安装 Navigation Marker 1.0.0，在 Package Manager 的 Samples 中导入 **Runtime Navigation**。
2. 新建一个空场景，创建空物体并添加 `RuntimeNavigationExample`，保持旋转为 0、缩放为 1。
3. 进入 Play 模式。示例自动创建相机、寻路数据、起终点和包内导航线 Prefab 的实例。
4. 在 Game 窗口切换 `Move destination`、`Switch destination`、`Hide navigation` / `Show navigation`；在 Inspector 调整 `Arrow Spacing` 查看箭头间距。

`Path corners` 应至少为 2，跨过中央障碍时会出现绕行拐点。隐藏导航后该值为 0，恢复显示后重新绘制。`DisableNavigation()` 只清空当前线段；示例同时停用组件，防止下一帧立即重绘。

## 依赖与清理

- 不依赖 Inspector Groups 或其他 Yvsim 业务包。使用包已声明的 Unity AI 模块；运行时操作面板使用 Unity 内置 IMGUI 模块，精简工程需保留 `com.unity.modules.imgui`。
- 示例视觉按 Unity 2021.3 内置渲染管线的 Editor Play 模式设计。使用包内 Resources 导航 Prefab 和箭头贴图；运行时仅修改材质副本。
- 示例只添加自己的 `NavMeshDataInstance`，停用或移除组件时移除此实例、销毁生成对象和材质；不会清空场景已有 NavMesh。
- 建议在空场景单独运行；演示相机的渲染优先级为 100。宿主位置用于决定演示区域中心，运行中不应移动宿主。
