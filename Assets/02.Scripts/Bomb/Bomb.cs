using UnityEngine;

public class Bomb : MonoBehaviour
{
    private float _bombCoolTime = 1f;
    public float BombCoolTime => _bombCoolTime;


    private void Update()
    {
        transform.localPosition += transform.right  * 10f * Time.deltaTime;
    }

    // 폭탄 두기기
    public void PlaceBomb(Transform transform)
    {
        Debug.Log($"폭탄 두기 {transform.localPosition} {transform.localEulerAngles.z}");
    }

    // 폭탄 던지기 (곡사)
    public void ThrowBomb(Transform transform)
    {
        Debug.Log($"폭탄 던지기 {transform.localPosition} {transform.localEulerAngles.z}");
    }

    // 폭탄 직선으로 던지기
    public void ThrowBombStraight(Transform transform)
    {
        Debug.Log($"폭탄 직선으로 던지기 {transform.localPosition} {transform.localEulerAngles.z}");
    }

    // 폭탄 부스트
    public void BoostBomb(Transform transform)
    {
        Debug.Log($"폭탄 부스트 {transform.localPosition} {transform.localEulerAngles.z}");
    }

    // 폭탄 내려 찍기
    public void SmashBomb(Transform transform)
    {
        Debug.Log($"폭탄 내려 찍기 {transform.localPosition} {transform.localEulerAngles.z}");
    }

    public void SetLastBombTime()
    {
        _bombCoolTime = Time.time;
    }
}
