using UnityEngine;

public static class BezierCurve
{
    /// <summary>
    /// 시작점, 끝점, 중간점1, 중간점2, 시간간
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static Vector2 BezierCurve2D(Vector2 start, Vector2 end, Vector2 p1, Vector2 p2, float time)
    {
        Vector2 a = Vector2.Lerp(start, p1, time);
        Vector2 b = Vector2.Lerp(p1, p2, time);
        Vector2 c = Vector2.Lerp(p2, end, time);

        Vector2 ab = Vector2.Lerp(a, b, time);
        Vector2 bc = Vector2.Lerp(b, c, time);

        Vector2 abbc = Vector2.Lerp(ab, bc, time);

        return abbc;
    }

    public static Vector3 BezierCurve3D(Vector3 start, Vector3 end, Vector3 p1, Vector3 p2, float time)
    {
        Vector3 a = Vector3.Lerp(start, p1, time);
        Vector3 b = Vector3.Lerp(p1, p2, time);
        Vector3 c = Vector3.Lerp(p2, end, time);

        Vector3 ab = Vector3.Lerp(a, b, time);
        Vector3 bc = Vector3.Lerp(b, c, time);

        Vector3 abbc = Vector3.Lerp(ab, bc, time);

        return abbc;
    } 
}
