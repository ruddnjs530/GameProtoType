using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// 게임 스크립트가 Assembly-CSharp에 있어 테스트 어셈블리에서 직접 참조할 수 없으므로 리플렉션으로 호출한다(TurretTargetingTests와 같은 방식).
public class LevitationAreaTests
{
    private const float Range = 20f;
    private const float Angle = 90f;
    private MethodInfo contains;

    [SetUp]
    public void SetUp()
    {
        Type area = Assembly.Load("Assembly-CSharp").GetType("LevitationArea", true);
        contains = area.GetMethod("Contains", BindingFlags.Public | BindingFlags.Static);
    }

    private bool Contains(Vector3 target, Vector3 forward)
    {
        return (bool)contains.Invoke(null, new object[] { Vector3.zero, forward, target, Range, Angle });
    }

    [Test]
    public void InsideRangeAndAngle_IsHit()
    {
        Assert.IsTrue(Contains(new Vector3(0, 0, 10), Vector3.forward));
        Assert.IsTrue(Contains(new Vector3(7, 0, 10), Vector3.forward)); // 약 35°
    }

    [Test]
    public void BeyondRange_IsNotHit()
    {
        Assert.IsFalse(Contains(new Vector3(0, 0, 20.5f), Vector3.forward));
    }

    [Test]
    public void OutsideAngle_IsNotHit()
    {
        Assert.IsFalse(Contains(new Vector3(10, 0, 8), Vector3.forward));  // 약 51°
        Assert.IsFalse(Contains(new Vector3(0, 0, -5), Vector3.forward));  // 뒤쪽
    }

    [Test]
    public void HeightIsIgnored()
    {
        // 점프 중이어도 수평 위치가 범위 안이면 맞는다.
        Assert.IsTrue(Contains(new Vector3(0, 6, 10), Vector3.forward));
        // 보스 전방 벡터가 약간 기울어져 있어도 수평 성분으로 판정한다.
        Assert.IsTrue(Contains(new Vector3(0, 0, 10), new Vector3(0, -0.3f, 1)));
    }

    [Test]
    public void BoundaryValues_AreHit()
    {
        Assert.IsTrue(Contains(new Vector3(0, 0, 20f), Vector3.forward));
        Assert.IsTrue(Contains(new Vector3(5, 0, 5), Vector3.forward)); // 정확히 45°
    }
}
