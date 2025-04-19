using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class PlumeModule : CozyModule
    {

        Vector3Int renderedCenterChunk;

        [Tooltip("Holds a reference to the cloud profile")]
        public PlumeProfile volumetricCloudProfile;
        AnimationCurve cloudHeightRatio = new AnimationCurve(new Keyframe[3] { new Keyframe(0, 0), new Keyframe(0.45f, 1), new Keyframe(0.75f, 0.35f) });
        public GameObject cloudChunkPrefab;

        [Range(-1, 1)]
        [Tooltip("Adds an offset to your cloud coverage to allow for more or less clouds in your sky. -1 is no clouds and +1 is fully overcast")]
        public float coverageIntensityOffset = 0;

        [Range(0, 1)]
        public float cloudCoverage;
        [Tooltip("Allows your clouds to bend towards or away from the horizon to give an effect of a round planet")]
        [Range(-0.2f, 0.2f)]
        public float bendToHorizonMultiplier = 0.1f;
        public Vector3 offset;
        public Transform matrixHolder;

        [Tooltip("Should the clouds generate colliders? Useful for culling light flares")]
        public bool useColliders = true;
        [Tooltip("Should the clouds cast shadows on the ground? Not recommended for low end devices")]
        public bool useShadows = false;
        [Tooltip("Should the clouds be culled inside triggers with the tag FX Block Zone")]
        public bool cullInsideTriggers = false;

        Dictionary<Vector3Int, SetCloudPosition> chunks = new Dictionary<Vector3Int, SetCloudPosition>();
        private List<Vector3Int> heavyChunks = new List<Vector3Int>();

        [Tooltip("How many frames should pass before the noise renders again? A value of 0 renders every frame and a value of 30 renders once every 60 frames.")]
        [Range(0, 60)]
        public int framesBetweenRenders = 10;
        int framesLeft;

        void Awake()
        {
            InitializeModule();
            Generate();
        }


        public void Generate()
        {
            chunks.Clear();
            heavyChunks.Clear();

            if (matrixHolder)
                DestroyImmediate(matrixHolder.gameObject);

            if (!volumetricCloudProfile)
            {
                Debug.Log("Be sure to setup your cloud profile in the PLUME settings! Defaulting to the default clouds.");
                volumetricCloudProfile = (PlumeProfile)Resources.Load("Profiles/Default Volumetric Clouds");
            }

            if (!cloudChunkPrefab)
            {
                cloudChunkPrefab = (GameObject)Resources.Load("Cloud Chunk");
            }

            renderedCenterChunk = new Vector3Int((int)(transform.position.x / volumetricCloudProfile.chunkSize), 0, (int)(transform.position.y / volumetricCloudProfile.chunkSize));

            matrixHolder = new GameObject().transform;
            matrixHolder.name = "Plume Matrix Holder";
            // matrixHolder.gameObject.hideFlags = HideFlags.HideAndDontSave;

            for (int i = -volumetricCloudProfile.renderDistance; i < volumetricCloudProfile.renderDistance; i++)
                for (int j = -volumetricCloudProfile.renderDistance; j < volumetricCloudProfile.renderDistance; j++)
                {
                    Vector3Int newPos = new Vector3Int(i + renderedCenterChunk.x, 0, j + renderedCenterChunk.z);
                    CreateChunk(newPos);
                }
        }

        public void CreateChunk(Vector3Int globalChunkPos)
        {
            SetCloudPosition chunk = Instantiate(cloudChunkPrefab, matrixHolder).GetComponent<SetCloudPosition>();
            ResetChunk(globalChunkPos, chunk);
            chunks.Add(globalChunkPos, chunk);
        }

        public void MoveChunk(Vector3Int newChunkPos, Vector3Int oldChunkPos)
        {
            try
            {
                SetCloudPosition currentChunk = chunks[oldChunkPos];
                chunks.Add(newChunkPos, currentChunk);
                chunks.Remove(oldChunkPos);
                ResetChunk(newChunkPos, currentChunk);

                if (GetDensity(oldChunkPos, volumetricCloudProfile.noiseScale) > volumetricCloudProfile.normalReferenceHeight)
                    heavyChunks.Remove(oldChunkPos);
                if (GetDensity(newChunkPos, volumetricCloudProfile.noiseScale) > volumetricCloudProfile.normalReferenceHeight)
                    heavyChunks.Add(newChunkPos);
            }
            catch
            {
                return;
            }
        }

        void ResetChunk(Vector3Int globalChunkPos, SetCloudPosition chunk)
        {


            chunk.plume = this;
            chunk.transform.position = new Vector3(volumetricCloudProfile.chunkSize * globalChunkPos.x, volumetricCloudProfile.cloudHeight, volumetricCloudProfile.chunkSize * globalChunkPos.z);
            float density = Mathf.Lerp(volumetricCloudProfile.minChunkHeight, volumetricCloudProfile.maxChunkHeight * 2, cloudHeightRatio.Evaluate(Mathf.Clamp01(GetDensity(globalChunkPos, volumetricCloudProfile.noiseScale) + ((cloudCoverage - 0.4f) * 1.8f))));

            chunk.transform.position += Vector3.up * (density / 2 + (volumetricCloudProfile.cloudHeightDistrubution * GetDensity(globalChunkPos, volumetricCloudProfile.noiseScale / 2)));
            ParticleSystem l = chunk.system;

            if (cullInsideTriggers)
            {
                for (int j = 0; j < weatherSphere.cozyTriggers.Count; j++)
                    l.trigger.SetCollider(j, weatherSphere.cozyTriggers[j]);
            }

            ParticleSystem.EmissionModule emit = l.emission;
            ParticleSystem.MainModule main = l.main;
            main.maxParticles = 10000;
            main.startSize = volumetricCloudProfile.cloudParticleSize;

            l.GetComponent<ParticleSystemRenderer>().shadowCastingMode = useShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;

            emit.rateOverTime = ((volumetricCloudProfile.chunkSize * volumetricCloudProfile.chunkSize) * volumetricCloudProfile.cloudDensity * 0.0005f / main.startLifetime.constant) * density / volumetricCloudProfile.chunkSize;

            if (emit.rateOverTime.constant < 5)
            {
                emit.rateOverTime = 0;
                chunk.collider.enabled = false;

            }
            else
                chunk.collider.enabled = useColliders;

            chunk.transform.localScale = new Vector3(volumetricCloudProfile.chunkSize, density, volumetricCloudProfile.chunkSize);

            l.Play();
            chunk.system = l;
            chunk.density = density;
            chunk.pos = globalChunkPos;
            chunk.Init();


        }

        public Vector3 GetClosestHeavy(Vector3 pos)
        {


            Vector3 i = heavyChunks.OrderBy(e => Vector3.SqrMagnitude(pos - (Vector3)e)).FirstOrDefault();


            i = Vector3.Lerp(i, pos, Vector3.SqrMagnitude(pos - i) / (volumetricCloudProfile.normalizedDistance * volumetricCloudProfile.normalizedDistance));

            return i;

        }

        public void AddNeededChunks(Vector3Int old, Vector3Int current)
        {



        }

        // Update is called once per frame
        void Update()
        {

            if (weatherSphere == null)
                InitializeModule();

            if (!Application.isPlaying)
                return;

            if (matrixHolder == null)
                Generate();


            cloudCoverage = Mathf.Clamp01(weatherSphere.cloudCoverage + 0.15f + coverageIntensityOffset);

            if ((int)(transform.position.x / volumetricCloudProfile.chunkSize) - renderedCenterChunk.x != 0 || (int)(transform.position.z / volumetricCloudProfile.chunkSize) - renderedCenterChunk.z != 0)
            {

                Vector3Int dir = new Vector3Int(Mathf.Clamp((int)(transform.position.x / volumetricCloudProfile.chunkSize) - renderedCenterChunk.x, -1, 1), 0, Mathf.Clamp((int)(transform.position.z / volumetricCloudProfile.chunkSize) - renderedCenterChunk.z, -1, 1));

                if (dir.x != 0)
                {
                    for (int i = -volumetricCloudProfile.renderDistance - 1; i < volumetricCloudProfile.renderDistance + 1; i++)
                    {

                        Vector3Int offset = new Vector3Int(renderedCenterChunk.x - (dir.x * volumetricCloudProfile.renderDistance), 0, renderedCenterChunk.z + i);
                        Vector3Int targetOffset = new Vector3Int(renderedCenterChunk.x + (dir.x * volumetricCloudProfile.renderDistance), 0, renderedCenterChunk.z + i);
                        MoveChunk(targetOffset, offset);

                    }

                    renderedCenterChunk += Vector3Int.right * dir.x;
                }
                if (dir.z != 0)
                {
                    for (int i = -volumetricCloudProfile.renderDistance - 1; i < volumetricCloudProfile.renderDistance + 1; i++)
                    {

                        Vector3Int offset = new Vector3Int(renderedCenterChunk.x + i, 0, renderedCenterChunk.z - (dir.z * volumetricCloudProfile.renderDistance));
                        Vector3Int targetOffset = new Vector3Int(renderedCenterChunk.x + i, 0, renderedCenterChunk.z + (dir.z * volumetricCloudProfile.renderDistance));
                        MoveChunk(targetOffset, offset);

                    }

                    renderedCenterChunk += Vector3Int.forward * dir.z;
                }
            }

            if (Application.isPlaying)
                offset += volumetricCloudProfile.windSpeed * Time.deltaTime * 0.01f;

            if (framesLeft < 0)
            {
                UpdateNoise();
                framesLeft = framesBetweenRenders;
            }
            else
                framesLeft--;


            UpdateShaderVariables();

        }

        public void UpdateShaderVariables()
        {
            Shader.SetGlobalColor("PLUME_MainCloudColor", weatherSphere.cloudColor * Mathf.Lerp(volumetricCloudProfile.cloudShadowColorMultiplier, volumetricCloudProfile.cloudColorMultiplier, Mathf.Clamp01(weatherSphere.sunColor.r)));
            Shader.SetGlobalColor("PLUME_CloudShadowColor", weatherSphere.cloudColor * volumetricCloudProfile.cloudShadowColorMultiplier);
            Shader.SetGlobalFloat("PLUME_CloudBendMultiplier", bendToHorizonMultiplier);
        }

        public void UpdateNoise()
        {
            ParticleSystem.ShapeModule shape;
            ParticleSystem.EmissionModule emit;
            ParticleSystem.MainModule main;
            ParticleSystem.VelocityOverLifetimeModule velocity;

            chunks.GetEnumerator();

            foreach (KeyValuePair<Vector3Int, SetCloudPosition> i in chunks)
            {
                SetCloudPosition j = i.Value;

                shape = j.system.shape;
                emit = j.system.emission;
                main = j.system.main;
                velocity = j.system.velocityOverLifetime;

                float noiseDensity = GetDensity(i.Key, volumetricCloudProfile.noiseScale) + ((cloudCoverage - 0.4f) * 1.8f);
                float density = Mathf.Lerp(volumetricCloudProfile.minChunkHeight, volumetricCloudProfile.maxChunkHeight * 2, cloudHeightRatio.Evaluate(Mathf.Clamp01(noiseDensity)));

                j.transform.localScale = new Vector3(volumetricCloudProfile.chunkSize, density, volumetricCloudProfile.chunkSize);
                emit.rateOverTime = ((volumetricCloudProfile.chunkSize * volumetricCloudProfile.chunkSize) * volumetricCloudProfile.cloudDensity * 0.0005f / main.startLifetime.constant) * density / volumetricCloudProfile.chunkSize;

                velocity.x = volumetricCloudProfile.windSpeed.x * volumetricCloudProfile.chunkSize * 0.1f;
                velocity.z = volumetricCloudProfile.windSpeed.y * volumetricCloudProfile.chunkSize * 0.1f;

                if (emit.rateOverTime.constant < 5)
                {
                    emit.rateOverTime = 0;
                    j.collider.enabled = false;

                }
            }
        }

        public float GetDensity(Vector3Int pos, float scale)
        {
            return ((Mathf.PerlinNoise((pos.x / scale) + (volumetricCloudProfile.seed * 10000) + offset.x, (pos.z / scale) + offset.z + (-volumetricCloudProfile.seed * 1000)) - 0.5f) * 2);
        }


        public override void DeinitializeModule()
        {
            if (matrixHolder != null)
                DestroyImmediate(matrixHolder.gameObject);

            base.DeinitializeModule();
        }

    }
}