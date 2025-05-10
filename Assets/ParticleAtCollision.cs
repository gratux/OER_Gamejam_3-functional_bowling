using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleAtCollision : MonoBehaviour
{
    public ParticleSystem collisionParticles;
    
    private void OnCollisionEnter(Collision other)
    {
        List<ContactPoint> contactPoints = new();
        other.GetContacts(contactPoints);
        var particleSystems = contactPoints.Select(contactPoint => Instantiate(collisionParticles, contactPoint.point, Quaternion.identity)).ToList();
        particleSystems.ForEach(ps =>
        {
            ps.gameObject.SetActive(true);
            Destroy(ps.gameObject, ps.main.duration);
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
