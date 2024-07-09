using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

public class RenderMeshUtilityDraw : MonoBehaviour
{
    public Mesh Mesh;
    public Material m_material;
    
    public bool m_useGameobjct = false;
    public bool m_differentMaterial = false;
    public bool m_differentMesh = false;

    public int m_w = 10;
    public int m_h = 10;

    public int drawNumberIndex = 0;
    public List<int> drawNumber = new List<int>(){
        10,50,100,150,
    };
    
    private  List<Material> matList = new List<Material>();
    private  List<Mesh> meshList = new List<Mesh>();
    private List<GameObject> gameObjects = new List<GameObject>();

    private GameObject primitiveObj;


    // Example Burst job that creates many entities
    [GenerateTestsForBurstCompatibility]
    public struct SpawnJob : IJobParallelFor
    {
        public Entity Prototype;
        public int w;
        public int h;
        public bool singleMat;
        public bool singleMesh;
        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute(int index)
        {
            // Clone the Prototype entity to create a new entity.
            var e = Ecb.Instantiate(index, Prototype);
            // Prototype has all correct components up front, can use SetComponent to
            // set values unique to the newly created entity, such as the transform.
            int matIndex = singleMat ? 0 : index;
            int meshIndex = singleMesh ? 0 : index;
            Ecb.SetComponent(index, e, MaterialMeshInfo.FromRenderMeshArrayIndices(matIndex, meshIndex));
            Ecb.SetComponent(index, e, new LocalToWorld {Value = ComputeTransform(index)});
        }

        public float4x4 ComputeTransform(int index)
        {
            int y = index / w;
            int x = index % h;
            float3 pos = new float3(x - (float)w * 0.5f, 0, y - (float)h * 0.5f);

            return float4x4.Translate(pos);
        }
    }


    public void AddDrawNumber()
    {
        if(drawNumberIndex >= drawNumber.Count-1)
        {
            return;
        }
        drawNumberIndex++;
        var curNumber = drawNumber[drawNumberIndex];
        m_w = m_h = curNumber;
    }
    
    public void DecressDrawNumber()
    {
        if(drawNumberIndex <= 0)
        {
            return;
        }
        drawNumberIndex--;
        var curNumber = drawNumber[drawNumberIndex];
        m_w = m_h = curNumber;
    }

    [ContextMenu("ReStartDraw")]
    public void ReStartDraw()
    {
        ClearAllDraw();
        
        if (m_useGameobjct)
        {
            DoStartGameobjectsRender();
        }
        else
        {
         
            DoStartEntitiesRender();
        }
    }
    
    [ContextMenu("ClearAllDraw")]
    public void ClearAllDraw()
    {
        DoEndEntitiesRender();
        DoEndGameobjectsRender();
        for(int i=0;i<matList.Count;i++)
        {
            DestroyImmediate(matList[i],true);
        }
        matList.Clear();
        for(int i=0;i<meshList.Count;i++)
        {
            DestroyImmediate(meshList[i],true);
        }
        meshList.Clear();
    }

    private void PrepareAssets(ref List<Material> matList, ref List<Mesh> meshList  )
    {
        int objCount = m_w * m_h;
        if ( m_differentMaterial )
        {
            for (int i=0;i<objCount;i++)
            {
                var mat = new Material(m_material);
                mat.name = m_material.name + i.ToString();
                Color col = Color.HSVToRGB(((float)(i * 10) / (float)objCount) % 1.0f, 0.7f, 1.0f);
                //                Color col = Color.HSVToRGB(Random.Range(0.0f,1.0f), 1.0f, 1.0f);
                mat.SetColor("_Color", col);              // set for LW
                mat.SetColor("_BaseColor", col);          // set for HD
                matList.Add(mat);
            }
        }
        else
        {
            var mat = new Material(m_material);
            matList.Add(mat); 
        }
        
        if ( m_differentMesh )
        {
            for (int i=0;i<objCount;i++)
            {
                Mesh copiedMesh = new Mesh();
                copiedMesh.name = Mesh.name + i.ToString();

                // Copy the original vertices
                Vector3[] vertices = Mesh.vertices;
                
                // Scale each vertex position
                for (int j = 0; j < Mesh.vertices.Length; j++)
                {
                    vertices[j] *=((float)(i * 10) / (float)objCount) % 1.0f;
                }
                
                // Apply the scaled vertices to the mesh
                copiedMesh.vertices = vertices;
                
                copiedMesh.triangles = Mesh.triangles;
                copiedMesh.normals = Mesh.normals;
                copiedMesh.uv = Mesh.uv;
              
                meshList.Add(copiedMesh);
            }
        }
        else
        {
            Mesh copiedMesh = new Mesh();
            copiedMesh.name = Mesh.name + "1";
            // Copy the original vertices
            Vector3[] vertices = Mesh.vertices;
            // Apply the scaled vertices to the mesh
            copiedMesh.vertices = vertices;
            copiedMesh.triangles = Mesh.triangles;
            copiedMesh.normals = Mesh.normals;
            copiedMesh.uv = Mesh.uv;

            meshList.Add(copiedMesh); 
        }
    }

    private void DoStartGameobjectsRender()
    {
        int objCount = m_w * m_h;

        matList.Clear();
        meshList.Clear();
        PrepareAssets(ref matList, ref meshList);

        if(primitiveObj == null)
        {
            primitiveObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            var _collider = primitiveObj.GetComponent<CapsuleCollider>();
            GameObject.DestroyImmediate(_collider);
            primitiveObj.SetActive(false);
      
        }

        for (int i = 0; i < objCount; i++)
        {
          var obj = Instantiate(primitiveObj,transform);
          obj.SetActive(true);
          var targetMat = matList[0];
          if (m_differentMaterial)
          {
              targetMat = matList[i];
          }
          var targetMesh = meshList[0];
          if (m_differentMesh)
          {
              targetMesh = meshList[i];
          }
          
          obj.GetComponent<MeshFilter>().mesh =targetMesh;
          obj.GetComponent<MeshRenderer>().sharedMaterial =targetMat;
          
          obj.transform.position =ComputeTransformObj(i).TransformPoint(obj.transform.position);
          gameObjects.Add(obj);
        }
    }

    private void DoEndGameobjectsRender()
    {
        foreach (var obj in gameObjects)
        {
            Destroy(obj);
        }
        gameObjects.Clear();
    }


    public float4x4 ComputeTransformObj(int index)
    {
        int y = index / m_w;
        int x = index % m_h;
        float3 pos = new float3(x - (float)m_w * 0.5f, 0, y - (float)m_h * 0.5f);

        return float4x4.Translate(pos);
    }
    

    private void DoStartEntitiesRender()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        int objCount = m_w * m_h;
        matList.Clear();
        meshList.Clear();
        PrepareAssets(ref matList, ref meshList);
        
        Debug.Log("matList:"+matList.Count);

        // Create a RenderMeshDescription using the convenience constructor
        // with named parameters.
        var desc = new RenderMeshDescription(
            shadowCastingMode: ShadowCastingMode.Off,
            receiveShadows: false);

        //var renderMeshArray = new RenderMeshArray(matList.ToArray(), new[] { Mesh });
        var renderMeshArray = new RenderMeshArray(matList.ToArray(), meshList.ToArray());

        // Create empty base entity
        var prototype = entityManager.CreateEntity();

        // Call AddComponents to populate base entity with the components required
        // by Entities Graphics
        RenderMeshUtility.AddComponents(
            prototype,
            entityManager,
            desc,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        entityManager.AddComponentData(prototype, new LocalToWorld());

        // Spawn most of the entities in a Burst job by cloning a pre-created prototype entity,
        // which can be either a Prefab or an entity created at run time like in this sample.
        // This is the fastest and most efficient way to create entities at run time.
        var spawnJob = new SpawnJob
        {
            Prototype = prototype,
            Ecb = ecb.AsParallelWriter(),
            w = m_w,
            h = m_h,
            singleMat = !m_differentMaterial,
            singleMesh = !m_differentMesh
        };

        var spawnHandle = spawnJob.Schedule(m_h*m_w,128);
        spawnHandle.Complete();

        ecb.Playback(entityManager);
        ecb.Dispose();
        entityManager.DestroyEntity(prototype);
    }
    
    private void DoEndEntitiesRender()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;

        // Destroy all entities with the RenderMesh component
        using (var query = entityManager.CreateEntityQuery(typeof(MaterialMeshInfo), typeof(LocalToWorld)))
        {
            Debug.Log("query:" + query.CalculateEntityCount());
            entityManager.DestroyEntity(query);
        }
    }
}
