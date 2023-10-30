using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FloorDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _floorViewTMP;
    [SerializeField] private Image _directionImage;

    [SerializeField] private ElevatorMotion _elevator;

    private void Awake()
    {
        _directionImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        _floorViewTMP.text = _elevator.CurrentPosition.ToString();
        _elevator.DirectionChanged += OnElevatorDirectionChanged;
        _elevator.PositionChanged += OnElevatorPositionChanged;
    }

    private void OnElevatorPositionChanged()
    {
        _floorViewTMP.text = _elevator.CurrentPosition.ToString();
    }

    private void OnElevatorDirectionChanged()
    {
        switch (_elevator.CurrentDirection)
        {
            case MoveDirection.None:
                _directionImage.gameObject.SetActive(false);
                break;
            case MoveDirection.Up:
                _directionImage.rectTransform.eulerAngles = Vector3.zero;
                _directionImage.gameObject.SetActive(true);
                break;
            case MoveDirection.Down:
                _directionImage.rectTransform.eulerAngles = Vector3.forward * 180f;
                _directionImage.gameObject.SetActive(true);
                break;
        }
    }
}
