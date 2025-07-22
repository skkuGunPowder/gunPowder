using UnityEngine;

public class GunPowderTest : MonoBehaviour
{
    public GameObject _gunPowderPrefab;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            // 마우스 위치에 생성
            //Instantiate(_gunPowderPrefab, Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);

            // 마우스 위치에 takedamage 호출
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<Player>().TakeDamage(5, Camera.main.ScreenToWorldPoint(Input.mousePosition), false);
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            for(int i=0; i<10; i++)
            {
                Instantiate(_gunPowderPrefab, transform.position, Quaternion.identity);
            }
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<PlayerFSM>().ChangeState<PlayerDropDeadState>();
        }
    }
}
