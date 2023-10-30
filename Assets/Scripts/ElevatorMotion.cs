using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum MoveDirection
{
    None, Up, Down 
}

public class ElevatorMotion : MonoBehaviour
{
    private byte _currentPosition;
    private byte _targetFloor;
    private byte _tempTarget;
    private MoveDirection _currentDirection;
    private MoveDirection _targetDirection;
    private int _speed;
    private bool _isTargetChanged;
    private bool _canMove;
    private bool _isOptionalMovement;
    private float _timer;

    private List<byte> _targetFloors;
    private List<FloorQuery> _waitingQuerys;

    public byte CurrentPosition => _currentPosition;
    public MoveDirection CurrentDirection => _currentDirection;
    public MoveDirection TargetDirection => _targetDirection;

    public event Action PositionChanged;
    public event Action DirectionChanged;

    private class FloorQuery
    {
        public byte Floor { get; private set; }
        public MoveDirection Direction { get; private set; }

        public FloorQuery(byte floor, MoveDirection direction)
        {
            Floor = floor;
            Direction = direction;
        }
    }

    private void Awake()
    {
        _currentPosition = 1;
        _currentDirection = _targetDirection = MoveDirection.None;
        _speed = 2000;
        _isTargetChanged = false;
        _canMove = true;
        _isOptionalMovement = false;
        _timer = 0f;
        _targetFloors = new List<byte>();
        _waitingQuerys = new List<FloorQuery>();
    }

    private void Update()
    {
        if (_targetFloor != _tempTarget && _isTargetChanged == false)
        {
            _isTargetChanged = true;
        }

        if (_targetFloor != _tempTarget && _canMove == true)
        {
            _isTargetChanged = false;
            _canMove = false;
            _tempTarget = _targetFloor;
            Move(_targetFloor);
        }

        if (_currentDirection == MoveDirection.None && _targetFloors.Count > 0)
        {
            _targetFloor = SelectTarget();
            Move(_targetFloor);
        }
        else if (_currentDirection == MoveDirection.None && _targetFloors.Count == 0 && _waitingQuerys.Count > 0)
        {
            SwitchDirection();
            FillTargetList();

            if (_targetFloors.Count > 0)
            {
                CleanWaiting();
                _targetFloor = SelectTarget();
            }
            else
            {
                SwitchDirection();
                FillTargetList();
                CleanWaiting();
                _targetFloor = SelectTarget(true);
            }
        }

        if (_currentDirection == MoveDirection.None && _targetFloors.Count == 0 && _waitingQuerys.Count == 0 && _currentPosition != 1)
        {
            _timer += Time.deltaTime;
            if (_timer > 5f)
            {
                _isOptionalMovement = true;
                _timer = 0;
                _targetDirection = MoveDirection.Down;
                _targetFloor = 1;
                _targetFloors.Add(_targetFloor);
            }
        }
        else
            _timer = 0;
    }

    private void FillTargetList()
    {
        foreach (var item in _waitingQuerys)
        {
            if (item.Direction == _targetDirection)
                _targetFloors.Add(item.Floor);
        }
    }

    private void CleanWaiting()
    {
        for (int i = 0; i < _waitingQuerys.Count; i++)
        {
            if (_targetFloors.Contains(_waitingQuerys[i].Floor) && _waitingQuerys[i].Direction == _targetDirection)
                _waitingQuerys.Remove(_waitingQuerys[i]);
        }
    }

    private void SwitchDirection()
    {
        switch (_targetDirection)
        {
            case MoveDirection.Up:
                _targetDirection = MoveDirection.Down;
                break;
            case MoveDirection.Down:
                _targetDirection = MoveDirection.Up;
                break;
        }
    }

    public void InnerCalling(byte floorNum)
    {
        if (_targetFloors.Contains(floorNum))
                return;

        MoveDirection direction;
        if (_currentPosition < floorNum)
            direction = MoveDirection.Up;
        else
            direction = MoveDirection.Down;
        if (_targetFloors.Count == 0)
        {
            _targetDirection = direction;
            _targetFloor = floorNum;
            _targetFloors.Add(floorNum);
            return;
        }

        if (_isOptionalMovement)
        {
            _isOptionalMovement = false;
            _targetFloors.Clear();
            _targetDirection = direction;
            _targetFloor = floorNum;
            _targetFloors.Add(floorNum);
            return;
        }

        if (_currentDirection == direction || _currentDirection == MoveDirection.None)
        {
            _targetFloors.Add(floorNum);
            _targetFloor = SelectTarget();
            return;
        }

        for (int j = 0; j < _waitingQuerys.Count; j++)
        {
            if (_waitingQuerys[j].Floor == floorNum && _waitingQuerys[j].Direction == direction)
                return;
        }
        _waitingQuerys.Add(new FloorQuery(floorNum, direction));

    }

    public void OuterCalling(byte floorNum, MoveDirection direction)
    {
        if (_targetFloors.Count == 0)
        {
            _targetDirection = direction;
            _targetFloor = floorNum;
            _targetFloors.Add(floorNum);
            return;
        }

        if (_isOptionalMovement)
        {
            _isOptionalMovement = false;
            _targetFloors.Clear();
            _targetDirection = direction;
            _targetFloor = floorNum;
            _targetFloors.Add(floorNum);
            return;
        }


        if ((_currentDirection == _targetDirection && direction != _currentDirection) || _targetDirection != direction)
        {
            for (int j = 0; j < _waitingQuerys.Count; j++)
            {
                if (_waitingQuerys[j].Floor == floorNum && _waitingQuerys[j].Direction == direction)
                    return;
            }
            _waitingQuerys.Add(new FloorQuery(floorNum, direction));
            
            return;
        }

        if (_targetDirection == _currentDirection)
        {
            if (_targetFloors.Count > 0 && _targetFloors.Contains(floorNum))
                return;

            switch (direction)
            {
                case MoveDirection.Up:
                    if (_currentPosition > floorNum)
                    {
                        for (int j = 0; j < _waitingQuerys.Count; j++)
                        {
                            if (_waitingQuerys[j].Floor == floorNum && _waitingQuerys[j].Direction == direction)
                                return;
                        }
                        _waitingQuerys.Add(new FloorQuery(floorNum, direction));

                        return;
                    }
                    break;

                case MoveDirection.Down:
                    if (_currentPosition < floorNum)
                    {
                        for (int j = 0; j < _waitingQuerys.Count; j++)
                        {
                            if (_waitingQuerys[j].Floor == floorNum && _waitingQuerys[j].Direction == direction)
                                return;
                        }
                        _waitingQuerys.Add(new FloorQuery(floorNum, direction));

                        return;
                    }
                    break;
            }
        }

        _targetFloors.Add(floorNum);

        if (_targetDirection != _currentDirection && _targetFloors.Count > 1)
        {
            _targetFloor = SelectTarget(true);
            return;
        }

        _targetFloor = SelectTarget();
    }

    private byte SelectTarget(bool isReverseSelect = false)
    {
        byte floorNum = default;
        if (isReverseSelect)
        {
            var maxStepsCount = 0;
            for (byte i = 0; i < _targetFloors.Count; i++)
            {
                var stepsCount = Mathf.Abs(_currentPosition - _targetFloors[i]);
                if (stepsCount > maxStepsCount)
                {
                    maxStepsCount = stepsCount;
                    floorNum = _targetFloors[i];
                }
            }
            return floorNum;
        }

        var minStepsCount = 6;
        for (byte i = 0; i < _targetFloors.Count; i++)
        {
            var stepsCount = Mathf.Abs(_currentPosition - _targetFloors[i]);
            if (stepsCount < minStepsCount)
            {
                minStepsCount = stepsCount;
                floorNum = _targetFloors[i];
            }
        }
        return floorNum;
    }

    private async void Move(byte targetFloor)
    {
        var floorOffset = Mathf.Abs(targetFloor - _currentPosition);

        for (int i = 0; i < floorOffset; i++)
        {
            if (_isTargetChanged)
            {
                _canMove = true;
                _isTargetChanged = false;
                return;
            }

            if (_currentPosition < targetFloor)
            {
                if (_currentDirection != MoveDirection.Up)
                    ChangeDirection(MoveDirection.Up);

                _currentPosition++;
            }
            else if (_currentPosition > targetFloor)
            {
                if (_currentDirection != MoveDirection.Down)
                    ChangeDirection(MoveDirection.Down);

                _currentPosition--;
            }
            
            await Task.Delay(_speed);
            PositionChanged?.Invoke();
        }
        Debug.Log("Done at: " + targetFloor);
        _targetFloors.Remove(targetFloor);
        ChangeDirection(MoveDirection.None);
        DirectionChanged?.Invoke();

    }

    private void ChangeDirection(MoveDirection direction)
    {
        _currentDirection = direction;
        DirectionChanged?.Invoke();
    }
}
