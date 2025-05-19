using System;
using System.Collections.Generic;
using Godot;

namespace Com.Astral.Stars;

[Tool, GlobalClass, Icon("res://Resources/Textures/Editor/planet.png")]
public partial class PlanetData : Resource
{
	[Export]
	public float Size
	{
		get => _size;
		protected set
		{
			_size = Mathf.Max(value, 1f);
		}
	}
	private float _size = 1000f;
	[Export(PropertyHint.Range, "0,1,0.001")] public float Roughness { get; protected set; } = 0.1f;
	[Export] protected NoiseTexture3D noiseTexture;

	[Export(PropertyHint.Range, "0,100,0.000001,or_greater")] public float Density { get; private set; } = 1f;
	public float Volume { get; protected set; } = 0f;
	public float Mass => Volume * Density;

	public Vector3[] Vertices { get; protected set; } = Array.Empty<Vector3>();
	public Vector3[] Normals { get; protected set; } = Array.Empty<Vector3>();
	public float[] Tangents { get; protected set; } = Array.Empty<float>();
	public Color[] Colors { get; protected set; } = Array.Empty<Color>();
	public Vector2[] UV { get; protected set; } = Array.Empty<Vector2>();
	public Vector2[] UV2 { get; protected set; } = Array.Empty<Vector2>();
	public int[] Indices { get; protected set; } = Array.Empty<int>();

	[Export]
	public int Precision
	{
		get => _precision;
		set
		{
			_precision = Mathf.Max(value, 0);
		}
	}
	protected int _precision = 5;

	public Godot.Collections.Array GetMeshData()
	{
		Godot.Collections.Array lMeshData = new Godot.Collections.Array();
		lMeshData.Resize((int)Mesh.ArrayType.Max);

		lMeshData[(int)Mesh.ArrayType.Vertex] = Vertices;
		lMeshData[(int)Mesh.ArrayType.Normal] = Normals;
		lMeshData[(int)Mesh.ArrayType.Tangent] = Tangents;
		lMeshData[(int)Mesh.ArrayType.Color] = Colors;
		lMeshData[(int)Mesh.ArrayType.TexUV] = UV;
		lMeshData[(int)Mesh.ArrayType.TexUV2] = UV2;
		lMeshData[(int)Mesh.ArrayType.Index] = Indices;

		return lMeshData;
	}

	public ArrayMesh GenerateMesh()
	{
		ArrayMesh lMesh = new ArrayMesh();
		lMesh.AddSurfaceFromArrays(
			Mesh.PrimitiveType.Triangles,
			GetMeshData()
		);
		return lMesh;
	}

	public void RecomputeMeshData()
	{
		// Algorithm from http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
		float t = (1f + Mathf.Sqrt(5f)) / 2f;
		List<Vector3> lVertices = new List<Vector3>() {
			new Vector3(-1f,  t,   0),
			new Vector3( 1f,  t,   0),
			new Vector3(-1f, -t,   0),
			new Vector3( 1f, -t,   0),

			new Vector3( 0f, -1f,  t),
			new Vector3( 0f,  1f,  t),
			new Vector3( 0f, -1f, -t),
			new Vector3( 0f,  1f, -t),

			new Vector3( t,   0f, -1f),
			new Vector3( t,   0f,  1f),
			new Vector3(-t,   0f, -1f),
			new Vector3(-t,   0f,  1f),
		};

		List<int> lRawTriangles = new List<int>() {
			11, 0,  5,
			5,  0,  1,
			1,  0,  7,
			7,  0,  10,
			10, 0,  11,

			5,  1,  9,
			11, 5,  4,
			10, 11, 2,
			7,  10, 6,
			1,  7,  8,

			9,  3,  4,
			4,  3,  2,
			2,  3,  6,
			6,  3,  8,
			8,  3,  9,

			9,  4,  5,
			4,  2,  11,
			2,  6,  10,
			6,  8,  7,
			8,  9,  1,
		};

		List<int> lTriangles = new List<int>();

		for (int i = 0; i < Precision; i++)
		{
			lTriangles = new List<int>();

			for (int j = 0; j < lRawTriangles.Count / 3; j++)
			{
				int lA = AddMidPoint(lVertices, lRawTriangles[j * 3],     lRawTriangles[j * 3 + 1]);
				int lB = AddMidPoint(lVertices, lRawTriangles[j * 3 + 1], lRawTriangles[j * 3 + 2]);
				int lC = AddMidPoint(lVertices, lRawTriangles[j * 3 + 2], lRawTriangles[j * 3]);

				lTriangles.AddRange(new int[] {
					lRawTriangles[j * 3],     lA, lC,
					lRawTriangles[j * 3 + 1], lB, lA,
					lRawTriangles[j * 3 + 2], lC, lB,
					lA, lB, lC
				});
			}

			lRawTriangles = lTriangles;
		}

		Vertices = lVertices.ToArray();
		Indices = (lTriangles.Count > 0 ? lTriangles : lRawTriangles).ToArray();

		Normals = new Vector3[Vertices.Length];
		Tangents = new float[Vertices.Length * 4];
		Colors = new Color[Vertices.Length];
		UV = new Vector2[Vertices.Length];
		UV2 = new Vector2[Vertices.Length];

		for (int i = 0; i < Vertices.Length; i++)
		{
			Vertices[i] = Vertices[i].Normalized() * Size;

			if (noiseTexture != null)
			{
				Vertices[i] *= 1f + noiseTexture.Noise.GetNoise3D(
					Vertices[i].X / Size * noiseTexture.Width,
					Vertices[i].Y / Size * noiseTexture.Height,
					Vertices[i].Z / Size * noiseTexture.Depth
				) * Roughness;
			}

			Normals[i] = Vertices[i].Normalized();
			Tangents[i * 4] = 0f;
			Tangents[i * 4 + 1] = 0f;
			Tangents[i * 4 + 2] = 0f;
			Tangents[i * 4 + 3] = 0f;
			Colors[i] = Godot.Colors.White;
			UV[i] = Vector2.Zero;
			UV2[i] = Vector2.Zero;
		}

		Volume = 0f;

		for (int i = 0; i < Indices.Length / 3; i++)
		{
			Vector3 lNormal = (Vertices[Indices[i * 3 + 1]] - Vertices[Indices[i * 3]]).Cross(Vertices[Indices[i * 3 + 2]] - Vertices[Indices[i * 3]]);
			Volume += Vertices[Indices[i * 3]].Dot(lNormal.Normalized()) * (lNormal.Length() * 0.5f);
		}

		Volume = Mathf.Abs(Volume) / 3f;
	}

	private int AddMidPoint(List<Vector3> pPoints, int pA, int pB)
	{
		Vector3 lMid = (pPoints[pA] + pPoints[pB]) * .5f;

		if (pPoints.Contains(lMid))
		{
			return pPoints.IndexOf(lMid);
		}

		pPoints.Add(lMid);
		return pPoints.Count - 1;
	}
}