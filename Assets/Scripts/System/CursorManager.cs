using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance; // 싱글톤
    public Texture2D GunTexture;
    public Vector2 Hotspot = Vector2.zero;
    private bool _isOriginalCursor = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SwitchCursor()
    {
        if (_isOriginalCursor)
        {
            SetGunCursor();
        }
        else
        {
            SetOriginalCursor();
        }
    }

    public bool IsGunCursor()
    {
        return !_isOriginalCursor;
    }
    public void SetGunCursor()
    {
        Cursor.SetCursor(GunTexture, Hotspot, CursorMode.ForceSoftware);
        _isOriginalCursor = false;
    }

    public void SetOriginalCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        _isOriginalCursor = true;
    }

    void Start()
    {

    }

    void Update()
    {

    }    
}

