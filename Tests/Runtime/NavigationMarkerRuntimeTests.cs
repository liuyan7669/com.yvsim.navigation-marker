using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Marker = Yvsim.NavigationMarker.NavigationMarker;

namespace Yvsim.NavigationMarker.Tests
{
    /// <summary>在播放模式中验证实际导航路径及材质实例的运行时更新。</summary>
    public sealed class NavigationMarkerRuntimeTests
    {
        private GameObject owner;
        private GameObject player;
        private GameObject destination;
        private Material material;
        private NavMeshData navigationData;
        private NavMeshDataInstance navigationInstance;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            navigationInstance.Remove();
            if (owner != null)
            {
                Material instance = owner.GetComponent<LineRenderer>().sharedMaterial;
                if (instance != null && instance != material)
                {
                    Object.Destroy(instance);
                }
            }
            Object.Destroy(owner);
            Object.Destroy(player);
            Object.Destroy(destination);
            Object.Destroy(material);
            Object.Destroy(navigationData);
            yield return null;
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidTextureWidth_PreservesFiniteTilingAndStillUpdatesNavigation(float width)
        {
            NavMeshBuildSource floor = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Box,
                size = new Vector3(20f, 0.2f, 20f),
                transform = Matrix4x4.TRS(new Vector3(0f, -0.1f, 0f), Quaternion.identity, Vector3.one)
            };
            navigationData = NavMeshBuilder.BuildNavMeshData(
                NavMesh.GetSettingsByIndex(0), new List<NavMeshBuildSource> { floor },
                new Bounds(Vector3.zero, new Vector3(25f, 5f, 25f)), Vector3.zero, Quaternion.identity);
            Assert.That(navigationData, Is.Not.Null);
            navigationInstance = NavMesh.AddNavMeshData(navigationData);

            owner = new GameObject("Navigation");
            player = new GameObject("Player");
            destination = new GameObject("Destination");
            destination.transform.position = new Vector3(5f, 0f, 0f);
            Marker marker = owner.AddComponent<Marker>();
            marker.player = player.transform;
            marker.endPoint = destination.transform;
            material = new Material(Shader.Find("Sprites/Default"));
            owner.GetComponent<LineRenderer>().sharedMaterial = material;
            marker.Initialization();
            marker.EnableNavigation();
            MethodInfo update = typeof(Marker).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            update.Invoke(marker, null);
            Assert.That(marker.line.positionCount, Is.GreaterThanOrEqualTo(2), "A usable path must be present before testing texture parameters.");
            Vector2 previousScale = marker.line.sharedMaterial.mainTextureScale;

            marker.textureWorldWidth = width;
            Assert.DoesNotThrow(() => update.Invoke(marker, null));

            Assert.That(marker.line.positionCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(marker.line.sharedMaterial.mainTextureScale, Is.EqualTo(previousScale));
            Assert.That(float.IsNaN(previousScale.x) || float.IsInfinity(previousScale.x), Is.False);
        }
    }
}
