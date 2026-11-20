# Changelog

## [1.0.0]

首次发布。

- 基于 NavMesh 路径绘制 LineRenderer 导航线，提供箭头贴图、材质及可加载预制体。
- 初始化自动绑定本物体的 LineRenderer；缺少材质或贴图宽度无效时仍可安全更新路径。
- 根据实际使用移除 Inspector Groups 依赖，支持不安装其他 Yvsim 包的独立使用。
- 新增可导入的 Runtime Navigation 示例：运行时自动创建 NavMesh，演示移动终点、路径更新和导航线显示控制。
