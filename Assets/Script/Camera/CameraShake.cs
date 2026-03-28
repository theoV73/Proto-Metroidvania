using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;

public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] 
    private float _shakePlayerDamaged = 1.5f;
    [SerializeField] 
    private float _shakePlayerDamagedTime = 1.5f;
    [SerializeField] 
    private float _shakeEnemyDead = 1.5f;
    [SerializeField] 
    private float _shakeEnemyDeadTime = 1.5f;
    void Start()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    [Button]
    public void ShakePlayerDamaged()
    {
        StopAllCoroutines();
        StartCoroutine(Shake(_shakePlayerDamaged, _shakePlayerDamagedTime));
    }
    [Button]
    public void ShakeEnemyExplose()
    {
        StopAllCoroutines();
        StartCoroutine(Shake(_shakeEnemyDead, _shakeEnemyDeadTime));
    }
    IEnumerator Shake(float valueShake, float time)
    {
        _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = valueShake;
        yield return new WaitForSeconds(time);
        _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;

    }
}
