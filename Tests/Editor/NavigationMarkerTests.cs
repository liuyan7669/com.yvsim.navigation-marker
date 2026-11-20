using NUnit.Framework;
using UnityEngine;
using Marker = Yvsim.NavigationMarker.NavigationMarker;

namespace Yvsim.NavigationMarker.Tests
{
    /// <summary>验证直接添加组件时，初始化能自动取得必需的线渲染器。</summary>
    public sealed class NavigationMarkerTests
    {
        [Test]
        public void AddedComponent_InitializesRequiredRendererWithoutManualAssignmentOrMaterial()
        {
            GameObject owner = new GameObject("Navigation");
            GameObject destination = new GameObject("Destination");
            try
            {
                Marker marker = owner.AddComponent<Marker>();
                marker.endPoint = destination.transform;
                owner.GetComponent<LineRenderer>().sharedMaterial = null;

                Assert.DoesNotThrow(() =>
                {
                    marker.Initialization();
                    marker.EnableNavigation();
                });

                Assert.That(marker.line, Is.SameAs(owner.GetComponent<LineRenderer>()));
                Assert.That(marker.line.alignment, Is.EqualTo(LineAlignment.TransformZ));
            }
            finally
            {
                Object.DestroyImmediate(owner);
                Object.DestroyImmediate(destination);
            }
        }
    }
}
