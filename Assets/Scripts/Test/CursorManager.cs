using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D GunTexture;
    public Texture2D OriginalTexture;
    public Vector2 Hotspot = Vector2.zero;
    private bool _isOriginal = true;

    public void SwitchCursor()
    {
        if (_isOriginal)
        {
            SetGunCursor();
        }
        else
        {
            SetOriginalCursor();
        }
    }

    public void SetGunCursor()
    {
        Cursor.SetCursor(GunTexture, Hotspot, CursorMode.ForceSoftware);
        _isOriginal = false;
    }

    public void SetOriginalCursor()
    {
        Cursor.SetCursor(OriginalTexture, Hotspot, CursorMode.ForceSoftware);
        _isOriginal = true;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
