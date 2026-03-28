using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private DamagedPlayer _damagedPlayer;
    [SerializeField]
    private GameObject _respawnPoint;
    [SerializeField]
    private LayerMask _layerRespawn;

    private void Awake()
    {
        _damagedPlayer = GetComponent<DamagedPlayer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == _layerRespawn)
            Respawn();
    }
    public void Respawn()
    {
        gameObject.transform.position = _respawnPoint.transform.position;
        _damagedPlayer.SetPv();
    }

}
