using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfLife : MonoBehaviour
{
    public int width = 16, depth = 16, height = 100, currentHeight = 1;
    [Range(0, 1)]
    public float random = 0f;
    [Range(0, 120)]
    public float fps = 1f;

    public GameObject cube, empty;

    public GameObject[,] board;
    public GameObject[] emptys;
    public int[,] iboard, itemp;

    public ComputeShader shader;
    private ComputeBuffer boardBuffer, tempBuffer;
    private int kernel;
    
    private void Start() {
        this.board = new GameObject[width, depth];
        this.emptys = new GameObject[height];
        this.iboard = new int[width, depth];
        this.itemp = new int[width, depth];
        RandomBoard();
        InvokeRepeating("main", 1f / fps, 1f / fps);
    }

    private void RandomBoard() {
        this.emptys[0] = GameObject.Instantiate(empty, new Vector3(0, 0, 0), Quaternion.identity, this.transform);
        int a = 0;
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < depth; z++) {
                float f = Mathf.PerlinNoise((x + Random.Range(-1000000, 1000000)) * 0.75f, (z + Random.Range(-1000000, 1000000)) * 0.75f);
                if (f > random) {
                    this.iboard[x, z] = 1;
                    this.board[x, z] = GameObject.Instantiate(cube, new Vector3(x, 0, z), Quaternion.identity, this.emptys[0].transform);
                    a++;
                }
            }
        } 

        CombineInstance[] combine = new CombineInstance[a];

        a = 0;
        foreach (MeshFilter m in this.emptys[0].transform.GetComponentsInChildren<MeshFilter>()) {
            if (a != 0) {
                combine[a - 1].mesh = m.sharedMesh;
                combine[a - 1].transform = m.transform.localToWorldMatrix;
                m.gameObject.SetActive(false);
            }
            a++;
        }

        this.emptys[0].transform.GetComponent<MeshFilter>().mesh = new Mesh();
        this.emptys[0].transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        this.emptys[0].transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        this.emptys[0].gameObject.SetActive(true);
    }

    private void main() {
        if (this.currentHeight < this.height) {
            this.itemp = this.iboard.Clone() as int[,];
            this.iboard = new int[width, depth];
            this.emptys[currentHeight] = GameObject.Instantiate(empty, new Vector3(0, 0, 0), Quaternion.identity, this.transform);

            // ================================================================================================================================ //
            
            int a = 0;
            for (int x = 0; x < width; x++) {
                for (int z = 0; z < depth; z++) {
                    bool create = false;
                    int neighbours = 0;
                    if (this.itemp[x, z] == 0) {
                        int x2_1 = (x - 1 < 0) ? 0 : x - 1;
                        int z2_1 = (z - 1 < 0) ? 0 : z - 1;
                        int x2_2 = (x + 1 == width) ? width - 1 : x + 1;
                        int z2_2 = (z + 1 == width) ? width - 1 : z + 1;
                        for (int x2 = x2_1; x2 <= x2_2; x2++) {
                            for (int z2 = z2_1; z2 <= z2_2; z2++ ) {
                                neighbours += (this.itemp[x2, z2] == 1 && (new Vector2(x, z) != new Vector2(x2, z2))) ? 1 : 0;
                            }
                        }
                        create = (neighbours == 3) ? true : false;
                    } else {
                        int x2_1 = (x - 1 < 0) ? 0 : x - 1;
                        int z2_1 = (z - 1 < 0) ? 0 : z - 1;
                        int x2_2 = (x + 1 == width) ? width - 1 : x + 1;
                        int z2_2 = (z + 1 == width) ? width - 1 : z + 1;
                        for (int x2 = x2_1; x2 <= x2_2; x2++) {
                            for (int z2 = z2_1; z2 <= z2_2; z2++ ) {
                                neighbours += (this.itemp[x2, z2] == 1 && (new Vector2(x, z) != new Vector2(x2, z2))) ? 1 : 0;
                            }
                        }
                        create = (neighbours == 2) ? true : (neighbours == 3) ? true : false;
                    }

                    if (create) {
                        this.iboard[x, z] = 1;
                        this.board[x, z] = GameObject.Instantiate(cube, new Vector3(x, currentHeight, z), Quaternion.identity, this.emptys[currentHeight].transform);
                        a++;
                    }
                }
            }

            // ================================================================================================================================ //

            CombineInstance[] combine = new CombineInstance[a];

            a = 0;
            foreach (MeshFilter m in this.emptys[currentHeight].transform.GetComponentsInChildren<MeshFilter>()) {
                if (a != 0) {
                    combine[a - 1].mesh = m.sharedMesh;
                    combine[a - 1].transform = m.transform.localToWorldMatrix;
                    m.gameObject.SetActive(false);
                }
                a++;
            }

            this.emptys[currentHeight].transform.GetComponent<MeshFilter>().mesh = new Mesh();
            this.emptys[currentHeight].transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            this.emptys[currentHeight].transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            this.emptys[currentHeight].gameObject.SetActive(true);

            this.currentHeight++;
        }
    }

    private void Clear() {
        GameObject[] children = new GameObject[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            children[i] = child.gameObject;
            i++;
        }
        foreach (GameObject child in children)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void OnDestroy() {
        Clear();
    }
}
