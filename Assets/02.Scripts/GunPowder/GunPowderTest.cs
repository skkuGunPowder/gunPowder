using UnityEngine;

public class GunPowderTest : MonoBehaviour
{
    public GameObject _gunPowderPrefab;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            // 마우스 위치에 생성
            Instantiate(_gunPowderPrefab, Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            for(int i=0; i<10; i++)
            {
                Instantiate(_gunPowderPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
