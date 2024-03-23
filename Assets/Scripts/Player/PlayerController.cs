using MicroJam10.SO;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MicroJam10.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private PlayerInfo _info;

        [SerializeField]
        private Transform _cam;

        private Vector2 _mov;
        private float _verticalSpeed;
        private CharacterController _controller;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void FixedUpdate()
        {
            var pos = _mov;
            Vector3 desiredMove = _cam.transform.forward * pos.y + _cam.transform.right * pos.x;

            // Get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, _controller.radius, Vector3.down, out RaycastHit hitInfo,
                               _controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            Vector3 moveDir = Vector3.zero;
            moveDir.x = desiredMove.x * _info.ForceMultiplier;
            moveDir.z = desiredMove.z * _info.ForceMultiplier;

            if (_controller.isGrounded && _verticalSpeed < 0f) // We are on the ground and not jumping
            {
                moveDir.y = -.1f; // Stick to the ground
                _verticalSpeed = -_info.GravityMultiplicator;
            }
            else
            {
                // We are currently jumping, reduce our jump velocity by gravity and apply it
                _verticalSpeed += Physics.gravity.y * _info.GravityMultiplicator;
                moveDir.y += _verticalSpeed;
            }

            var p = transform.position;
            _controller.Move(moveDir);
        }

        public void OnMove(InputAction.CallbackContext value)
        {
            _mov = value.ReadValue<Vector2>();
        }

        private RaycastHit? GetInteractionTarget()
        {
            if (Physics.Raycast(new Ray(_cam.position, _cam.forward), out RaycastHit interInfo, 100f, ~(1 << LayerMask.GetMask("Player"))))
            {
                return interInfo;
            }
            return null;
        }

        private void OnDrawGizmos()
        {
            var target = GetInteractionTarget();
            if (target != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(_cam.transform.position, target.Value.point);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_cam.transform.position, _cam.transform.position + _cam.transform.forward * 10f);
            }
        }
    }
}