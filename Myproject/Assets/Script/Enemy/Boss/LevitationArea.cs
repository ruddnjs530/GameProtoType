using UnityEngine;

/// <summary>
/// 레비테이션 공격의 판정 범위(보스 위치를 꼭짓점으로 하는 부채꼴)와, 그 범위를 바닥에 그리는 Projector 배치를 계산한다.
/// 판정과 표시를 같은 값으로 계산해야 "표시 밖인데 맞는" 문제가 생기지 않는다.
/// </summary>
public static class LevitationArea
{
    // Cone_Indicator-removebg-preview.png(514x486)에서 측정한 부채꼴 형태.
    // 텍스처를 바꾸면 이 값도 다시 측정해야 한다.
    private const float TextureWidth = 514f;
    private const float TextureHeight = 486f;
    private const float ApexU = 0.5029f;       // 꼭짓점의 u(가로) 위치
    private const float ApexV = 0.2027f;       // 꼭짓점의 v(세로) 위치. 부채꼴은 v가 커지는 방향으로 열린다
    private const float RadiusV = 260.25f / TextureHeight; // 반지름을 텍스처 높이 비율로 나타낸 값

    /// <summary>
    /// 대상이 부채꼴 안에 있는지 판정한다. 높이(y)는 무시하고 수평 거리·각도만 본다.
    /// </summary>
    public static bool Contains(Vector3 origin, Vector3 forward, Vector3 target, float range, float angle)
    {
        Vector3 toTarget = target - origin;
        toTarget.y = 0f;
        forward.y = 0f;

        if (toTarget.sqrMagnitude > range * range) return false;
        if (toTarget.sqrMagnitude < 0.0001f) return true; // 보스 바로 아래는 범위 안으로 본다
        if (forward.sqrMagnitude < 0.0001f) return false;

        return Vector3.Angle(forward, toTarget) <= angle * 0.5f;
    }

    /// <summary>
    /// 아래를 향한 직교 Projector를 부채꼴 꼭짓점이 origin에 오고, 반지름이 range가 되도록 배치한다.
    /// Projector의 up 방향이 부채꼴이 열리는 방향이다.
    /// </summary>
    public static void AlignProjector(Projector projector, Vector3 origin, float range)
    {
        float aspect = TextureWidth / TextureHeight; // 텍스처 픽셀이 정사각형으로 투영되도록 가로세로 비율을 맞춘다
        float size = range / (2f * RadiusV);

        projector.orthographic = true;
        projector.aspectRatio = aspect;
        projector.orthographicSize = size;

        Transform t = projector.transform;
        Vector3 apexOffset = t.up * ((ApexV - 0.5f) * 2f * size) + t.right * ((ApexU - 0.5f) * 2f * size * aspect);
        Vector3 center = origin - apexOffset;
        center.y = t.position.y; // 높이는 기존 배치(바닥 위)를 유지한다
        t.position = center;
    }
}
