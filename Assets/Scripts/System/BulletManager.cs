using TMPro;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance;

    public int MaxBullets = 2;
    private int _currentBullets;

    public TextMeshProUGUI BulletText;

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

    void Start()
    {
        _currentBullets = MaxBullets;
        UpdateBulletUI();
    }

    public void Shoot()
    {
        if (_currentBullets > 0)
        {
            _currentBullets--;
            UpdateBulletUI();
        }
    }

    public bool HasBullets()
    {
        return _currentBullets > 0;
    }

    private void UpdateBulletUI()
    {
        BulletText.text = $"X {_currentBullets}";
    }


}
