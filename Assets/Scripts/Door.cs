using UnityEngine;

public class Door : MonoBehaviour
{
    private BlackboardController bc;
    [SerializeField] private string blackboardEventName;
    
    private Vector3 closedPos;
    private Vector3 openPos;
    private Vector3 goal;
    private bool isOpen;
    
    // Start is called before the first frame update
    private void Start()
    {
        closedPos = transform.position;
        openPos = closedPos - new Vector3(2f, 0f, 0f);
        bc = GameObject.FindWithTag("BlackboardController").GetComponent<BlackboardController>();
        
        bc.StartListening(blackboardEventName, OnDoorToggle);
        
        OnDoorToggle(bc.GetBlackboardValue(blackboardEventName));
    }

    // Update is called once per frame
    private void Update()
    {
        if (Vector3.Distance(goal, transform.position) <= 2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, goal, 0.1f);
        }
    }

    private void OnDoorToggle(BlackboardVariable data)
    {
        isOpen = ((BoolVariable) data).value;
        
        UpdateGoal();
    }

    private void UpdateGoal()
    {
        if (isOpen) goal = openPos;
        else goal = closedPos;
    }

    private void OnDestroy()
    {
        bc.StopListening(blackboardEventName, OnDoorToggle);
    }
}