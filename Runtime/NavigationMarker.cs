using UnityEngine;
using UnityEngine.AI;

namespace Yvsim.NavigationMarker
{
/// <summary>
/// 使用 Unity NavMesh 路径拐点，从玩家位置到目标点绘制带动画效果的导航线。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class NavigationMarker : MonoBehaviour
{
    /// <summary>
    /// 计算导航路径时使用的起点。
    /// </summary>
    [Tooltip("导航路径的起点，通常指定玩家或角色的 Transform。")]
    public Transform player;

    /// <summary>
    /// 计算导航路径时使用的终点。
    /// </summary>
    [Tooltip("导航路径的终点；路径会随该目标的位置更新。")]
    public Transform endPoint;

    /// <summary>
    /// 用于显示导航路径的线渲染器。
    /// </summary>
    [Tooltip("显示导航路径的 LineRenderer；未指定时使用当前物体上的组件。")]
    public LineRenderer line;

    /// <summary>
    /// 线条贴图横向重复一次对应的世界空间宽度。
    /// </summary>
    [Tooltip("导航线贴图重复一次对应的世界空间长度；必须大于 0，用于保持箭头间距稳定。")]
    public float textureWorldWidth = 2f;

    private NavMeshPath path;
    private float textureOffsetX;

    /// <summary>
    /// 创建可复用的 NavMesh 路径缓存，并自动绑定本物体的线渲染器。启用导航前需要先调用。
    /// </summary>
    public void Initialization()
    {
        path = new NavMeshPath();
        if (line == null)
        {
            line = GetComponent<LineRenderer>();
        }
    }

    /// <summary>
    /// 准备用于显示路径的线渲染器，并重置贴图滚动偏移。
    /// </summary>
    public void EnableNavigation()
    {
        if (line == null)
        {
            line = GetComponent<LineRenderer>();
        }

        if (path == null || endPoint == null || line == null)
        {
            return;
        }

        line.alignment = LineAlignment.TransformZ;
        line.transform.eulerAngles = new Vector3(90, 0, 0);
        textureOffsetX = 0f;
        SetLineOffset(0f);
    }

    /// <summary>
    /// 清空当前导航路径显示。
    /// </summary>
    public void DisableNavigation()
    {
        if (line != null)
        {
            line.positionCount = 0;
        }
    }

    /// <summary>
    /// 在必要引用齐全时，每帧重新计算并绘制导航路径。
    /// </summary>
    private void Update()
    {
        if (path == null || line == null || player == null || endPoint == null)
        {
            return;
        }

        NavMesh.CalculatePath(player.position, endPoint.position, NavMesh.AllAreas, path);
        Vector3[] corners = path.corners;
        line.positionCount = corners.Length;
        line.SetPositions(corners);
        textureOffsetX -= Time.deltaTime;
        SetLineOffset(textureOffsetX);
        UpdateLineScale();
    }

    /// <summary>
    /// 根据路径总长度缩放线条材质贴图，保持箭头间距稳定。
    /// </summary>
    private void UpdateLineScale()
    {
        if (line == null || line.sharedMaterial == null || line.positionCount < 2 ||
            textureWorldWidth <= 0f || float.IsNaN(textureWorldWidth) || float.IsInfinity(textureWorldWidth))
        {
            return;
        }

        float totalLength = 0f;
        for (int i = 1; i < line.positionCount; i++)
        {
            totalLength += Vector3.Distance(line.GetPosition(i - 1), line.GetPosition(i));
        }

        float x = totalLength / textureWorldWidth;
        if (!float.IsNaN(x) && !float.IsInfinity(x))
        {
            line.material.mainTextureScale = new Vector2(x, 1f);
        }
    }

    /// <summary>
    /// 设置线条贴图偏移，用于实现路径滚动动画。
    /// </summary>
    private void SetLineOffset(float x, float y = 0f)
    {
        if (line != null && line.sharedMaterial != null)
        {
            line.material.mainTextureOffset = new Vector2(x, y);
        }
    }
}
}
