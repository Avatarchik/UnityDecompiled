using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(ProceduralMaterial))]
	internal class ProceduralMaterialInspector : MaterialEditor
	{
		private class Styles
		{
			public GUIContent hslContent = new GUIContent("HSL Adjustment", "Hue_Shift, Saturation, Luminosity");
			public GUIContent randomSeedContent = new GUIContent("Random Seed", "$randomseed : the overall random aspect of the texture.");
			public GUIContent randomizeButtonContent = new GUIContent("Randomize");
			public GUIContent generateAllOutputsContent = new GUIContent("Generate all outputs", "Force the generation of all Substance outputs.");
			public GUIContent animatedContent = new GUIContent("Animation update rate", "Set the animation update rate in millisecond");
			public GUIContent defaultPlatform = EditorGUIUtility.TextContent("TextureImporter.Platforms.Default");
			public GUIContent targetWidth = new GUIContent("Target Width");
			public GUIContent targetHeight = new GUIContent("Target Height");
			public GUIContent textureFormat = EditorGUIUtility.TextContent("TextureImporter.TextureFormat");
			public GUIContent loadBehavior = new GUIContent("Load Behavior");
			public GUIContent mipmapContent = new GUIContent("Generate Mip Maps");
		}
		[Serializable]
		protected class ProceduralPlatformSetting
		{
			private UnityEngine.Object[] targets;
			public string name;
			public bool m_Overridden;
			public int maxTextureWidth;
			public int maxTextureHeight;
			public int m_TextureFormat;
			public int m_LoadBehavior;
			public BuildTarget target;
			public Texture2D icon;
			public bool isDefault
			{
				get
				{
					return this.name == string.Empty;
				}
			}
			public int textureFormat
			{
				get
				{
					return this.m_TextureFormat;
				}
				set
				{
					this.m_TextureFormat = value;
				}
			}
			public bool overridden
			{
				get
				{
					return this.m_Overridden;
				}
			}
			public ProceduralPlatformSetting(UnityEngine.Object[] objects, string _name, BuildTarget _target, Texture2D _icon)
			{
				this.targets = objects;
				this.m_Overridden = false;
				this.target = _target;
				this.name = _name;
				this.icon = _icon;
				this.m_Overridden = false;
				if (this.name != string.Empty)
				{
					UnityEngine.Object[] array = this.targets;
					for (int i = 0; i < array.Length; i++)
					{
						ProceduralMaterial proceduralMaterial = (ProceduralMaterial)array[i];
						string assetPath = AssetDatabase.GetAssetPath(proceduralMaterial);
						SubstanceImporter substanceImporter = AssetImporter.GetAtPath(assetPath) as SubstanceImporter;
						if (substanceImporter != null && substanceImporter.GetPlatformTextureSettings(proceduralMaterial.name, this.name, out this.maxTextureWidth, out this.maxTextureHeight, out this.m_TextureFormat, out this.m_LoadBehavior))
						{
							this.m_Overridden = true;
							break;
						}
					}
				}
				if (!this.m_Overridden && this.targets.Length > 0)
				{
					string assetPath2 = AssetDatabase.GetAssetPath(this.targets[0]);
					SubstanceImporter substanceImporter2 = AssetImporter.GetAtPath(assetPath2) as SubstanceImporter;
					if (substanceImporter2 != null)
					{
						substanceImporter2.GetPlatformTextureSettings((this.targets[0] as ProceduralMaterial).name, string.Empty, out this.maxTextureWidth, out this.maxTextureHeight, out this.m_TextureFormat, out this.m_LoadBehavior);
					}
				}
			}
			public void SetOverride(ProceduralMaterialInspector.ProceduralPlatformSetting master)
			{
				this.m_Overridden = true;
			}
			public void ClearOverride(ProceduralMaterialInspector.ProceduralPlatformSetting master)
			{
				this.m_TextureFormat = master.textureFormat;
				this.maxTextureWidth = master.maxTextureWidth;
				this.maxTextureHeight = master.maxTextureHeight;
				this.m_LoadBehavior = master.m_LoadBehavior;
				this.m_Overridden = false;
			}
			public bool HasChanged()
			{
				ProceduralMaterialInspector.ProceduralPlatformSetting proceduralPlatformSetting = new ProceduralMaterialInspector.ProceduralPlatformSetting(this.targets, this.name, this.target, null);
				return proceduralPlatformSetting.m_Overridden != this.m_Overridden || proceduralPlatformSetting.maxTextureWidth != this.maxTextureWidth || proceduralPlatformSetting.maxTextureHeight != this.maxTextureHeight || proceduralPlatformSetting.textureFormat != this.textureFormat || proceduralPlatformSetting.m_LoadBehavior != this.m_LoadBehavior;
			}
			public void Apply()
			{
				UnityEngine.Object[] array = this.targets;
				for (int i = 0; i < array.Length; i++)
				{
					ProceduralMaterial proceduralMaterial = (ProceduralMaterial)array[i];
					string assetPath = AssetDatabase.GetAssetPath(proceduralMaterial);
					SubstanceImporter substanceImporter = AssetImporter.GetAtPath(assetPath) as SubstanceImporter;
					if (this.name != string.Empty)
					{
						if (this.m_Overridden)
						{
							substanceImporter.SetPlatformTextureSettings(proceduralMaterial.name, this.name, this.maxTextureWidth, this.maxTextureHeight, this.m_TextureFormat, this.m_LoadBehavior);
						}
						else
						{
							substanceImporter.ClearPlatformTextureSettings(proceduralMaterial.name, this.name);
						}
					}
					else
					{
						substanceImporter.SetPlatformTextureSettings(proceduralMaterial.name, this.name, this.maxTextureWidth, this.maxTextureHeight, this.m_TextureFormat, this.m_LoadBehavior);
					}
				}
			}
		}
		private static ProceduralMaterial m_Material = null;
		private static Shader m_ShaderPMaterial = null;
		private static SubstanceImporter m_Importer = null;
		private static string[] kMaxTextureSizeStrings = new string[]
		{
			"32",
			"64",
			"128",
			"256",
			"512",
			"1024",
			"2048"
		};
		private static int[] kMaxTextureSizeValues = new int[]
		{
			32,
			64,
			128,
			256,
			512,
			1024,
			2048
		};
		private bool m_ShowTexturesSection;
		private bool m_ShowHSLInputs = true;
		private string m_LastGroup;
		private ProceduralMaterialInspector.Styles m_Styles;
		private static string[] kMaxLoadBehaviorStrings = new string[]
		{
			"Do nothing",
			"Do nothing and cache",
			"Build on level load",
			"Build on level load and cache",
			"Bake and keep Substance",
			"Bake and discard Substance"
		};
		private static int[] kMaxLoadBehaviorValues = new int[]
		{
			0,
			5,
			1,
			4,
			2,
			3
		};
		private static string[] kTextureFormatStrings = new string[]
		{
			"Compressed",
			"Compressed - No Alpha",
			"RAW",
			"RAW - No Alpha"
		};
		private static int[] kTextureFormatValues = new int[]
		{
			0,
			2,
			1,
			3
		};
		private bool m_MightHaveModified;
		private static bool m_UndoWasPerformed = false;
		private static Dictionary<ProceduralMaterial, float> m_GeneratingSince = new Dictionary<ProceduralMaterial, float>();
		private bool m_ReimportOnDisable = true;
		private Vector2 m_ScrollPos = default(Vector2);
		protected List<ProceduralMaterialInspector.ProceduralPlatformSetting> m_PlatformSettings;
		public void DisableReimportOnDisable()
		{
			this.m_ReimportOnDisable = false;
		}
		public void ReimportSubstances()
		{
			string[] array = new string[base.targets.GetLength(0)];
			int num = 0;
			UnityEngine.Object[] targets = base.targets;
			for (int i = 0; i < targets.Length; i++)
			{
				ProceduralMaterial assetObject = (ProceduralMaterial)targets[i];
				array[num++] = AssetDatabase.GetAssetPath(assetObject);
			}
			for (int j = 0; j < num; j++)
			{
				SubstanceImporter substanceImporter = AssetImporter.GetAtPath(array[j]) as SubstanceImporter;
				if (substanceImporter && EditorUtility.IsDirty(substanceImporter.GetInstanceID()))
				{
					AssetDatabase.ImportAsset(array[j], ImportAssetOptions.ForceUncompressedImport);
				}
			}
		}
		public override void Awake()
		{
			base.Awake();
			this.m_ShowTexturesSection = EditorPrefs.GetBool("ProceduralShowTextures", false);
			this.m_ReimportOnDisable = true;
			ProceduralMaterialInspector.m_UndoWasPerformed = false;
		}
		public override void OnEnable()
		{
			base.OnEnable();
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
		}
		public void ReimportSubstancesIfNeeded()
		{
			if (this.m_MightHaveModified && !ProceduralMaterialInspector.m_UndoWasPerformed && !EditorApplication.isPlaying && !InternalEditorUtility.ignoreInspectorChanges)
			{
				this.ReimportSubstances();
			}
		}
		public override void OnDisable()
		{
			if (this.m_ReimportOnDisable)
			{
				this.ReimportSubstancesIfNeeded();
			}
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
			base.OnDisable();
		}
		public override void UndoRedoPerformed()
		{
			ProceduralMaterialInspector.m_UndoWasPerformed = true;
			base.UndoRedoPerformed();
			if (ProceduralMaterialInspector.m_Material != null)
			{
				ProceduralMaterialInspector.m_Material.RebuildTextures();
			}
			base.Repaint();
		}
		internal void DisplayRestrictedInspector()
		{
			this.m_MightHaveModified = false;
			if (this.m_Styles == null)
			{
				this.m_Styles = new ProceduralMaterialInspector.Styles();
			}
			ProceduralMaterial proceduralMaterial = this.target as ProceduralMaterial;
			if (ProceduralMaterialInspector.m_Material != proceduralMaterial)
			{
				ProceduralMaterialInspector.m_Material = proceduralMaterial;
				ProceduralMaterialInspector.m_ShaderPMaterial = proceduralMaterial.shader;
			}
			this.ProceduralProperties();
			GUILayout.Space(15f);
			this.GeneratedTextures();
		}
		internal override void OnAssetStoreInspectorGUI()
		{
			this.DisplayRestrictedInspector();
		}
		internal override bool IsEnabled()
		{
			return base.IsOpenForEdit();
		}
		internal override void OnHeaderTitleGUI(Rect titleRect, string header)
		{
			ProceduralMaterial proceduralMaterial = this.target as ProceduralMaterial;
			string assetPath = AssetDatabase.GetAssetPath(this.target);
			ProceduralMaterialInspector.m_Importer = (AssetImporter.GetAtPath(assetPath) as SubstanceImporter);
			if (ProceduralMaterialInspector.m_Importer == null)
			{
				return;
			}
			string text = proceduralMaterial.name;
			text = EditorGUI.DelayedTextField(titleRect, text, null, EditorStyles.textField);
			if (text != proceduralMaterial.name)
			{
				if (ProceduralMaterialInspector.m_Importer.RenameMaterial(proceduralMaterial, text))
				{
					AssetDatabase.ImportAsset(ProceduralMaterialInspector.m_Importer.assetPath, ImportAssetOptions.ForceUncompressedImport);
					GUIUtility.ExitGUI();
				}
				else
				{
					text = proceduralMaterial.name;
				}
			}
		}
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
			this.m_MightHaveModified = true;
			if (this.m_Styles == null)
			{
				this.m_Styles = new ProceduralMaterialInspector.Styles();
			}
			ProceduralMaterial proceduralMaterial = this.target as ProceduralMaterial;
			string assetPath = AssetDatabase.GetAssetPath(this.target);
			ProceduralMaterialInspector.m_Importer = (AssetImporter.GetAtPath(assetPath) as SubstanceImporter);
			if (ProceduralMaterialInspector.m_Importer == null)
			{
				this.DisplayRestrictedInspector();
				return;
			}
			if (ProceduralMaterialInspector.m_Material != proceduralMaterial)
			{
				ProceduralMaterialInspector.m_Material = proceduralMaterial;
				ProceduralMaterialInspector.m_ShaderPMaterial = proceduralMaterial.shader;
			}
			if (!base.isVisible || proceduralMaterial.shader == null)
			{
				return;
			}
			if (ProceduralMaterialInspector.m_ShaderPMaterial != proceduralMaterial.shader)
			{
				ProceduralMaterialInspector.m_ShaderPMaterial = proceduralMaterial.shader;
				UnityEngine.Object[] targets = base.targets;
				for (int i = 0; i < targets.Length; i++)
				{
					ProceduralMaterial proceduralMaterial2 = (ProceduralMaterial)targets[i];
					string assetPath2 = AssetDatabase.GetAssetPath(proceduralMaterial2);
					SubstanceImporter substanceImporter = AssetImporter.GetAtPath(assetPath2) as SubstanceImporter;
					substanceImporter.OnShaderModified(proceduralMaterial2);
				}
			}
			if (base.PropertiesGUI())
			{
				ProceduralMaterialInspector.m_ShaderPMaterial = proceduralMaterial.shader;
				UnityEngine.Object[] targets2 = base.targets;
				for (int j = 0; j < targets2.Length; j++)
				{
					ProceduralMaterial proceduralMaterial3 = (ProceduralMaterial)targets2[j];
					string assetPath3 = AssetDatabase.GetAssetPath(proceduralMaterial3);
					SubstanceImporter substanceImporter2 = AssetImporter.GetAtPath(assetPath3) as SubstanceImporter;
					substanceImporter2.OnShaderModified(proceduralMaterial3);
				}
				base.PropertiesChanged();
			}
			GUILayout.Space(5f);
			this.ProceduralProperties();
			GUILayout.Space(15f);
			this.GeneratedTextures();
			EditorGUI.EndDisabledGroup();
		}
		private void ProceduralProperties()
		{
			GUILayout.Label("Procedural Properties", EditorStyles.boldLabel, new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(true)
			});
			UnityEngine.Object[] targets = base.targets;
			for (int i = 0; i < targets.Length; i++)
			{
				ProceduralMaterial proceduralMaterial = (ProceduralMaterial)targets[i];
				if (proceduralMaterial.isProcessing)
				{
					base.Repaint();
					SceneView.RepaintAll();
					GameView.RepaintAll();
					break;
				}
			}
			if (base.targets.Length > 1)
			{
				GUILayout.Label("Procedural properties do not support multi-editing.", EditorStyles.wordWrappedLabel, new GUILayoutOption[0]);
				return;
			}
			EditorGUIUtility.labelWidth = 0f;
			EditorGUIUtility.fieldWidth = 0f;
			if (ProceduralMaterialInspector.m_Importer != null)
			{
				if (!ProceduralMaterial.isSupported)
				{
					GUILayout.Label("Procedural Materials are not supported on " + EditorUserBuildSettings.activeBuildTarget + ". Textures will be baked.", EditorStyles.helpBox, new GUILayoutOption[]
					{
						GUILayout.ExpandWidth(true)
					});
				}
				bool changed = GUI.changed;
				EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
				EditorGUI.BeginChangeCheck();
				bool generated = EditorGUILayout.Toggle(this.m_Styles.generateAllOutputsContent, ProceduralMaterialInspector.m_Importer.GetGenerateAllOutputs(ProceduralMaterialInspector.m_Material), new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck())
				{
					ProceduralMaterialInspector.m_Importer.SetGenerateAllOutputs(ProceduralMaterialInspector.m_Material, generated);
				}
				EditorGUI.BeginChangeCheck();
				bool mode = EditorGUILayout.Toggle(this.m_Styles.mipmapContent, ProceduralMaterialInspector.m_Importer.GetGenerateMipMaps(ProceduralMaterialInspector.m_Material), new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck())
				{
					ProceduralMaterialInspector.m_Importer.SetGenerateMipMaps(ProceduralMaterialInspector.m_Material, mode);
				}
				EditorGUI.EndDisabledGroup();
				if (ProceduralMaterialInspector.m_Material.HasProceduralProperty("$time"))
				{
					EditorGUI.BeginChangeCheck();
					int animation_update_rate = EditorGUILayout.IntField(this.m_Styles.animatedContent, ProceduralMaterialInspector.m_Importer.GetAnimationUpdateRate(ProceduralMaterialInspector.m_Material), new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						ProceduralMaterialInspector.m_Importer.SetAnimationUpdateRate(ProceduralMaterialInspector.m_Material, animation_update_rate);
					}
				}
				GUI.changed = changed;
			}
			this.InputOptions(ProceduralMaterialInspector.m_Material);
		}
		private void GeneratedTextures()
		{
			if (base.targets.Length > 1)
			{
				return;
			}
			string text = "Generated Textures";
			if (ProceduralMaterialInspector.ShowIsGenerating(this.target as ProceduralMaterial))
			{
				text += " (Generating...)";
			}
			EditorGUI.BeginChangeCheck();
			this.m_ShowTexturesSection = EditorGUILayout.Foldout(this.m_ShowTexturesSection, text);
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetBool("ProceduralShowTextures", this.m_ShowTexturesSection);
			}
			if (this.m_ShowTexturesSection)
			{
				this.ShowProceduralTexturesGUI(ProceduralMaterialInspector.m_Material);
				this.ShowGeneratedTexturesGUI(ProceduralMaterialInspector.m_Material);
				if (ProceduralMaterialInspector.m_Importer != null)
				{
					if (this.HasProceduralTextureProperties(ProceduralMaterialInspector.m_Material))
					{
						this.OffsetScaleGUI(ProceduralMaterialInspector.m_Material);
					}
					EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
					this.ShowTextureSizeGUI();
					EditorGUI.EndDisabledGroup();
				}
			}
		}
		public static bool ShowIsGenerating(ProceduralMaterial mat)
		{
			if (!ProceduralMaterialInspector.m_GeneratingSince.ContainsKey(mat))
			{
				ProceduralMaterialInspector.m_GeneratingSince[mat] = 0f;
			}
			if (mat.isProcessing)
			{
				return Time.realtimeSinceStartup > ProceduralMaterialInspector.m_GeneratingSince[mat] + 0.4f;
			}
			ProceduralMaterialInspector.m_GeneratingSince[mat] = Time.realtimeSinceStartup;
			return false;
		}
		public override string GetInfoString()
		{
			ProceduralMaterial proceduralMaterial = this.target as ProceduralMaterial;
			if (proceduralMaterial.mainTexture == null)
			{
				return string.Empty;
			}
			return proceduralMaterial.mainTexture.width + "x" + proceduralMaterial.mainTexture.height;
		}
		public bool HasProceduralTextureProperties(Material material)
		{
			Shader shader = material.shader;
			int propertyCount = ShaderUtil.GetPropertyCount(shader);
			for (int i = 0; i < propertyCount; i++)
			{
				if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
				{
					string propertyName = ShaderUtil.GetPropertyName(shader, i);
					Texture texture = material.GetTexture(propertyName);
					if (SubstanceImporter.IsProceduralTextureSlot(material, texture, propertyName))
					{
						return true;
					}
				}
			}
			return false;
		}
		protected void RecordForUndo(ProceduralMaterial material, SubstanceImporter importer, string message)
		{
			if (importer)
			{
				Undo.RecordObjects(new UnityEngine.Object[]
				{
					material,
					importer
				}, message);
			}
			else
			{
				Undo.RecordObject(material, message);
			}
		}
		protected void OffsetScaleGUI(ProceduralMaterial material)
		{
			if (ProceduralMaterialInspector.m_Importer == null || base.targets.Length > 1)
			{
				return;
			}
			GUILayoutOption gUILayoutOption = GUILayout.Width(10f);
			GUILayoutOption gUILayoutOption2 = GUILayout.MinWidth(32f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
			Vector2 materialScale = ProceduralMaterialInspector.m_Importer.GetMaterialScale(material);
			Vector2 lhs = materialScale;
			Vector2 materialOffset = ProceduralMaterialInspector.m_Importer.GetMaterialOffset(material);
			Vector2 lhs2 = materialOffset;
			GUILayout.BeginHorizontal(new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(true)
			});
			GUILayout.Space(8f);
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label(string.Empty, EditorStyles.miniLabel, new GUILayoutOption[]
			{
				gUILayoutOption
			});
			GUILayout.Label("x", EditorStyles.miniLabel, new GUILayoutOption[]
			{
				gUILayoutOption
			});
			GUILayout.Label("y", EditorStyles.miniLabel, new GUILayoutOption[]
			{
				gUILayoutOption
			});
			GUILayout.EndVertical();
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label("Tiling", EditorStyles.miniLabel, new GUILayoutOption[0]);
			materialScale.x = EditorGUILayout.FloatField(materialScale.x, EditorStyles.miniTextField, new GUILayoutOption[]
			{
				gUILayoutOption2
			});
			materialScale.y = EditorGUILayout.FloatField(materialScale.y, EditorStyles.miniTextField, new GUILayoutOption[]
			{
				gUILayoutOption2
			});
			GUILayout.EndVertical();
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label("Offset", EditorStyles.miniLabel, new GUILayoutOption[0]);
			materialOffset.x = EditorGUILayout.FloatField(materialOffset.x, EditorStyles.miniTextField, new GUILayoutOption[]
			{
				gUILayoutOption2
			});
			materialOffset.y = EditorGUILayout.FloatField(materialOffset.y, EditorStyles.miniTextField, new GUILayoutOption[]
			{
				gUILayoutOption2
			});
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			if (lhs != materialScale || lhs2 != materialOffset)
			{
				this.RecordForUndo(material, ProceduralMaterialInspector.m_Importer, "Modify " + material.name + "'s Tiling/Offset");
				ProceduralMaterialInspector.m_Importer.SetMaterialOffset(material, materialOffset);
				ProceduralMaterialInspector.m_Importer.SetMaterialScale(material, materialScale);
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		protected void InputOptions(ProceduralMaterial material)
		{
			EditorGUI.BeginChangeCheck();
			this.InputsGUI();
			if (EditorGUI.EndChangeCheck())
			{
				material.RebuildTextures();
			}
		}
		[MenuItem("CONTEXT/ProceduralMaterial/Reset", false, -100)]
		public static void ResetSubstance(MenuCommand command)
		{
			string assetPath = AssetDatabase.GetAssetPath(command.context);
			ProceduralMaterialInspector.m_Importer = (AssetImporter.GetAtPath(assetPath) as SubstanceImporter);
			ProceduralMaterialInspector.m_Importer.ResetMaterial(command.context as ProceduralMaterial);
		}
		[MenuItem("CONTEXT/ProceduralMaterial/Export Bitmaps", false)]
		public static void ExportBitmaps(MenuCommand command)
		{
			string assetPath = AssetDatabase.GetAssetPath(command.context);
			ProceduralMaterialInspector.m_Importer = (AssetImporter.GetAtPath(assetPath) as SubstanceImporter);
			ProceduralMaterialInspector.m_Importer.ExportBitmaps(command.context as ProceduralMaterial);
		}
		protected void ShowProceduralTexturesGUI(ProceduralMaterial material)
		{
			if (base.targets.Length > 1)
			{
				return;
			}
			EditorGUILayout.Space();
			Shader shader = material.shader;
			if (shader == null)
			{
				return;
			}
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(4f);
			GUILayout.FlexibleSpace();
			float pixels = 10f;
			bool flag = true;
			for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
			{
				if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
				{
					string propertyName = ShaderUtil.GetPropertyName(shader, i);
					Texture texture = material.GetTexture(propertyName);
					if (SubstanceImporter.IsProceduralTextureSlot(material, texture, propertyName))
					{
						string propertyDescription = ShaderUtil.GetPropertyDescription(shader, i);
						Type objType;
						switch (ShaderUtil.GetTexDim(shader, i))
						{
						case ShaderUtil.ShaderPropertyTexDim.TexDim2D:
							objType = typeof(Texture);
							break;
						case ShaderUtil.ShaderPropertyTexDim.TexDim3D:
							objType = typeof(Texture3D);
							break;
						case ShaderUtil.ShaderPropertyTexDim.TexDimCUBE:
							objType = typeof(Cubemap);
							break;
						case ShaderUtil.ShaderPropertyTexDim.TexDimAny:
							objType = typeof(Texture);
							break;
						default:
							objType = null;
							break;
						}
						GUIStyle gUIStyle = "ObjectPickerResultsGridLabel";
						if (flag)
						{
							flag = false;
						}
						else
						{
							GUILayout.Space(pixels);
						}
						GUILayout.BeginVertical(new GUILayoutOption[]
						{
							GUILayout.Height(72f + gUIStyle.fixedHeight + gUIStyle.fixedHeight + 8f)
						});
						Rect rect = GUILayoutUtility.GetRect(72f, 72f);
						ProceduralMaterialInspector.DoObjectPingField(rect, rect, GUIUtility.GetControlID(12354, EditorGUIUtility.native, rect), texture, objType);
						this.ShowAlphaSourceGUI(material, texture as ProceduralTexture, ref rect);
						rect.height = gUIStyle.fixedHeight;
						GUI.Label(rect, propertyDescription, gUIStyle);
						GUILayout.EndVertical();
						GUILayout.FlexibleSpace();
					}
				}
			}
			GUILayout.Space(4f);
			EditorGUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
		}
		protected void ShowGeneratedTexturesGUI(ProceduralMaterial material)
		{
			if (base.targets.Length > 1)
			{
				return;
			}
			if (ProceduralMaterialInspector.m_Importer != null && !ProceduralMaterialInspector.m_Importer.GetGenerateAllOutputs(ProceduralMaterialInspector.m_Material))
			{
				return;
			}
			GUIStyle gUIStyle = "ObjectPickerResultsGridLabel";
			EditorGUILayout.Space();
			GUILayout.FlexibleSpace();
			this.m_ScrollPos = EditorGUILayout.BeginScrollView(this.m_ScrollPos, new GUILayoutOption[]
			{
				GUILayout.Height(64f + gUIStyle.fixedHeight + gUIStyle.fixedHeight + 16f)
			});
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			float pixels = 10f;
			Texture[] generatedTextures = material.GetGeneratedTextures();
			for (int i = 0; i < generatedTextures.Length; i++)
			{
				ProceduralTexture proceduralTexture = generatedTextures[i] as ProceduralTexture;
				if (proceduralTexture != null)
				{
					GUILayout.Space(pixels);
					GUILayout.BeginVertical(new GUILayoutOption[]
					{
						GUILayout.Height(64f + gUIStyle.fixedHeight + 8f)
					});
					Rect rect = GUILayoutUtility.GetRect(64f, 64f);
					ProceduralMaterialInspector.DoObjectPingField(rect, rect, GUIUtility.GetControlID(12354, EditorGUIUtility.native, rect), proceduralTexture, typeof(Texture));
					this.ShowAlphaSourceGUI(material, proceduralTexture, ref rect);
					GUILayout.EndVertical();
					GUILayout.Space(pixels);
					GUILayout.FlexibleSpace();
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();
		}
		private void ShowAlphaSourceGUI(ProceduralMaterial material, ProceduralTexture tex, ref Rect rect)
		{
			GUIStyle gUIStyle = "ObjectPickerResultsGridLabel";
			float num = 10f;
			rect.y = rect.yMax + 2f;
			if (ProceduralMaterialInspector.m_Importer != null)
			{
				EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
				if (tex.GetProceduralOutputType() != ProceduralOutputType.Normal && tex.hasAlpha)
				{
					rect.height = gUIStyle.fixedHeight;
					string[] displayedOptions = new string[]
					{
						"Source (A)",
						"Diffuse (A)",
						"Normal (A)",
						"Height (A)",
						"Emissive (A)",
						"Specular (A)",
						"Opacity (A)"
					};
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUI.BeginChangeCheck();
					int alphaSource = EditorGUI.Popup(rect, (int)ProceduralMaterialInspector.m_Importer.GetTextureAlphaSource(material, tex.name), displayedOptions);
					if (EditorGUI.EndChangeCheck())
					{
						this.RecordForUndo(material, ProceduralMaterialInspector.m_Importer, "Modify " + material.name + "'s Alpha Modifier");
						ProceduralMaterialInspector.m_Importer.SetTextureAlphaSource(material, tex.name, (ProceduralOutputType)alphaSource);
					}
					rect.y = rect.yMax + 2f;
				}
				EditorGUI.EndDisabledGroup();
			}
			rect.width += num;
		}
		private UnityEngine.Object TextureValidator(UnityEngine.Object[] references, Type objType, SerializedProperty property)
		{
			for (int i = 0; i < references.Length; i++)
			{
				UnityEngine.Object @object = references[i];
				Texture texture = @object as Texture;
				if (texture)
				{
					return texture;
				}
			}
			return null;
		}
		internal static void DoObjectPingField(Rect position, Rect dropRect, int id, UnityEngine.Object obj, Type objType)
		{
			Event current = Event.current;
			EventType eventType = current.type;
			if (!GUI.enabled && GUIClip.enabled && Event.current.rawType == EventType.MouseDown)
			{
				eventType = Event.current.rawType;
			}
			bool flag = EditorGUIUtility.HasObjectThumbnail(objType) && position.height > 16f;
			EventType eventType2 = eventType;
			if (eventType2 != EventType.MouseDown)
			{
				if (eventType2 == EventType.Repaint)
				{
					GUIContent gUIContent = EditorGUIUtility.ObjectContent(obj, objType);
					if (flag)
					{
						GUIStyle objectFieldThumb = EditorStyles.objectFieldThumb;
						objectFieldThumb.Draw(position, GUIContent.none, id, DragAndDrop.activeControlID == id);
						if (obj != null)
						{
							EditorGUI.DrawPreviewTexture(objectFieldThumb.padding.Remove(position), gUIContent.image);
						}
						else
						{
							GUIStyle gUIStyle = objectFieldThumb.name + "Overlay";
							gUIStyle.Draw(position, gUIContent, id);
						}
					}
					else
					{
						GUIStyle objectField = EditorStyles.objectField;
						objectField.Draw(position, gUIContent, id, DragAndDrop.activeControlID == id);
					}
				}
			}
			else
			{
				if (Event.current.button == 0)
				{
					if (position.Contains(Event.current.mousePosition))
					{
						UnityEngine.Object @object = obj;
						Component component = @object as Component;
						if (component)
						{
							@object = component.gameObject;
						}
						if (Event.current.clickCount == 1)
						{
							GUIUtility.keyboardControl = id;
							if (@object)
							{
								EditorGUIUtility.PingObject(@object);
							}
							current.Use();
						}
						else
						{
							if (Event.current.clickCount == 2)
							{
								if (@object)
								{
									AssetDatabase.OpenAsset(@object);
									GUIUtility.ExitGUI();
								}
								current.Use();
							}
						}
					}
				}
			}
		}
		internal void ResetValues()
		{
			this.BuildTargetList();
			if (this.HasModified())
			{
				Debug.LogError("Impossible");
			}
		}
		internal void Apply()
		{
			foreach (ProceduralMaterialInspector.ProceduralPlatformSetting current in this.m_PlatformSettings)
			{
				current.Apply();
			}
		}
		internal bool HasModified()
		{
			foreach (ProceduralMaterialInspector.ProceduralPlatformSetting current in this.m_PlatformSettings)
			{
				if (current.HasChanged())
				{
					return true;
				}
			}
			return false;
		}
		public void BuildTargetList()
		{
			List<BuildPlayerWindow.BuildPlatform> validPlatforms = BuildPlayerWindow.GetValidPlatforms();
			this.m_PlatformSettings = new List<ProceduralMaterialInspector.ProceduralPlatformSetting>();
			this.m_PlatformSettings.Add(new ProceduralMaterialInspector.ProceduralPlatformSetting(base.targets, string.Empty, BuildTarget.StandaloneWindows, null));
			foreach (BuildPlayerWindow.BuildPlatform current in validPlatforms)
			{
				this.m_PlatformSettings.Add(new ProceduralMaterialInspector.ProceduralPlatformSetting(base.targets, current.name, current.DefaultTarget, current.smallIcon));
			}
		}
		public void ShowTextureSizeGUI()
		{
			if (this.m_PlatformSettings == null)
			{
				this.BuildTargetList();
			}
			this.TextureSizeGUI();
		}
		protected void TextureSizeGUI()
		{
			BuildPlayerWindow.BuildPlatform[] platforms = BuildPlayerWindow.GetValidPlatforms().ToArray();
			int num = EditorGUILayout.BeginPlatformGrouping(platforms, this.m_Styles.defaultPlatform);
			ProceduralMaterialInspector.ProceduralPlatformSetting proceduralPlatformSetting = this.m_PlatformSettings[num + 1];
			ProceduralMaterialInspector.ProceduralPlatformSetting proceduralPlatformSetting2 = proceduralPlatformSetting;
			bool flag = true;
			if (proceduralPlatformSetting.name != string.Empty)
			{
				EditorGUI.BeginChangeCheck();
				flag = GUILayout.Toggle(proceduralPlatformSetting.overridden, "Override for " + proceduralPlatformSetting.name, new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck())
				{
					if (flag)
					{
						proceduralPlatformSetting.SetOverride(this.m_PlatformSettings[0]);
					}
					else
					{
						proceduralPlatformSetting.ClearOverride(this.m_PlatformSettings[0]);
					}
				}
			}
			EditorGUI.BeginDisabledGroup(!flag);
			EditorGUI.BeginChangeCheck();
			proceduralPlatformSetting2.maxTextureWidth = EditorGUILayout.IntPopup(this.m_Styles.targetWidth.text, proceduralPlatformSetting2.maxTextureWidth, ProceduralMaterialInspector.kMaxTextureSizeStrings, ProceduralMaterialInspector.kMaxTextureSizeValues, new GUILayoutOption[0]);
			proceduralPlatformSetting2.maxTextureHeight = EditorGUILayout.IntPopup(this.m_Styles.targetHeight.text, proceduralPlatformSetting2.maxTextureHeight, ProceduralMaterialInspector.kMaxTextureSizeStrings, ProceduralMaterialInspector.kMaxTextureSizeValues, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck() && proceduralPlatformSetting2.isDefault)
			{
				foreach (ProceduralMaterialInspector.ProceduralPlatformSetting current in this.m_PlatformSettings)
				{
					if (!current.isDefault && !current.overridden)
					{
						current.maxTextureWidth = proceduralPlatformSetting2.maxTextureWidth;
						current.maxTextureHeight = proceduralPlatformSetting2.maxTextureHeight;
					}
				}
			}
			EditorGUI.BeginChangeCheck();
			int num2 = proceduralPlatformSetting2.textureFormat;
			if (num2 < 0 || num2 >= ProceduralMaterialInspector.kTextureFormatStrings.Length)
			{
				Debug.LogError("Invalid TextureFormat");
			}
			num2 = EditorGUILayout.IntPopup(this.m_Styles.textureFormat.text, num2, ProceduralMaterialInspector.kTextureFormatStrings, ProceduralMaterialInspector.kTextureFormatValues, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				proceduralPlatformSetting2.textureFormat = num2;
				if (proceduralPlatformSetting2.isDefault)
				{
					foreach (ProceduralMaterialInspector.ProceduralPlatformSetting current2 in this.m_PlatformSettings)
					{
						if (!current2.isDefault && !current2.overridden)
						{
							current2.textureFormat = proceduralPlatformSetting2.textureFormat;
						}
					}
				}
			}
			EditorGUI.BeginChangeCheck();
			proceduralPlatformSetting2.m_LoadBehavior = EditorGUILayout.IntPopup(this.m_Styles.loadBehavior.text, proceduralPlatformSetting2.m_LoadBehavior, ProceduralMaterialInspector.kMaxLoadBehaviorStrings, ProceduralMaterialInspector.kMaxLoadBehaviorValues, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck() && proceduralPlatformSetting2.isDefault)
			{
				foreach (ProceduralMaterialInspector.ProceduralPlatformSetting current3 in this.m_PlatformSettings)
				{
					if (!current3.isDefault && !current3.overridden)
					{
						current3.m_LoadBehavior = proceduralPlatformSetting2.m_LoadBehavior;
					}
				}
			}
			GUILayout.Space(5f);
			EditorGUI.BeginDisabledGroup(!this.HasModified());
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Revert", new GUILayoutOption[0]))
			{
				this.ResetValues();
			}
			if (GUILayout.Button("Apply", new GUILayoutOption[0]))
			{
				this.Apply();
				this.ReimportSubstances();
				this.ResetValues();
			}
			GUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();
			GUILayout.Space(5f);
			EditorGUILayout.EndPlatformGrouping();
			EditorGUI.EndDisabledGroup();
		}
		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			base.OnPreviewGUI(r, background);
			if (ProceduralMaterialInspector.ShowIsGenerating(this.target as ProceduralMaterial) && r.width > 50f)
			{
				EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 20f), "Generating...");
			}
		}
		public void InputsGUI()
		{
			ProceduralPropertyDescription[] proceduralPropertyDescriptions = ProceduralMaterialInspector.m_Material.GetProceduralPropertyDescriptions();
			ProceduralPropertyDescription proceduralPropertyDescription = null;
			ProceduralPropertyDescription proceduralPropertyDescription2 = null;
			ProceduralPropertyDescription proceduralPropertyDescription3 = null;
			this.m_LastGroup = string.Empty;
			for (int i = 0; i < proceduralPropertyDescriptions.Length; i++)
			{
				ProceduralPropertyDescription proceduralPropertyDescription4 = proceduralPropertyDescriptions[i];
				if (!(proceduralPropertyDescription4.name == "$outputsize"))
				{
					if (proceduralPropertyDescription4.name == "$randomseed")
					{
						this.InputSeedGUI(proceduralPropertyDescription4);
					}
					else
					{
						if (proceduralPropertyDescription4.name == "Hue_Shift" && proceduralPropertyDescription4.group == string.Empty)
						{
							proceduralPropertyDescription = proceduralPropertyDescription4;
						}
						else
						{
							if (proceduralPropertyDescription4.name == "Saturation" && proceduralPropertyDescription4.group == string.Empty)
							{
								proceduralPropertyDescription2 = proceduralPropertyDescription4;
							}
							else
							{
								if (proceduralPropertyDescription4.name == "Luminosity" && proceduralPropertyDescription4.group == string.Empty)
								{
									proceduralPropertyDescription3 = proceduralPropertyDescription4;
								}
								else
								{
									if (proceduralPropertyDescription4.name.Length <= 0 || proceduralPropertyDescription4.name[0] != '$')
									{
										this.InputGUI(proceduralPropertyDescription4);
									}
								}
							}
						}
					}
				}
			}
			if (proceduralPropertyDescription != null && proceduralPropertyDescription2 != null && proceduralPropertyDescription3 != null && proceduralPropertyDescription.type == ProceduralPropertyType.Float && proceduralPropertyDescription2.type == ProceduralPropertyType.Float && proceduralPropertyDescription3.type == ProceduralPropertyType.Float)
			{
				this.InputHSLGUI(proceduralPropertyDescription, proceduralPropertyDescription2, proceduralPropertyDescription3);
			}
			else
			{
				if (proceduralPropertyDescription != null)
				{
					this.InputGUI(proceduralPropertyDescription);
				}
				if (proceduralPropertyDescription2 != null)
				{
					this.InputGUI(proceduralPropertyDescription2);
				}
				if (proceduralPropertyDescription3 != null)
				{
					this.InputGUI(proceduralPropertyDescription3);
				}
			}
		}
		private void InputGUI(ProceduralPropertyDescription input)
		{
			bool flag = true;
			ProceduralMaterial proceduralMaterial = this.target as ProceduralMaterial;
			string name = proceduralMaterial.name;
			string key = name + input.group;
			if (input.group != this.m_LastGroup)
			{
				GUILayout.Space(5f);
				if (input.group != string.Empty)
				{
					this.m_LastGroup = input.group;
					flag = EditorPrefs.GetBool(key, true);
					EditorGUI.BeginChangeCheck();
					flag = EditorGUILayout.Foldout(flag, input.group);
					if (EditorGUI.EndChangeCheck())
					{
						EditorPrefs.SetBool(key, flag);
					}
				}
			}
			else
			{
				flag = EditorPrefs.GetBool(key, true);
			}
			if (flag || input.group == string.Empty)
			{
				int indentLevel = EditorGUI.indentLevel;
				if (input.group != string.Empty)
				{
					EditorGUI.indentLevel++;
				}
				ProceduralPropertyType type = input.type;
				GUIContent gUIContent = new GUIContent(input.label, input.name);
				switch (type)
				{
				case ProceduralPropertyType.Boolean:
				{
					EditorGUI.BeginChangeCheck();
					bool value = EditorGUILayout.Toggle(gUIContent, ProceduralMaterialInspector.m_Material.GetProceduralBoolean(input.name), new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						this.RecordForUndo(ProceduralMaterialInspector.m_Material, ProceduralMaterialInspector.m_Importer, "Modified property " + input.name + " for material " + ProceduralMaterialInspector.m_Material.name);
						ProceduralMaterialInspector.m_Material.SetProceduralBoolean(input.name, value);
					}
					break;
				}
				case ProceduralPropertyType.Float:
				{
					EditorGUI.BeginChangeCheck();
					float value2;
					if (input.hasRange)
					{
						float minimum = input.minimum;
						float maximum = input.maximum;
						value2 = EditorGUILayout.Slider(gUIContent, ProceduralMaterialInspector.m_Material.GetProceduralFloat(input.name), minimum, maximum, new GUILayoutOption[0]);
					}
					else
					{
						value2 = EditorGUILayout.FloatField(gUIContent, ProceduralMaterialInspector.m_Material.GetProceduralFloat(input.name), new GUILayoutOption[0]);
					}
					if (EditorGUI.EndChangeCheck())
					{
						this.RecordForUndo(ProceduralMaterialInspector.m_Material, ProceduralMaterialInspector.m_Importer, "Modified property " + input.name + " for material " + ProceduralMaterialInspector.m_Material.name);
						ProceduralMaterialInspector.m_Material.SetProceduralFloat(input.name, value2);
					}
					break;
				}
				case ProceduralPropertyType.Vector2:
				case ProceduralPropertyType.Vector3:
				case ProceduralPropertyType.Vector4:
				{
					int num = (type != ProceduralPropertyType.Vector2) ? ((type != ProceduralPropertyType.Vector3) ? 4 : 3) : 2;
					Vector4 vector = ProceduralMaterialInspector.m_Material.GetProceduralVector(input.name);
					EditorGUI.BeginChangeCheck();
					if (input.hasRange)
					{
						float minimum2 = input.minimum;
						float maximum2 = input.maximum;
						EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
						GUILayout.Label(gUIContent, new GUILayoutOption[0]);
						EditorGUI.indentLevel++;
						for (int i = 0; i < num; i++)
						{
							vector[i] = EditorGUILayout.Slider(new GUIContent(input.componentLabels[i]), vector[i], minimum2, maximum2, new GUILayoutOption[0]);
						}
						EditorGUI.indentLevel--;
						EditorGUILayout.EndVertical();
					}
					else
					{
						switch (num)
						{
						case 2:
							vector = EditorGUILayout.Vector2Field(input.name, vector, new GUILayoutOption[0]);
							break;
						case 3:
							vector = EditorGUILayout.Vector3Field(input.name, vector, new GUILayoutOption[0]);
							break;
						case 4:
							vector = EditorGUILayout.Vector4Field(input.name, vector, new GUILayoutOption[0]);
							break;
						}
					}
					if (EditorGUI.EndChangeCheck())
					{
						this.RecordForUndo(ProceduralMaterialInspector.m_Material, ProceduralMaterialInspector.m_Importer, "Modified property " + input.name + " for material " + ProceduralMaterialInspector.m_Material.name);
						ProceduralMaterialInspector.m_Material.SetProceduralVector(input.name, vector);
					}
					break;
				}
				case ProceduralPropertyType.Color3:
				case ProceduralPropertyType.Color4:
				{
					EditorGUI.BeginChangeCheck();
					Color value3 = EditorGUILayout.ColorField(gUIContent, ProceduralMaterialInspector.m_Material.GetProceduralColor(input.name), new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						this.RecordForUndo(ProceduralMaterialInspector.m_Material, ProceduralMaterialInspector.m_Importer, "Modified property " + input.name + " for material " + ProceduralMaterialInspector.m_Material.name);
						ProceduralMaterialInspector.m_Material.SetProceduralColor(input.name, value3);
					}
					break;
				}
				case ProceduralPropertyType.Enum:
				{
					GUIContent[] array = new GUIContent[input.enumOptions.Length];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = new GUIContent(input.enumOptions[j]);
					}
					EditorGUI.BeginChangeCheck();
					int value4 = EditorGUILayout.Popup(gUIContent, ProceduralMaterialInspector.m_Material.GetProceduralEnum(input.name), array, new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						this.RecordForUndo(ProceduralMaterialInspector.m_Material, ProceduralMaterialInspector.m_Importer, "Modified property " + input.name + " for material " + ProceduralMaterialInspector.m_Material.name);
						ProceduralMaterialInspector.m_Material.SetProceduralEnum(input.name, value4);
					}
					break;
				}
				case ProceduralPropertyType.Texture:
				{
					EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
					GUILayout.Label(gUIContent, new GUILayoutOption[0]);
					GUILayout.FlexibleSpace();
					Rect rect = GUILayoutUtility.GetRect(64f, 64f, new GUILayoutOption[]
					{
						GUILayout.ExpandWidth(false)
					});
					EditorGUI.BeginChangeCheck();
					Texture2D value5 = EditorGUI.DoObjectField(rect, rect, GUIUtility.GetControlID(12354, EditorGUIUtility.native, rect), ProceduralMaterialInspector.m_Material.GetProceduralTexture(input.name), typeof(Texture2D), null, null, false) as Texture2D;
					EditorGUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck())
					{
						this.RecordForUndo(ProceduralMaterialInspector.m_Material, ProceduralMaterialInspector.m_Importer, "Modified property " + input.name + " for material " + ProceduralMaterialInspector.m_Material.name);
						ProceduralMaterialInspector.m_Material.SetProceduralTexture(input.name, value5);
					}
					break;
				}
				}
				EditorGUI.indentLevel = indentLevel;
			}
		}
		private void InputHSLGUI(ProceduralPropertyDescription hInput, ProceduralPropertyDescription sInput, ProceduralPropertyDescription lInput)
		{
			GUILayout.Space(5f);
			this.m_ShowHSLInputs = EditorPrefs.GetBool("ProceduralShowHSL", true);
			EditorGUI.BeginChangeCheck();
			this.m_ShowHSLInputs = EditorGUILayout.Foldout(this.m_ShowHSLInputs, this.m_Styles.hslContent);
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetBool("ProceduralShowHSL", this.m_ShowHSLInputs);
			}
			if (this.m_ShowHSLInputs)
			{
				EditorGUI.indentLevel++;
				this.InputGUI(hInput);
				this.InputGUI(sInput);
				this.InputGUI(lInput);
				EditorGUI.indentLevel--;
			}
		}
		private void InputSeedGUI(ProceduralPropertyDescription input)
		{
			Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
			EditorGUI.BeginChangeCheck();
			float value = (float)this.RandomIntField(controlRect, this.m_Styles.randomSeedContent, (int)ProceduralMaterialInspector.m_Material.GetProceduralFloat(input.name), 0, 9999);
			if (EditorGUI.EndChangeCheck())
			{
				this.RecordForUndo(ProceduralMaterialInspector.m_Material, ProceduralMaterialInspector.m_Importer, "Modified random seed for material " + ProceduralMaterialInspector.m_Material.name);
				ProceduralMaterialInspector.m_Material.SetProceduralFloat(input.name, value);
			}
		}
		internal int RandomIntField(Rect position, GUIContent label, int val, int min, int max)
		{
			position = EditorGUI.PrefixLabel(position, 0, label);
			return this.RandomIntField(position, val, min, max);
		}
		internal int RandomIntField(Rect position, int val, int min, int max)
		{
			position.width = position.width - EditorGUIUtility.fieldWidth - 5f;
			if (GUI.Button(position, this.m_Styles.randomizeButtonContent, EditorStyles.miniButton))
			{
				val = UnityEngine.Random.Range(min, max + 1);
			}
			position.x += position.width + 5f;
			position.width = EditorGUIUtility.fieldWidth;
			val = Mathf.Clamp(EditorGUI.IntField(position, val), min, max);
			return val;
		}
	}
}
