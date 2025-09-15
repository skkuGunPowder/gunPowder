using UnityEngine;

public class TurnOnTutorialTV : MonoBehaviour
{
    [SerializeField] private bool _isOn = false;
    [SerializeField] private GameObject _tutorialTV;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isOn)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            _isOn = true;
            _tutorialTV.SetActive(true);
        }
    }
}
