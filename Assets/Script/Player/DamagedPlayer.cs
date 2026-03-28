using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagedPlayer : MonoBehaviour
{
    Spawner _spawner;
    [SerializeField]
    private GameObject _respawnPoint;
    [SerializeField]
    private LayerMask _layerEnemies;
    [SerializeField]
    private int _pvMax=2;
    private int _pvActual;

    private void Awake()
    {
        _spawner = GetComponentInParent<Spawner>();
        SetPv();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == _layerEnemies)
            Damaged();
    }
    private void Damaged()
    {
        _pvActual--;
        if (_pvActual < 0)
            _spawner.Respawn();
    }
    public void SetPv()
    {
        _pvActual = _pvMax;

    }
}
