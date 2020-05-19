using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace UMA.PowerTools
{
	/// <summary>
	/// Utility class for merging multiple skinned meshes.
	/// </summary>
	public static class SkinnedMeshCombinerRetargeting
	{
		/// <summary>
		/// Container for source mesh data.
		/// </summary>
		public class CombineInstance
		{
			public UMAMeshData meshData;
			public int[] targetSubmeshIndices;
			public Matrix4x4[] resolvedBoneMatrixes;
			public int[] targetBoneIndices;
			public System.Int32[][] triangleOcclusion;
		}

		private enum MeshComponents
		{
			none = 0,
			has_normals = 1,
			has_tangents = 2,
			has_colors32 = 4,
			has_uv = 8,
			has_uv2 = 16,
			has_uv3 = 32,
			has_uv4 = 64
		}

		/// <summary>
		/// Combines a set of meshes into the target mesh.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="sources">Sources.</param>
		public static void CombineMeshes(MeshBuilder target, CombineInstance[] sources, Matrix4x4[] inverseTargetBoneMatrixes)
		{
			target.Reset();
			int vertexCount = 0;

			target.PrepareSubMeshCount(FindTargetSubMeshCount(sources));

			AnalyzeSources(sources, target);

			int vertexIndex = 0;
			var context = new SkinningContext();
			context.targetEffectivePoses = inverseTargetBoneMatrixes;

			foreach (var source in sources)
			{
				vertexCount = source.meshData.vertices.Length;
				context.targetBoneIndices = source.targetBoneIndices;

				context.resolvedBoneMatrixes = source.resolvedBoneMatrixes;
				context.vertices = source.meshData.vertices;
				context.normals = source.meshData.normals;
				context.tangents = source.meshData.tangents;
				if (source.meshData.unityBoneWeights == null || source.meshData.unityBoneWeights.Length == 0)
				{
					source.meshData.unityBoneWeights = UMABoneWeight.Convert(source.meshData.boneWeights);
				}
		
				for (int i = 0; i < vertexCount; i++)
				{
					context.ProcessVertex(ref source.meshData.unityBoneWeights[i], i, ref target.boneWeights[vertexIndex + i], ref target.vertices[vertexIndex + i], ref target.normals[vertexIndex + i], ref target.tangents[vertexIndex + i]);
				}

				if (target.has_uv)
				{
					if (source.meshData.uv != null)
					{
						Array.Copy(source.meshData.uv, 0, target.uv, vertexIndex, vertexCount);
					}
					else
					{
						FillArray(target.uv, vertexIndex, vertexCount, Vector4.zero);
					}
				}
				if (target.has_uv2)
				{
					if (source.meshData.uv2 != null)
					{
						Array.Copy(source.meshData.uv2, 0, target.uv2, vertexIndex, vertexCount);
					}
					else
					{
						FillArray(target.uv2, vertexIndex, vertexCount, Vector4.zero);
					}
				}
				if (target.has_uv3)
				{
					if (source.meshData.uv3 != null)
					{
						Array.Copy(source.meshData.uv3, 0, target.uv3, vertexIndex, vertexCount);
					}
					else
					{
						FillArray(target.uv3, vertexIndex, vertexCount, Vector4.zero);
					}
				}
				if (target.has_uv4)
				{
					if (source.meshData.uv4 != null)
					{
						Array.Copy(source.meshData.uv4, 0, target.uv4, vertexIndex, vertexCount);
					}
					else
					{
						FillArray(target.uv4, vertexIndex, vertexCount, Vector4.zero);
					}
				}
				if (target.has_colors32)
				{
					if (source.meshData.colors32 != null && source.meshData.colors32.Length > 0)
					{
						Array.Copy(source.meshData.colors32, 0, target.colors32, vertexIndex, vertexCount);
					}
					else
					{
						Color32 white32 = Color.white;
						FillArray(target.colors32, vertexIndex, vertexCount, white32);
					}
				}

				for (int i = 0; i < source.meshData.subMeshCount; i++)
				{
					if (source.targetSubmeshIndices[i] >= 0)
					{
						int[] subTriangles = source.meshData.submeshes[i].triangles;
						int triangleLength = subTriangles.Length;
						int destMesh = source.targetSubmeshIndices[i];

						var targetSubMesh = target.submeshes[destMesh];
						if (source.triangleOcclusion != null && source.triangleOcclusion[i] != null)
						{
							var triangles = CopyIntArrayAdd(subTriangles, 0, targetSubMesh.triangles, targetSubMesh.triangleCount, triangleLength, vertexIndex, source.triangleOcclusion[i]);
							targetSubMesh.triangleCount += triangles*3;
						}
						else
						{ 
							CopyIntArrayAdd(subTriangles, 0, targetSubMesh.triangles, targetSubMesh.triangleCount, triangleLength, vertexIndex);
							targetSubMesh.triangleCount += triangleLength;
						}
					}
				}

				vertexIndex += vertexCount;
			}
		}

		public static void MergeSortedTransforms(UMATransform[] mergedTransforms, ref int transformCount, UMATransform[] umaTransforms)
		{
			int newBones = 0;
			int pos1 = 0;
			int pos2 = 0;
			int len2 = umaTransforms.Length;

			while(pos1 < transformCount && pos2 < len2 )
			{
				long i = ((long)mergedTransforms[pos1].hash) - ((long)umaTransforms[pos2].hash);
				if (i == 0)
				{
					pos1++;
					pos2++;
				}
				else if (i < 0)
				{
					pos1++;
				}
				else
				{
					pos2++;
					newBones++;
				}
			}
			newBones += len2 - pos2;
			pos1 = transformCount - 1;
			pos2 = len2 - 1;

			transformCount += newBones;

			int dest = transformCount-1;
			while (pos1 >= 0 && pos2 >= 0)
			{
				long i = ((long)mergedTransforms[pos1].hash) - ((long)umaTransforms[pos2].hash);
				if (i == 0)
				{
					mergedTransforms[dest] = mergedTransforms[pos1];
					pos1--;
					pos2--;
				}
				else if (i > 0)
				{
					mergedTransforms[dest] = mergedTransforms[pos1];
					pos1--;
				}
				else
				{
					mergedTransforms[dest] = umaTransforms[pos2];
					pos2--;
				}
				dest--;
			}
			while (pos2 >= 0)
			{
				mergedTransforms[dest] = umaTransforms[pos2];
				pos2--;
				dest--;
			}
		}

		private static void AnalyzeSources(CombineInstance[] sources, MeshBuilder target)
		{
			foreach (var source in sources)
			{
				target.vertexCount += source.meshData.vertices.Length;
				target.has_normals |= (source.meshData.normals != null && source.meshData.normals.Length != 0);
				target.has_tangents |= (source.meshData.tangents != null && source.meshData.tangents.Length != 0);
				target.has_uv |= (source.meshData.uv != null && source.meshData.uv.Length != 0);
				target.has_uv2 |= (source.meshData.uv2 != null && source.meshData.uv2.Length != 0);
				target.has_uv3 |= (source.meshData.uv3 != null && source.meshData.uv3.Length != 0);
				target.has_uv4 |= (source.meshData.uv4 != null && source.meshData.uv4.Length != 0);
				target.has_colors32 |= (source.meshData.colors32 != null && source.meshData.colors32.Length != 0);

				for (int i = 0; i < source.meshData.subMeshCount; i++)
				{
					if (source.targetSubmeshIndices[i] >= 0)
					{
						target.submeshes[source.targetSubmeshIndices[i]].triangleCount += source.meshData.submeshes[i].triangles.Length;
					}
				}
			}
			target.PrepareBuffers();
		}

		private static int FindTargetSubMeshCount(CombineInstance[] sources)
		{
			int highestTargetIndex = -1;
			foreach (var source in sources)
			{
				foreach (var targetIndex in source.targetSubmeshIndices)
				{
					if (highestTargetIndex < targetIndex)
					{
						highestTargetIndex = targetIndex;
					}
				}
			}
			return highestTargetIndex + 1;
		}

		private static void BuildBoneWeights(UMABoneWeight[] source, int sourceIndex, BoneWeight[] dest, int destIndex, int count, int[] bones, Matrix4x4[] bindPoses, Dictionary<int, BoneIndexEntry> bonesCollection, List<Matrix4x4> bindPosesList, List<int> bonesList)
		{
			int[] boneMapping = new int[bones.Length];
			for (int i = 0; i < boneMapping.Length; i++)
			{
				boneMapping[i] = TranslateBoneIndex(i, bones, bindPoses, bonesCollection, bindPosesList, bonesList);
			}

			while (count-- > 0)
			{
				TranslateBoneWeight(ref source[sourceIndex++], ref dest[destIndex++], boneMapping);
			}
		}

		private static void TranslateBoneWeight(ref UMABoneWeight source, ref BoneWeight dest, int[] boneMapping)
		{
			dest.weight0 = source.weight0;
			dest.weight1 = source.weight1;
			dest.weight2 = source.weight2;
			dest.weight3 = source.weight3;

			dest.boneIndex0 = boneMapping[source.boneIndex0];
			dest.boneIndex1 = boneMapping[source.boneIndex1];
			dest.boneIndex2 = boneMapping[source.boneIndex2];
			dest.boneIndex3 = boneMapping[source.boneIndex3];
		}

		private struct BoneIndexEntry
		{
			public int index;
			public List<int> indices;
			public int Count { get { return index >= 0 ? 1 : indices.Count; } }
			public int this[int idx]
			{
				get
				{
					if (index >= 0)
					{
						if (idx == 0) return index;
						throw new ArgumentOutOfRangeException();
					}
					return indices[idx];
				}
			}

			internal void AddIndex(int idx)
			{
				if (index >= 0)
				{
					indices = new List<int>(10);
					indices.Add(index);
					index = -1;
				}
				indices.Add(idx);
			}
		}

		private static bool CompareSkinningMatrices(Matrix4x4 m1, ref Matrix4x4 m2)
		{
			if (Mathf.Abs(m1.m00 - m2.m00) > 0.0001) return false;
			if (Mathf.Abs(m1.m01 - m2.m01) > 0.0001) return false;
			if (Mathf.Abs(m1.m02 - m2.m02) > 0.0001) return false;
			if (Mathf.Abs(m1.m03 - m2.m03) > 0.0001) return false;
			if (Mathf.Abs(m1.m10 - m2.m10) > 0.0001) return false;
			if (Mathf.Abs(m1.m11 - m2.m11) > 0.0001) return false;
			if (Mathf.Abs(m1.m12 - m2.m12) > 0.0001) return false;
			if (Mathf.Abs(m1.m13 - m2.m13) > 0.0001) return false;
			if (Mathf.Abs(m1.m20 - m2.m20) > 0.0001) return false;
			if (Mathf.Abs(m1.m21 - m2.m21) > 0.0001) return false;
			if (Mathf.Abs(m1.m22 - m2.m22) > 0.0001) return false;
			if (Mathf.Abs(m1.m23 - m2.m23) > 0.0001) return false;
			// These never change in a TRS Matrix4x4
//			if (Mathf.Abs(m1.m30 - m2.m30) > 0.0001) return false;
//			if (Mathf.Abs(m1.m31 - m2.m31) > 0.0001) return false;
//			if (Mathf.Abs(m1.m32 - m2.m32) > 0.0001) return false;
//			if (Mathf.Abs(m1.m33 - m2.m33) > 0.0001) return false;
			return true;
		}

		private static int TranslateBoneIndex(int index, int[] bonesHashes, Matrix4x4[] bindPoses, Dictionary<int, BoneIndexEntry> bonesCollection, List<Matrix4x4> bindPosesList, List<int> bonesList)
		{
			var boneTransform = bonesHashes[index];
			BoneIndexEntry entry;
			if (bonesCollection.TryGetValue(boneTransform, out entry))
			{
				for (int i = 0; i < entry.Count; i++)
				{
					var res = entry[i];
					if (CompareSkinningMatrices(bindPosesList[res], ref bindPoses[index]))
					{
						return res;
					}
				}
				var idx = bindPosesList.Count;
				entry.AddIndex(idx);
				bindPosesList.Add(bindPoses[index]);
				bonesList.Add(boneTransform);
				return idx;
			}
			else
			{
				var idx = bindPosesList.Count;
				bonesCollection.Add(boneTransform, new BoneIndexEntry() { index = idx });
				bindPosesList.Add(bindPoses[index]);
				bonesList.Add(boneTransform);
				return idx;
			}
		}

		private static void CopyColorsToColors32(Color[] source, int sourceIndex, Color32[] dest, int destIndex, int count)
		{
			while (count-- > 0)
			{
				var sColor = source[sourceIndex++];
				dest[destIndex++] = new Color32((byte)Mathf.RoundToInt(sColor.r * 255f), (byte)Mathf.RoundToInt(sColor.g * 255f), (byte)Mathf.RoundToInt(sColor.b * 255f), (byte)Mathf.RoundToInt(sColor.a * 255f));
			}
		}

		private static void FillArray(Vector4[] array, int index, int count, Vector4 value)
		{
			while (count-- > 0)
			{
				array[index++] = value;
			}
		}

		private static void FillArray(Vector3[] array, int index, int count, Vector3 value)
		{
			while (count-- > 0)
			{
				array[index++] = value;
			}
		}

		private static void FillArray(Vector2[] array, int index, int count, Vector2 value)
		{
			while (count-- > 0)
			{
				array[index++] = value;
			}
		}

		private static void FillArray(Color[] array, int index, int count, Color value)
		{
			while (count-- > 0)
			{
				array[index++] = value;
			}
		}

		private static void FillArray(Color32[] array, int index, int count, Color32 value)
		{
			while (count-- > 0)
			{
				array[index++] = value;
			}
		}

		private static int CopyIntArrayAdd(int[] source, int sourceIndex, int[] dest, int destIndex, int count, int add, Int32[] mask)
		{
			int copied = 0;
			UInt32 maskValue;
			UInt32 maskIterator;
			int maskIndex = 0;
			count += sourceIndex;
			while (sourceIndex < count)
			{
				maskValue = (System.UInt32)mask[maskIndex++];
				maskIterator = 1;
				while (maskIterator != 0)
				{
					if ((maskValue & maskIterator) != 0)
					{
						dest[destIndex++] = source[sourceIndex++] + add;
						dest[destIndex++] = source[sourceIndex++] + add;
						dest[destIndex++] = source[sourceIndex++] + add;
						copied++;
					}
					else
					{
						sourceIndex += 3;
					}
					maskIterator = maskIterator << 1;
				}
			}
			return copied;
		}

		private static void CopyIntArrayAdd(int[] source, int sourceIndex, int[] dest, int destIndex, int count, int add)
		{
			for (int i = 0; i < count; i++)
			{
				dest[destIndex++] = source[sourceIndex++] + add;
			}
		}

		private static T[] EnsureArrayLength<T>(T[] oldArray, int newLength)
		{
			if (newLength <= 0)
				return null;

			if (oldArray != null && oldArray.Length >= newLength)
				return oldArray;

			return new T[newLength];
		}
	}
}
