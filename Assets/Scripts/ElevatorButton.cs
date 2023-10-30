using TMPro;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    private ElevatorMotion _elevator;
    private bool _isEnabled;
    private byte _floorNum;

    [SerializeField] private TextMeshProUGUI _buttonName;

    private void Awake()
    {
        _elevator = GameObject.Find("Elevator").GetComponent<ElevatorMotion>();
        _isEnabled = false;
        _floorNum = byte.Parse(_buttonName.text);
    }

    public void OnElevatorButtonClick()
    {
        if (_isEnabled)
            return;
        _isEnabled = true;
        _buttonName.color = Color.red;
        _elevator.DirectionChanged += OnElevatorDirectionChanged;
        _elevator.InnerCalling(_floorNum);
    }

    private void OnElevatorDirectionChanged()
    {
        if (_elevator.CurrentDirection == MoveDirection.None && _elevator.CurrentPosition == _floorNum)
        {
            _buttonName.color = Color.white;
            _elevator.DirectionChanged -= OnElevatorDirectionChanged;
            _isEnabled = false;
        }
    }
}
