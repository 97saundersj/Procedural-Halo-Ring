using UnityEngine;

public class ToggleActiveObject : MonoBehaviour
{
    // Start is called before the first frame update
    public void ToggleActive()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
