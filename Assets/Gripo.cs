using UnityEngine;

public class Gripo : MonoBehaviour
{
    public float interactRange = 2.5f;
    public Camera cam;

    private Animator animator;
    private bool useFirst = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            Debug.Log("Ray hit: " + hit.collider.gameObject.name);

            if (hit.collider.gameObject == gameObject)
            {
                PlayGripAnimation();
            }
        }
        else
        {
            Debug.Log("Ray hit nothing");
        }
    }

    void PlayGripAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("GripAction: No Animator assigned!");
            return;
        }

        string animToPlay = useFirst ? "gripo bukas" : "gripo sarado";
        animator.Play(animToPlay, 0, 0f);
        useFirst = !useFirst;
    }
}