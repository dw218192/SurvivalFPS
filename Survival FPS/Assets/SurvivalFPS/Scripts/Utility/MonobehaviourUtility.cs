using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Utility
{
    /// <summary>
    /// converts the box collider's position and side lengths into world space, taking into account hierarchial scaling
    /// </summary>
    /// <param name="collider">the box collider</param>
    /// <param name="centerPos">the box collider's center position in world space</param>
    /// <param name="radius">the box collider's world space side lengths</param>
    public static class MonobehaviourUtility
    {
        /// <summary>
        /// signed angle between two vectors in degrees
        /// </summary>
        public static float SignedAngleBetween(Vector3 from, Vector3 to)
        {
            if (from == to) return 0.0f;
            float angle = Vector3.Angle(from, to);
            Vector3 cross = Vector3.Cross(from, to);
            angle *= Mathf.Sign(cross.y);
            return angle;
        }

        public static void ConvertBoxColliderToWorldSpace(BoxCollider collider, out Vector3 centerPos, out Vector3 sidelengths)
        {
            centerPos = Vector3.zero;
            sidelengths = Vector3.zero;

            if (collider)
            {
                centerPos = collider.transform.position;
                centerPos.x += collider.center.x * collider.transform.lossyScale.x;
                centerPos.y += collider.center.y * collider.transform.lossyScale.y;
                centerPos.z += collider.center.z * collider.transform.lossyScale.z;

                sidelengths = collider.size;
                sidelengths.x *= collider.transform.lossyScale.x;
                sidelengths.y *= collider.transform.lossyScale.y;
                sidelengths.z *= collider.transform.lossyScale.z;
            }
        }

        /// <summary>
        /// converts the sphere collider's position and radius into world space, taking into account hierarchial scaling
        /// </summary>
        /// <param name="collider">the sphere collider</param>
        /// <param name="centerPos">the sphere collider's center position in world space</param>
        /// <param name="radius">the sphere collider's world space radius</param>
        public static void ConvertSphereColliderToWorldSpace(SphereCollider collider, out Vector3 centerPos, out float radius)
        {
            centerPos = Vector3.zero;
            radius = 0.0f;

            if (collider)
            {
                centerPos = collider.transform.position;
                centerPos.x += collider.center.x * collider.transform.lossyScale.x;
                centerPos.y += collider.center.y * collider.transform.lossyScale.y;
                centerPos.z += collider.center.z * collider.transform.lossyScale.z;

                radius = Mathf.Max(collider.radius * collider.transform.lossyScale.x, collider.radius * collider.transform.lossyScale.y);
                radius = Mathf.Max(radius, collider.radius * collider.transform.lossyScale.z);
            }
        }

        public static void PlayRandom(this AudioSource audioSource, AudioClip[] clips)
        {
            if (clips.Length > 0)
            {
                int randomIndex = Random.Range(0, clips.Length);
                audioSource.PlayOneShot(clips[randomIndex]);
            }
        }
    }
}
