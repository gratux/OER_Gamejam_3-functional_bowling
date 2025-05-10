using System.Collections.Generic;
using UnityEngine;

namespace Particles
{
    public class ParticleAtCollision : MonoBehaviour
    {
        public ParticleSystem collisionParticles;
    
        private void OnCollisionEnter(Collision other)
        {
            List<ContactPoint> contactPoints = new();
            other.GetContacts(contactPoints);
            contactPoints.ForEach(cp =>
            {
                var ps = Instantiate(collisionParticles, cp.point, Quaternion.identity);
                ps.gameObject.SetActive(true);
                Destroy(ps.gameObject, ps.main.duration);
            });
        }
    }
}
