using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    [SerializeField] private string _dataPath = "Assets/13.CSV/test/testCSV.CSV";
    private List<testCSV> _dataList;

    private void Awake()
    {
        _dataList = CSVReader.Read<testCSV>(_dataPath);

        foreach (testCSV testdata in _dataList)
        {
            Debug.Log($"ID: {testdata.ID}");
            Debug.Log($"Attack: {testdata.Attack}");
            Debug.Log($"Cost: {testdata.cost}");
            Debug.Log($"Priority: {testdata.priority}");
            Debug.Log($"Gimmic: {testdata.gimmic}");
            Debug.Log($"Command: {testdata.Command}");
        }
    }
}
