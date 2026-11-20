# Navigation Marker

基于 Unity NavMesh 路径绘制运行时导航线，并通过材质贴图滚动显示连续的方向箭头。

包内包含可直接使用的脚本、Prefab、材质和箭头贴图。

## 功能

- 使用 `NavMesh.CalculatePath` 计算玩家到目标点的路径
- 根据 NavMesh 路径拐点更新 LineRenderer
- 每帧追踪移动中的玩家和目标点
- 自动根据路径总长度调整箭头贴图重复次数
- 通过材质偏移实现箭头滚动动画
- 支持自定义箭头在世界空间中的显示间距

## 示例

在 Package Manager 的 Samples 中导入 **Runtime Navigation**，新建空场景并将 `RuntimeNavigationExample` 添加到空物体，进入 Play 模式即可操作。运行时自动创建 NavMesh，演示移动终点、路径更新和导航线显示控制。

详见 [示例说明](Samples~/Runtime%20Navigation/README.md)。示例只创建并清理自己的运行时对象。

## 安装

当前源码版本为 `1.0.0`。准备本包源码，使用本地 UPM 安装：

1. 在 `Window > Package Manager > + > Add package from disk...` 选择本包根目录的 `package.json`。
2. 等待 Unity 解析清单中声明的 Unity 依赖并完成编译。

也可以将完整包目录放入项目的 `Packages/com.yvsim.navigation-marker`，作为 Embedded Package 使用。

本说明不代表 Git Tag 或 OpenUPM 的 `1.0.0` 已发布；发布完成前请使用上述本地源码安装方式。

## 使用前准备

1. 在场景中准备可用的 NavMesh。
2. 确保玩家和目标点位于可计算路径的区域。
3. 使用包内的 `NavigationMarker` Prefab，或将组件添加到带有 LineRenderer 的 GameObject。
4. 设置玩家 `player` 和终点 `endPoint`。`line` 未赋值时，初始化会取得同物体的 LineRenderer；使用自定义 LineRenderer 时需配置支持主贴图缩放/偏移的材质。

## 使用方法

运行时代码使用以下命名空间：

```csharp
using Yvsim.NavigationMarker;
```

### 启动导航

```csharp
using Yvsim.NavigationMarker;
using UnityEngine;

public class NavigationExample : MonoBehaviour
{
    [SerializeField] private NavigationMarker marker;
    [SerializeField] private Transform player;
    [SerializeField] private Transform destination;

    private void Start()
    {
        marker.player = player;
        marker.endPoint = destination;
        marker.Initialization();
        marker.EnableNavigation();
    }
}
```

必须先调用 `Initialization()` 创建 NavMeshPath 缓存，再调用 `EnableNavigation()` 初始化 LineRenderer 状态。

### 停止导航

```csharp
marker.enabled = false;
marker.DisableNavigation();
```

`DisableNavigation()` 只清空当前 LineRenderer。如果组件仍处于启用状态且引用完整，`Update()` 会在下一帧重新计算并显示路径，因此需要同时禁用组件。

重新启用：

```csharp
marker.enabled = true;
marker.EnableNavigation();
```

### 调整箭头间距

```csharp
marker.textureWorldWidth = 2f;
```

该值表示贴图横向重复一次所对应的世界空间长度。数值越小，路径上的箭头越密集。

## Prefab 与资源

包内提供：

- `NavigationMarker.prefab`
- `NavigationMarker.mat`
- `Arrow.png`

可以从 Package Manager 的包目录中将 Prefab 拖入场景，也可以通过 Resources 加载：

```csharp
GameObject prefab = Resources.Load<GameObject>("NavigationMarker");
GameObject instance = Instantiate(prefab);
```

## 注意事项

- 组件会每帧重新计算完整 NavMesh 路径，目标数量较多时应评估性能。
- 材质缺失时仍更新路径，跳过贴图缩放与偏移；箭头动画需要支持主贴图的材质。
- `line.material` 会访问当前 LineRenderer 的材质实例。包内材质面向 Built-in 渲染管线，URP/HDRP 项目需自行换成对应管线材质。
- `textureWorldWidth` 应为有限正数；无效值会跳过材质缩放并保留上次有效结果，路径仍正常更新。
- 玩家或目标点不在 NavMesh 上时，路径可能为空。

## 主要类型

| 类型 | 说明 |
| --- | --- |
| `NavigationMarker` | NavMesh 路径计算、LineRenderer 绘制与贴图滚动组件 |

## 环境要求

- Unity 2021.3 或更高版本
- Unity AI 模块（`com.unity.modules.ai`）
- 场景中已烘焙或运行时生成的 NavMesh

## 许可证

本包使用 MIT License，详情参见仓库中的许可证文件。

## 依赖说明

本包不依赖 Inspector Groups 或其他 Yvsim 包。

## 从 GitHub 安装

仓库：[com.yvsim.navigation-marker](https://github.com/liuyan7669/com.yvsim.navigation-marker)。仓库名与 UPM 包名一致。

在 Unity Package Manager 选择 Add package from git URL，输入：

```text
https://github.com/liuyan7669/com.yvsim.navigation-marker.git#main
```

此地址指向 main 分支，当前包版本为 1.0.0。本次上传不创建版本 tag 或 GitHub Release，也不代表已在 OpenUPM 登记；Unity 官方依赖仍按 package.json 解析。
