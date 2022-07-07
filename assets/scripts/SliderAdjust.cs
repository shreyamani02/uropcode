using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.UI;

public class SliderAdjust : MonoBehaviour
{

    GameObject dot;
    public TextAsset pcaDataAsset;
    public TextAsset meanDataAsset;
    public TextAsset landmarks_coordinates;

    public Slider sliderBMI;
    public Slider sliderStature;
    public Slider sliderSittingHeight;
    public Slider sliderAge;

    public Text lbBMI;
    public Text lbStature;
    public Text lbSittingHeight;
    public Text lbAge;
    public Toggle myToggle;

    List<double[]> pcaData = new List<double[]>();
    List<double[]> landmarkData = new List<double[]>();
    List<double[]> totalData = new List<double[]>();



    int predAnthNum = 51;
    int predLandmarkNum = 93;

    double[][] meanVertices;
    double[][] meanLandmarks;
    Mesh mesh;
    Vector3[] vertices;


    //this for initialization
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        var pcaDataStr = pcaDataAsset.text.Split(new char[] { '\n' });

        for (int ncnt = 0; ncnt < pcaDataStr.Length; ncnt++)
        {
            var aline = pcaDataStr[ncnt];
            string[] linedata = aline.Split(new char[] { ',' });

            if (aline == "") continue;
            List<double> adata = new List<double>();
            for (int i = 0; i < linedata.Length; i++)
            {
                if (linedata[i].Contains("\r"))
                    linedata[i] = linedata[i].Replace("\r", string.Empty);

                adata.Add(Convert.ToDouble(linedata[i]));
                //Debug.Log(adata[i]);
            }

            pcaData.Add(adata.ToArray());
            Debug.Log(ncnt);

            if (ncnt == 43610)
                Debug.Log(ncnt);
        }

        var landmarkStr = landmarks_coordinates.text.Split(new char[] { '\n' });

        for (int ncnt = 0; ncnt < predLandmarkNum * 3; ncnt += 3)
        {
            List<double> adata = new List<double>();
            adata.Add(Convert.ToDouble(landmarkStr[ncnt]));
            adata.Add(Convert.ToDouble(landmarkStr[ncnt + 1]));
            adata.Add(Convert.ToDouble(landmarkStr[ncnt + 2]));

            landmarkData.Add(adata.ToArray());
        }

        var filter = GetComponent<MeshFilter>();
        if (!filter)
            filter = gameObject.AddComponent<MeshFilter>();

        // get mean vertices
        mesh = filter.mesh;

        // make the mesh capable for dynamic change
        mesh.MarkDynamic();

        List<double[]> verts = new List<double[]>();
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            verts.Add(new double[] { mesh.vertices[i].x, mesh.vertices[i].y, mesh.vertices[i].z });
        }

        meanVertices = verts.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] += Vector3.up * Time.deltaTime;
        }

        // assign the local vertices array into the vertices array of the Mesh.
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    public void ModelAnthroUpdate()
    {
        var newverts = mesh.vertices;
        var newtri = mesh.triangles;
        var newnorm = mesh.normals;

        ////lbStatus.text = pcaData[(int)slider.value];

        // input : {stature (mm), BMI (kg/m^2), Sitting Height/Stature, Age (YO), Age*BMI, 1.0}.
        var Anths = new double[] {
            sliderStature.value,
            sliderBMI.value,
            sliderSittingHeight.value,
            sliderAge.value,
            sliderBMI.value*sliderAge.value,
            1.0
        };

        var skipNum = predAnthNum + predLandmarkNum * 3;
        for (int i = 0; i < meanVertices.Length; i++)
        {
            var diffx = calcCoords(Anths, pcaData[skipNum + i * 3 + 0]);
            var diffy = calcCoords(Anths, pcaData[skipNum + i * 3 + 1]);
            var diffz = calcCoords(Anths, pcaData[skipNum + i * 3 + 2]);

            newverts[i].x = (float)meanVertices[i][0] + diffx;
            newverts[i].y = (float)meanVertices[i][1] + diffy;
            newverts[i].z = (float)meanVertices[i][2] + diffz;
        }

        mesh.Clear();
        mesh.vertices = newverts;
        mesh.triangles = newtri;
        mesh.normals = newnorm;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        lbStature.text = "Stature: " + sliderStature.value.ToString();
        lbBMI.text = "BMI: " + sliderBMI.value.ToString();
        lbSittingHeight.text = "Sitting Height / Stature: " + sliderSittingHeight.value.ToString();
        lbAge.text = "Age: " + sliderAge.value.ToString();

    }

    public void ShowLandmarks()
    {

        //reads user inputs
        var Anths = new double[] {
            sliderStature.value,
            sliderBMI.value,
            sliderSittingHeight.value,
            sliderAge.value,
            sliderBMI.value*sliderAge.value,
            1.0
        };

        //stores landmark datat into array of game objects
        Vector3[] vertices = new Vector3[predLandmarkNum];
        for (int i = 0; i < predLandmarkNum; i++)
        {
            vertices[i].x = (float)landmarkData[i][0];
            vertices[i].y = (float)landmarkData[i][1];
            vertices[i].z = (float)landmarkData[i][2];
            Debug.Log(vertices[i].x);
        }
        GameObject[] sphere = new GameObject[predLandmarkNum];
        for (int i = 0; i < predLandmarkNum; i++)
        {
            sphere[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 set = vertices[i];
            float xval = set.x;
            Debug.Log(xval);
            float yval = set.y;
            float zval = set.z;
            Debug.Log(sphere[i].transform.position.x);
            sphere[i].transform.position = set;
            Vector3 temp2 = new Vector3(20, 20, 20);
            sphere[i].transform.localScale += temp2;
        }

        //if toggle is off, then it turns the alpha to 0. If it is on, then it turns the alpha to 1.
        if (!myToggle.isOn)
        {
            Debug.Log("hi");
            for (int i = 0; i < 93; i++)
            {
                Renderer r = sphere[i].GetComponent<Renderer>();
                Color newColor = r.material.color;
                newColor.a = 0;
                r.material.color = newColor;
            }
        }
        else
        {
            for (int i = 0; i < 93; i++)
            {
                Renderer r = sphere[i].GetComponent<Renderer>();
                Color newColor = r.material.color;
                newColor.a = 1;
                r.material.color = newColor;

                /*//adjusts the coordinates and landmarks are changed
                var diffx = calcCoords(Anths, landmarkData[i]);
                var diffy = calcCoords(Anths, landmarkData[i+1]);
                var diffz = calcCoords(Anths, landmarkData[i+2]);
                Vector3 newPosition = new Vector3(diffx, diffy, diffz);
                sphere[i].transform.position += newPosition;*/
            }
        }
    }

    private float calcCoords(double[] diffAnths, double[] onePCAdata)
    {
        var diffCoords = 0.0;

        for (int k = 0; k < diffAnths.Length; k++)
        {
            diffCoords += onePCAdata[k] * diffAnths[k];
        }

        return ((float)diffCoords);
    }
}
