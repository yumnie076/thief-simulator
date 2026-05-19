using UnityEngine;

/// <summary>
/// Allows the player to place ecosystem items using the Spacebar or Left Mouse Click.
/// </summary>
public class PlayerAction : MonoBehaviour
{
    private void Update()
    {
        // When space is pressed, place the selected item at the player's feet
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InteractWithCurrentPosition();
        }
        // When left mouse button is clicked, place at mouse position
        else if (Input.GetMouseButtonDown(0))
        {
            // Do not place if clicking on a UI element (e.g. the toolbar or buttons!)
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            InteractWithMousePosition();
        }
    }

    private void InteractWithCurrentPosition()
    {
        if (GardenManager.Instance == null) return;

        // Player's world position
        Vector3 pos = transform.position;
        
        // Place the tool slightly below the player's center (at their feet)
        Vector3 placePos = new Vector3(pos.x, pos.y - 0.5f, 0f);

        GardenManager.Instance.PlaceToolAt(placePos);
    }

    private void InteractWithMousePosition()
    {
        if (GardenManager.Instance == null || Camera.main == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 placePos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0f);

        // Clamp to garden boundaries
        placePos.x = Mathf.Clamp(placePos.x, 0.5f, GardenManager.Instance.GardenWidth - 0.5f);
        placePos.y = Mathf.Clamp(placePos.y, 0.5f, GardenManager.Instance.GardenHeight - 0.5f);

        GardenManager.Instance.PlaceToolAt(placePos);
    }
}
