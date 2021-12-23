using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private enum ObstacleTypeEnum
    {
        Fixed,Rotate,Move
    }
    private int _sideSwitch = 1;
    private Vector3 _startPos;
    [SerializeField] private ObstacleTypeEnum obstacleType;
    private void Start()
    {
        _startPos = transform.position;
    }
    private void FixedUpdate()
    {
        switch (obstacleType)
        {
            case ObstacleTypeEnum.Rotate:
                var rot = Quaternion.AngleAxis(20 * Time.fixedDeltaTime, transform.right*_sideSwitch) * transform.rotation;
                var rotX = rot.eulerAngles.x;
                if (rotX < 180 && rotX > 45 ) _sideSwitch = -1;
                if (rotX > 180 && rotX < 315) _sideSwitch = 1;
                transform.rotation = rot;
                break;
            case ObstacleTypeEnum.Move:
                var pos = transform.position;
                if ((_startPos - pos).magnitude > 3f) _sideSwitch *= -1;
                pos += transform.forward * _sideSwitch * Time.fixedDeltaTime;
                transform.position = pos;
                break;
        }
        
    }
}
