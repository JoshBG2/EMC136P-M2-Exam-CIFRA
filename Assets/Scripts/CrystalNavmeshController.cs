using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalNavmeshController : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.AI.NavMeshAgent agent;
    private float crystalHP = 75f;

    private AudioSource audioSource;

    [SerializeField]
    private AudioClip destroyedSfx;

    public delegate void DamageEvent(float mine);

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = destroyedSfx;
        audioSource.playOnAwake = false;
        audioSource.volume = 2f; 
    }

    public void MineDamage(float mine)
    {
        crystalHP -= mine;
        Debug.Log("Crystal HP: " + crystalHP);

        if (crystalHP <= 0)
        {
            if (destroyedSfx != null)
            {
                audioSource.PlayOneShot(destroyedSfx);
            }
            
            Destroy(gameObject, destroyedSfx.length);
            Debug.Log("Crystal Mined");
        }
    }
}
