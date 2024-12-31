using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerSensor : MonoBehaviour
{
    public float distance;
    public float angle;
    public float height;
    public Color meshColor = Color.red;
    private Mesh mesh;
    public int scanFrequency = 30;
    public LayerMask layers;
    public LayerMask occlusionLayers;
    public List<GameObject> objects = new List<GameObject>();
    Collider[] colliders = new Collider[50];
    int count;
    float scanInterval;
    float scanTimer;

    void Start(){
        scanInterval= 1.0f/scanFrequency;
    }

    void Update(){
        scanTimer -= Time.deltaTime;
        if(scanTimer<0){
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan(){
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);
        objects.Clear();
        for(int i=0; i<count; ++i){
            GameObject obj = colliders[i].gameObject;
            if(isInSight(obj)){
                objects.Add(obj);
            }
        }
    }

    public bool isInSight(GameObject obj){
        Vector3 origin = transform.position;
        Vector3 dest = obj.transform.position;
        Vector3 direction=dest-origin;

        // if(direction.y<0 || direction.y> height){
        //     return false;
        // }

        direction.y = 0;

        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if(deltaAngle>angle){
            return false;
        }

        origin.y +=height/2;
        dest.y = origin.y;

        if(Physics.Linecast(origin, dest, occlusionLayers)){
            return false;
        }

        return true;
    }

    private Mesh CreateWedgeMesh(){
        Mesh mesh = new Mesh();

        int segments=10;
        int numTraingles =(segments * 4) + 2 +2;
        int numVertices = numTraingles * 3;
        
        Vector3[] vertices = new Vector3[numVertices];
        int [] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance;
        Vector3 bottomRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;

        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up *height;
        Vector3 topRight = bottomRight + Vector3.up *height;

        int vert=0;

        //left side code
        vertices[vert++]=bottomCenter;
        vertices[vert++]=bottomLeft;
        vertices[vert++]=topLeft;

        vertices[vert++]=topLeft;
        vertices[vert++]=topCenter;
        vertices[vert++]=bottomCenter;


        //right hand side
        vertices[vert++]=bottomCenter;
        vertices[vert++]=topCenter;
        vertices[vert++]=topRight;

        vertices[vert++]=topRight;
        vertices[vert++]=bottomRight;
        vertices[vert++]=bottomCenter;

        float currentAngle = -angle;
        float deltaAngle =(angle * 2)/segments;

        for(int i=0; i<segments; ++i){
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance;

            topLeft = bottomLeft + Vector3.up *height;
            topRight = bottomRight + Vector3.up *height;

            //far side
            vertices[vert++]=bottomLeft;
            vertices[vert++]=bottomRight;
            vertices[vert++]=topRight;

            vertices[vert++]=topRight;
            vertices[vert++]=topLeft;
            vertices[vert++]=bottomLeft;

            //top side
            vertices[vert++]=topCenter;
            vertices[vert++]=topLeft;
            vertices[vert++]=topRight;

            //bottom side
            vertices[vert++]=bottomCenter;
            vertices[vert++]=bottomRight;
            vertices[vert++]=bottomLeft;
            currentAngle+=deltaAngle;
        }

        
        for(int i=0; i<numVertices; ++i){
            triangles[i]=i;
        }

        mesh.vertices=vertices;
        mesh.triangles=triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnValidate(){
        mesh= CreateWedgeMesh();
        scanInterval= 1.0f/scanFrequency;
    }

    private void OnDrawGizmos(){
        if(mesh){
            Gizmos.color=meshColor;
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);

            Gizmos.DrawWireSphere(transform.position, distance);

            for(int i=0; i<count; i++){
                Gizmos.DrawSphere(colliders[i].transform.position, 0.2f);
            }

        }
    }
}
