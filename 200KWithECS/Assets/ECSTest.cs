using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

public class ECSTest : MonoBehaviour
{
    public UInbCubesDisplayer m_textDisplayer;
    
    public Mesh m_cubeMesh;

    public Material[] m_cubeMaterials;
    
    public int m_nbOfCubes = 50;
    public float m_maxDistance = 10;
    public float m_gravity = 8f;
    public float m_defaultInitialSpeed = 5;

    private int m_totalCubesOnScene;
    // Start is called before the first frame update
    void Start()
    {
        m_entityManager = World.Active.EntityManager;
        m_cubeArchetype = m_entityManager.CreateArchetype(
            typeof(RenderMesh),
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(Jump),
            typeof(Gravity)
            );
        
        //CreateCubes();
    }

    private void CreateCubes()
    {
        NativeArray<Entity> nativeArray = new NativeArray<Entity>(m_nbOfCubes, Allocator.Temp);

        m_entityManager.CreateEntity(m_cubeArchetype, nativeArray);

        foreach (var entity in nativeArray)
        {
            int randomMaterialIndex = UnityEngine.Random.Range(0, m_cubeMaterials.Length);
            m_entityManager.SetComponentData(entity, new Translation
            {
                Value = new float3(UnityEngine.Random.Range(-m_maxDistance, m_maxDistance), 0, UnityEngine.Random.Range(-m_maxDistance, m_maxDistance))
            });

            m_entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = m_cubeMesh,
                material = m_cubeMaterials[randomMaterialIndex]
            });
            
            m_entityManager.SetComponentData(entity, new Gravity
            {
                m_value = m_gravity
            });

            float initialSpeed = m_defaultInitialSpeed * UnityEngine.Random.Range(1f, 10f);
            
            m_entityManager.SetComponentData(entity, new Jump
            {
                m_initialJumpSpeed = initialSpeed
            });
        }

        m_totalCubesOnScene += m_nbOfCubes;
        m_textDisplayer.DisplayText("Total number of Cubes: "+m_totalCubesOnScene);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateCubes();
        }
    }

    private EntityManager m_entityManager;
    private EntityArchetype m_cubeArchetype;
    
}

public struct Gravity : IComponentData
{
    public float m_value;
}

public struct Jump : IComponentData
{
    public float m_initialJumpSpeed;
    public float m_currentJumpSpeed;
}
#region ComponentSystems
//public class JumpSystem : ComponentSystem
//{
//    protected override void OnUpdate()
//    {
//        Entities.ForEach((Entity _entity, ref Translation _translation, ref Jump _jump) =>
//        {
//            _translation.Value += new float3(0,_jump.m_currentJumpSpeed*Time.deltaTime,0);
//        });
//    }
//}

//public class GravitySystem : ComponentSystem
//{
//    protected override void OnUpdate()
//    {
//        Entities.ForEach((Entity _entity, ref Jump _jump, ref Gravity _gravity) =>
//        {
//            _jump.m_currentJumpSpeed -= _gravity.m_value * Time.deltaTime;
//        });
//    }
//}

//public class FloorDetectionSystem : ComponentSystem
//{
//    protected override void OnUpdate()
//    {
//        Entities.ForEach((Entity _entity, ref Translation _translation, ref Jump _jump) =>
//        {
//            if (_translation.Value.y < 0)
//            {
//                _translation.Value.y = 0;
//                _jump.m_currentJumpSpeed = _jump.m_initialJumpSpeed;
//            }
//        });
//    }
//}
#endregion

#region JobSystems


public class JumpJobSystem : JobComponentSystem
{
    [BurstCompile]
    public struct JumpJob : IJobForEachWithEntity<Translation, Jump>
    {
        public float m_deltaTime;
        public void Execute(Entity _entity, int _index, ref Translation _translation, ref Jump _jump)
        {
            _translation.Value += new float3(0,_jump.m_currentJumpSpeed*m_deltaTime,0);
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JumpJob job = new JumpJob
        {
            m_deltaTime = Time.deltaTime
        };

        JobHandle jobHandle = job.Schedule(this, inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}

public class GravityJobSystem : JobComponentSystem
{ 
    [BurstCompile]
    public struct GravityJob : IJobForEachWithEntity<Jump, Gravity>
    {
        public float m_deltaTime;
        public void Execute(Entity _entity, int _index, ref Jump _jump, ref Gravity _gravity)
        {
            _jump.m_currentJumpSpeed -= _gravity.m_value * m_deltaTime;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        GravityJob job = new GravityJob
        {
            m_deltaTime = Time.deltaTime
        };

        JobHandle jobHandle = job.Schedule(this, inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}

public class FloorDetectionJobSystem : JobComponentSystem
{
    [BurstCompile]
    public struct FloorDetectionJob : IJobForEachWithEntity<Jump, Translation>
    {
        public void Execute(Entity _entity, int _index, ref Jump _jump, ref Translation _translation)
        {
            if (_translation.Value.y < 0)
            {
                _translation.Value.y = 0;
                _jump.m_currentJumpSpeed = _jump.m_initialJumpSpeed;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        FloorDetectionJob job = new FloorDetectionJob();
        JobHandle jobHandle = job.Schedule(this, inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}








#endregion














