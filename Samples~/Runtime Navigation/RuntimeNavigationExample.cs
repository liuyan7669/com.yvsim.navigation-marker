using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Yvsim.NavigationMarker.Samples
{
    /// <summary>在运行时创建独立的寻路区域，演示导航线初始化、目标切换和显示控制。</summary>
    public sealed class RuntimeNavigationExample : MonoBehaviour
    {
        [SerializeField, Tooltip("终点沿右侧区域往返移动的速度，单位为米/秒；设为 0 时保持当前终点。"), Min(0f)]
        private float targetSpeed = 2f;

        [SerializeField, Tooltip("导航线箭头重复一次对应的世界空间长度，单位为米；运行时调整可观察箭头间距。"), Min(0.1f)]
        private float arrowSpacing = 1.5f;

        private GameObject ownedRoot;
        private NavigationMarker marker;
        private Transform startPoint;
        private Transform destination;
        private Camera previewCamera;
        private Material lineMaterial;
        private NavMeshData navMeshData;
        private NavMeshDataInstance navMeshInstance;
        private Vector3 origin;
        private float movement;
        private bool moveTarget = true;
        private bool navigationVisible = true;
        private string status;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            origin = transform.position;
            movement = 0f;
            moveTarget = true;
            navigationVisible = true;
            if (NavMesh.GetSettingsCount() == 0)
            {
                status = "No NavMesh agent settings. Add an agent type before running.";
                return;
            }

            GameObject prefab = Resources.Load<GameObject>("NavigationMarker");
            if (prefab == null)
            {
                status = "The package NavigationMarker prefab was not found.";
                return;
            }

            ownedRoot = new GameObject("Runtime Navigation Sample (Generated)");
            ownedRoot.transform.SetParent(transform, false);
            var sources = new List<NavMeshBuildSource>
            {
                new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Box,
                    size = new Vector3(18f, 0.2f, 18f),
                    transform = Matrix4x4.TRS(new Vector3(0f, -0.1f, 0f), Quaternion.identity, Vector3.one)
                },
                new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Box,
                    size = new Vector3(4f, 2f, 10f),
                    transform = Matrix4x4.TRS(new Vector3(0f, 1f, 0f), Quaternion.identity, Vector3.one),
                    area = 1 // Unity 内置的 Not Walkable 区域，使路径绕过中间长方形。
                }
            };
            navMeshData = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0), sources,
                new Bounds(Vector3.up, new Vector3(22f, 6f, 22f)), Vector3.zero, Quaternion.identity);
            if (navMeshData == null)
            {
                status = "NavMesh generation failed. Check the Console.";
                return;
            }
            navMeshInstance = NavMesh.AddNavMeshData(navMeshData, origin, Quaternion.identity);

            startPoint = CreatePoint("Start", new Vector3(-7f, 0f, -6f));
            destination = CreatePoint("Destination", new Vector3(7f, 0f, 6f));
            GameObject markerObject = Instantiate(prefab, ownedRoot.transform);
            marker = markerObject.GetComponent<NavigationMarker>();
            marker.player = startPoint;
            marker.endPoint = destination;
            marker.Initialization();
            marker.line.widthMultiplier = 0.6f;
            marker.line.useWorldSpace = true;
            // material 返回实例，修改和销毁此副本不会改动包内材质资源。
            lineMaterial = marker.line.material;
            Shader unlitShader = Shader.Find("Sprites/Default");
            if (unlitShader != null)
            {
                lineMaterial.shader = unlitShader;
            }
            marker.textureWorldWidth = Mathf.Max(0.1f, arrowSpacing);
            marker.EnableNavigation();

            previewCamera = new GameObject("Sample Camera", typeof(Camera)).GetComponent<Camera>();
            previewCamera.transform.SetParent(ownedRoot.transform, false);
            previewCamera.transform.position = origin + Vector3.up * 20f;
            previewCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = 11f;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
            previewCamera.depth = 100f;
            status = "The line bends around the non-walkable center area.";
        }

        private Transform CreatePoint(string objectName, Vector3 localPosition)
        {
            var point = new GameObject(objectName).transform;
            point.SetParent(ownedRoot.transform, false);
            point.position = origin + localPosition;
            return point;
        }

        private void Update()
        {
            if (marker == null) return;
            marker.textureWorldWidth = Mathf.Max(0.1f, arrowSpacing);
            if (moveTarget)
            {
                movement += Mathf.Max(0f, targetSpeed) * Time.deltaTime;
                destination.position = origin + new Vector3(7f, 0f, 6f - Mathf.PingPong(movement, 12f));
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 380f, 210f), GUI.skin.box);
            GUILayout.Label("Navigation Marker / Runtime Navigation");
            GUILayout.Label(status);
            if (marker != null)
            {
                GUILayout.Label("Path corners: " + marker.line.positionCount);
                moveTarget = GUILayout.Toggle(moveTarget, "Move destination");
                if (GUILayout.Button("Switch destination (near / far)"))
                {
                    moveTarget = false;
                    float z = destination.position.z - origin.z;
                    destination.position = origin + new Vector3(7f, 0f, z > 0f ? -6f : 6f);
                }
                if (GUILayout.Button(navigationVisible ? "Hide navigation" : "Show navigation"))
                {
                    navigationVisible = !navigationVisible;
                    // DisableNavigation 只清空当前线条；同时停用组件才能阻止下一帧重绘。
                    marker.enabled = navigationVisible;
                    if (navigationVisible) marker.EnableNavigation();
                    else marker.DisableNavigation();
                }
            }
            GUILayout.EndArea();
            DrawPointLabel(startPoint, "START");
            DrawPointLabel(destination, "DESTINATION");
        }

        private void DrawPointLabel(Transform point, string label)
        {
            if (point == null || previewCamera == null) return;
            Vector3 screen = previewCamera.WorldToScreenPoint(point.position);
            GUI.Label(new Rect(screen.x - 45f, Screen.height - screen.y - 26f, 140f, 24f), label);
        }

        private void OnDisable()
        {
            if (navMeshInstance.valid) navMeshInstance.Remove();
            if (ownedRoot != null)
            {
                ownedRoot.SetActive(false);
                Destroy(ownedRoot);
            }
            if (lineMaterial != null) Destroy(lineMaterial);
            if (navMeshData != null) Destroy(navMeshData);
            ownedRoot = null;
            marker = null;
            startPoint = null;
            destination = null;
            previewCamera = null;
            lineMaterial = null;
            navMeshData = null;
        }
    }
}
