using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorButton : MonoBehaviour
{
    private ElevatorMotion _elevator;
    private byte _floorNum;
    private bool _isActive;

    [SerializeField] private Image _buttonImg;
    [SerializeField] private TextMeshProUGUI _floorName;
    [SerializeField] private MoveDirection _moveDirection;
    [SerializeField] private Sprite _innactiveStateImg;
    [SerializeField] private Sprite _activeSateImg;

    private void Awake()
    {
        _elevator = GameObject.Find("Elevator").GetComponent<ElevatorMotion>();
        _floorNum = byte.Parse(_floorName.text);
        _isActive = false;
    }

    public void OnFloorButtonClick()
    {
        if (_isActive)
            return;

        _isActive = true;
        _buttonImg.sprite = _activeSateImg;
        _elevator.DirectionChanged += OnElevatorDirectionChanged;
        _elevator.OuterCalling(_floorNum, _moveDirection);
    }

    private void OnElevatorDirectionChanged()
    {
        if (_elevator.CurrentDirection == MoveDirection.None && _elevator.CurrentPosition == _floorNum && _elevator.TargetDirection == _moveDirection)
        {
            _buttonImg.sprite = _innactiveStateImg;
            _elevator.DirectionChanged -= OnElevatorDirectionChanged;
            _isActive = false;
        }
    }
}
