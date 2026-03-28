using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EnvironmentObject : MonoBehaviour
{
    [SerializeField]
    private ListObjects _objet;
    [SerializeField]
    private List<Vector2> checkpoints;
    [SerializeField]
    private List<float> _time;
    [SerializeField]
    private bool _mobile;
    [SerializeField]
    private bool _loop = true;
    [SerializeField]
    private float time;
    private float actualtime;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_objet == ListObjects.DestructibleObject && collision.gameObject.layer == 10 || _objet == ListObjects.Enemies && collision.gameObject.layer == 10)
        {
            collision.gameObject.GetComponentInParent<PlayerJump>().OnJumpStarted();
            this.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        switch(_objet)
        {
            case ListObjects.DestructibleObject:
                gameObject.layer = 13;
                break;
            case ListObjects.Trap:
                gameObject.layer = 11;
                break;
            case ListObjects.Enemies:
                gameObject.layer = 12;
                if(_mobile)
                    StartCoroutine(Movement());
                break;
            case ListObjects.MovementPlatform:
                StartCoroutine(Movement());
                break;
        }
            
    }
    
    private IEnumerator Movement()
    {
        for (int i = 0; i < checkpoints.Count-1; i++)
        {
            actualtime = 0;
            while (actualtime < _time[i]) //suit les points dans l'ordre -> aller
            {
                actualtime += Time.deltaTime;
                gameObject.transform.position = Vector2.Lerp(checkpoints[i], checkpoints[i + 1], actualtime / _time[i]);
                yield return null;
            }
        }
        for (int i = checkpoints.Count-1; i > 0; i--)
        {
            actualtime = 0;
            while (actualtime < _time[i - 1]) //suit les points dans le sens inverse ->retour
            {
                actualtime += Time.deltaTime;
                gameObject.transform.position = Vector2.Lerp(checkpoints[i], checkpoints[i - 1], actualtime / _time[i-1]);
                yield return null;
            }
        }
        if(_loop)
            StartCoroutine(Movement());
    }
}
