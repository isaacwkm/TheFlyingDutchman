using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class SetCloudPosition : MonoBehaviour
    {


        Renderer render;
        ParticleSystem.ShapeModule shape;
        public float density;
        public PlumeModule plume;
        public ParticleSystem system;
        public new BoxCollider collider;
        public float destroyTime;

        public Vector3Int pos;
        public Vector3 closestHeavy = Vector3.zero;


        public void Init()
        {

            render = GetComponent<Renderer>();
            shape = GetComponent<ParticleSystem>().shape;
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            render.GetPropertyBlock(propBlock);
            closestHeavy = plume.GetClosestHeavy(transform.position);
            propBlock.SetVector("_CenterPoint", closestHeavy == Vector3.zero ? transform.position : closestHeavy);
            propBlock.SetFloat("_Density", density);
            render.SetPropertyBlock(propBlock);

        }

        public void Destroy()
        {

            system.Stop();

            StartCoroutine(DestroyTimer());


        }

        IEnumerator DestroyTimer()
        {

            yield return new WaitForSeconds(destroyTime);

            DestroyImmediate(gameObject);

        }

    }
}