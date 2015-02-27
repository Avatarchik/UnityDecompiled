using System;
using System.Collections;
using System.Collections.Generic;
namespace UnityEngine
{
	internal class InternalStaticBatchingUtility
	{
		internal class SortGO : IComparer
		{
			int IComparer.Compare(object a, object b)
			{
				if (a == b)
				{
					return 0;
				}
				Renderer renderer = InternalStaticBatchingUtility.SortGO.GetRenderer(a as GameObject);
				Renderer renderer2 = InternalStaticBatchingUtility.SortGO.GetRenderer(b as GameObject);
				int num = InternalStaticBatchingUtility.SortGO.GetMaterialId(renderer).CompareTo(InternalStaticBatchingUtility.SortGO.GetMaterialId(renderer2));
				if (num == 0)
				{
					num = InternalStaticBatchingUtility.SortGO.GetLightmapIndex(renderer).CompareTo(InternalStaticBatchingUtility.SortGO.GetLightmapIndex(renderer2));
				}
				return num;
			}
			private static int GetMaterialId(Renderer renderer)
			{
				if (renderer == null || renderer.sharedMaterial == null)
				{
					return 0;
				}
				return renderer.sharedMaterial.GetInstanceID();
			}
			private static int GetLightmapIndex(Renderer renderer)
			{
				if (renderer == null)
				{
					return -1;
				}
				return renderer.lightmapIndex;
			}
			private static Renderer GetRenderer(GameObject go)
			{
				if (go == null)
				{
					return null;
				}
				MeshFilter meshFilter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
				if (meshFilter == null)
				{
					return null;
				}
				return meshFilter.renderer;
			}
		}
		private const int MaxVerticesInBatch = 64000;
		private const string CombinedMeshPrefix = "Combined Mesh";
		public static void Combine(GameObject staticBatchRoot)
		{
			InternalStaticBatchingUtility.Combine(staticBatchRoot, false);
		}
		public static void Combine(GameObject staticBatchRoot, bool combineOnlyStatic)
		{
			GameObject[] array = (GameObject[])Object.FindObjectsOfType(typeof(GameObject));
			List<GameObject> list = new List<GameObject>();
			GameObject[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				GameObject gameObject = array2[i];
				if (!(staticBatchRoot != null) || gameObject.transform.IsChildOf(staticBatchRoot.transform))
				{
					if (!combineOnlyStatic || gameObject.isStaticBatchable)
					{
						list.Add(gameObject);
					}
				}
			}
			array = list.ToArray();
			if (!Application.HasProLicense() && !Application.HasAdvancedLicense() && staticBatchRoot != null && array.Length > 0)
			{
				Debug.LogError("Your Unity license is not sufficient for Static Batching.");
			}
			InternalStaticBatchingUtility.Combine(array, staticBatchRoot);
		}
		public static void Combine(GameObject[] gos, GameObject staticBatchRoot)
		{
			Matrix4x4 lhs = Matrix4x4.identity;
			Transform staticBatchRootTransform = null;
			if (staticBatchRoot)
			{
				lhs = staticBatchRoot.transform.worldToLocalMatrix;
				staticBatchRootTransform = staticBatchRoot.transform;
			}
			int batchIndex = 0;
			int num = 0;
			List<MeshSubsetCombineUtility.MeshInstance> list = new List<MeshSubsetCombineUtility.MeshInstance>();
			List<MeshSubsetCombineUtility.SubMeshInstance> list2 = new List<MeshSubsetCombineUtility.SubMeshInstance>();
			List<GameObject> list3 = new List<GameObject>();
			Array.Sort(gos, new InternalStaticBatchingUtility.SortGO());
			for (int i = 0; i < gos.Length; i++)
			{
				GameObject gameObject = gos[i];
				MeshFilter meshFilter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
				if (!(meshFilter == null))
				{
					if (!(meshFilter.sharedMesh == null))
					{
						if (meshFilter.sharedMesh.canAccess)
						{
							if (!(meshFilter.renderer == null) && meshFilter.renderer.enabled)
							{
								if (meshFilter.renderer.staticBatchIndex == 0)
								{
									if (num + meshFilter.sharedMesh.vertexCount > 64000)
									{
										InternalStaticBatchingUtility.MakeBatch(list, list2, list3, staticBatchRootTransform, batchIndex++);
										list.Clear();
										list2.Clear();
										list3.Clear();
										num = 0;
									}
									MeshSubsetCombineUtility.MeshInstance item = default(MeshSubsetCombineUtility.MeshInstance);
									Mesh sharedMesh = meshFilter.sharedMesh;
									item.meshInstanceID = sharedMesh.GetInstanceID();
									item.transform = lhs * meshFilter.transform.localToWorldMatrix;
									item.lightmapTilingOffset = meshFilter.renderer.lightmapTilingOffset;
									list.Add(item);
									Material[] array = meshFilter.renderer.sharedMaterials;
									if (array.Length > sharedMesh.subMeshCount)
									{
										Debug.LogWarning(string.Concat(new object[]
										{
											"Mesh has more materials (",
											array.Length,
											") than subsets (",
											sharedMesh.subMeshCount,
											")"
										}));
										Material[] array2 = new Material[sharedMesh.subMeshCount];
										for (int j = 0; j < sharedMesh.subMeshCount; j++)
										{
											array2[j] = meshFilter.renderer.sharedMaterials[j];
										}
										meshFilter.renderer.sharedMaterials = array2;
										array = array2;
									}
									for (int k = 0; k < Math.Min(array.Length, sharedMesh.subMeshCount); k++)
									{
										list2.Add(new MeshSubsetCombineUtility.SubMeshInstance
										{
											meshInstanceID = meshFilter.sharedMesh.GetInstanceID(),
											vertexOffset = num,
											subMeshIndex = k,
											gameObjectInstanceID = gameObject.GetInstanceID(),
											transform = item.transform
										});
										list3.Add(gameObject);
									}
									num += sharedMesh.vertexCount;
								}
							}
						}
					}
				}
			}
			InternalStaticBatchingUtility.MakeBatch(list, list2, list3, staticBatchRootTransform, batchIndex);
		}
		private static void MakeBatch(List<MeshSubsetCombineUtility.MeshInstance> meshes, List<MeshSubsetCombineUtility.SubMeshInstance> subsets, List<GameObject> subsetGOs, Transform staticBatchRootTransform, int batchIndex)
		{
			if (meshes.Count < 2)
			{
				return;
			}
			MeshSubsetCombineUtility.MeshInstance[] meshes2 = meshes.ToArray();
			MeshSubsetCombineUtility.SubMeshInstance[] array = subsets.ToArray();
			string text = "Combined Mesh";
			text = text + " (root: " + ((!(staticBatchRootTransform != null)) ? "scene" : staticBatchRootTransform.name) + ")";
			if (batchIndex > 0)
			{
				text = text + " " + (batchIndex + 1);
			}
			Mesh mesh = StaticBatchingUtility.InternalCombineVertices(meshes2, text);
			StaticBatchingUtility.InternalCombineIndices(array, mesh);
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				MeshSubsetCombineUtility.SubMeshInstance subMeshInstance = array[i];
				GameObject gameObject = subsetGOs[i];
				Mesh sharedMesh = mesh;
				MeshFilter meshFilter = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
				meshFilter.sharedMesh = sharedMesh;
				gameObject.renderer.SetSubsetIndex(subMeshInstance.subMeshIndex, num);
				gameObject.renderer.staticBatchRootTransform = staticBatchRootTransform;
				gameObject.renderer.enabled = false;
				gameObject.renderer.enabled = true;
				num++;
			}
		}
	}
}
