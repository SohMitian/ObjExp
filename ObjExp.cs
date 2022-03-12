using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections.LowLevel.Unsafe;

// [RequireComponent(typeof(ARPlaneManager))]
// [RequireComponent(typeof(MeshFilter))]
public class ObjExp : MonoBehaviour
{
    ARSessionOrigin m_ARSessionOrigin;
    ARPlaneManager m_ARPlaneManager;
    MeshFilter _obj;
    MeshFilter[] sceneMeshes;
    private string _storagePath;

    void Awake()
    {
        m_ARSessionOrigin = GetComponent<ARSessionOrigin>();
        m_ARPlaneManager = GetComponent<ARPlaneManager>();
    }

    // 回転反映
    Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }

    // スケール反映
    Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public void SaveOBJ()
    {
        int lastIndex = 0;
        // Mesh取得
        var sceneMeshesIndex = 0;
        sceneMeshes = new MeshFilter[m_ARPlaneManager.trackables.count];
        foreach (ARPlane ARPlane in m_ARPlaneManager.trackables)
        {
            sceneMeshes[sceneMeshesIndex++] = ARPlane.GetComponent<MeshFilter>();
        }

        //保存先ディレクトリ生成
        _storagePath = Application.dataPath + "/" + "ObjExp";
        Directory.CreateDirectory(_storagePath);

        //ファイルパス定義
        var _filePath = _storagePath + "/" + Application.productName + ".obj";


        StringBuilder sb = new StringBuilder();

        // OBJファイルの頭部分
        sb.AppendLine("# OBJ File:" + Application.productName);
        sb.AppendLine("mtllib " + Application.productName + ".mtl");

        string meshName = sceneMeshes[0].gameObject.name;
        MeshFilter mf = sceneMeshes[0];
        MeshRenderer mr = sceneMeshes[0].gameObject.GetComponent<MeshRenderer>();

        // グループ名
        sb.AppendLine("o " + mf.name);

        // mesh出力
        Mesh msh = mf.sharedMesh;

        //頂点情報
        foreach (Vector3 vx in msh.vertices)
        {
            Vector3 v = vx;
            v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale);
            v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
            v += mf.gameObject.transform.position;
            v.x *= -1;
            sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);
        }

        //法線情報
        foreach (Vector3 vx in msh.normals)
        {
            Vector3 v = vx;
            v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale.normalized);      
            v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
            v.x *= -1;
            sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

        }
        //UV情報
        foreach (Vector2 v in msh.uv)
        {
            sb.AppendLine("vt " + v.x + " " + v.y);
        }
        //面情報
        int faceOrder = (int)Mathf.Clamp((mf.gameObject.transform.lossyScale.x * mf.gameObject.transform.lossyScale.z), -1, 1);

        for (int j=0; j < msh.subMeshCount; j++)
        {
            if(mr != null && j < mr.sharedMaterials.Length)
            {
                string matName = mr.sharedMaterials[j].name;
                sb.AppendLine("usemtl " + matName);
            }
            else
            {
                sb.AppendLine("usemtl " + meshName + "_sm" + j);
            }

            int[] tris = msh.GetTriangles(j);
            for(int t = 0; t < tris.Length; t+= 3)
            {
                int idx2 = tris[t] + 1 + lastIndex;
                int idx1 = tris[t + 1] + 1 + lastIndex;
                int idx0 = tris[t + 2] + 1 + lastIndex;
                if(faceOrder < 0)
                {
                    sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                }
                else
                {
                    sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                }
                
            }
        }
        lastIndex += msh.vertices.Length;

        // 書き出し
        File.WriteAllText(_storagePath + "/" + Application.productName + ".obj", sb.ToString());
    }

    private string ConstructOBJString(int index)
    {
        string idxString = index.ToString();
        return idxString + "/" + idxString + "/" + idxString;
    }

}