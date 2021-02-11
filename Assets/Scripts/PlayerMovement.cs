using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController cc;
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    
    private Vector2 direction;
    [SerializeField] private float walkSpeed = 6f;
    private Transform cam;
    
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = Camera.main.transform;
    }

    public void Move(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        if (direction.magnitude >= 0.1f)
        {
            Vector3 projectedCamUp = Vector3.ProjectOnPlane(cam.up, transform.up);
            
            Vector3 moveDir = cam.right * direction.x + projectedCamUp * direction.y;
            
            // Compute the angle towards the direction
            float angle = Mathf.Atan2( moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle,  
                ref turnSmoothVelocity, turnSmoothTime);
            
            transform.localRotation = Quaternion.Euler(0f, smoothAngle, 0f);
            
            if (cc.enabled) cc.Move(moveDir.normalized * walkSpeed * Time.deltaTime);
        }
    }
}
