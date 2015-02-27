using System;
using UnityEngine;
namespace UnityEditor
{
	internal class ModelImporterRigEditor : AssetImporterInspector
	{
		private class Styles
		{
			public GUIContent AnimationType = EditorGUIUtility.TextContent("ModelImporterAnimationType");
			public GUIContent[] AnimationTypeOpt = new GUIContent[]
			{
				EditorGUIUtility.TextContent("ModelImporterAnimationTypeNone"),
				EditorGUIUtility.TextContent("ModelImporterAnimationTypeLegacy"),
				EditorGUIUtility.TextContent("ModelImporterAnimationTypeGeneric"),
				EditorGUIUtility.TextContent("ModelImporterAnimationTypeHumanoid")
			};
			public GUIContent AnimLabel = EditorGUIUtility.TextContent("ModelImporterAnimLabel");
			public GUIContent[] AnimationsOpt = new GUIContent[]
			{
				EditorGUIUtility.TextContent("ModelImporterAnimNone"),
				EditorGUIUtility.TextContent("ModelImporterAnimOrigRoot"),
				EditorGUIUtility.TextContent("ModelImporterAnimNode"),
				EditorGUIUtility.TextContent("ModelImporterAnimRoot"),
				EditorGUIUtility.TextContent("ModelImporterAnimRootNew")
			};
			public GUIStyle helpText = new GUIStyle(EditorStyles.helpBox);
			public GUIContent avatar = new GUIContent("Animator");
			public GUIContent configureAvatar = EditorGUIUtility.TextContent("Configure...");
			public GUIContent avatarValid = EditorGUIUtility.TextContent("✓");
			public GUIContent avatarInvalid = EditorGUIUtility.TextContent("✕");
			public GUIContent avatarPending = EditorGUIUtility.TextContent("...");
			public GUIContent UpdateMuscleDefinitionFromSource = EditorGUIUtility.TextContent("ModelImporterRigUpdateMuscleDefinitionFromSource");
			public GUIContent RootNode = EditorGUIUtility.TextContent("ModelImporterRigRootNode");
			public GUIContent AvatarDefinition = EditorGUIUtility.TextContent("ModelImporterRigAvatarDefinition");
			public GUIContent[] AvatarDefinitionOpt = new GUIContent[]
			{
				EditorGUIUtility.TextContent("ModelImporterRigCreateFromThisModel"),
				EditorGUIUtility.TextContent("ModelImporterRigCopyFromOtherAvatar")
			};
			public GUIContent UpdateReferenceClips = EditorGUIUtility.TextContent("ModelImporterRigUpdateReferenceClips");
			public Styles()
			{
				this.helpText.normal.background = null;
				this.helpText.alignment = TextAnchor.MiddleLeft;
				this.helpText.padding = new RectOffset(0, 0, 0, 0);
			}
		}
		private struct MappingRelevantSettings
		{
			public bool humanoid;
			public bool copyAvatar;
			public bool hasNoAnimation;
			public bool usesOwnAvatar
			{
				get
				{
					return this.humanoid && !this.copyAvatar;
				}
			}
		}
		private const float kDeleteWidth = 17f;
		public int m_SelectedClipIndex = -1;
		private Avatar m_Avatar;
		private SerializedProperty m_OptimizeGameObjects;
		private SerializedProperty m_AnimationType;
		private SerializedProperty m_AvatarSource;
		private SerializedProperty m_CopyAvatar;
		private SerializedProperty m_LegacyGenerateAnimations;
		private SerializedProperty m_AnimationCompression;
		private SerializedProperty m_RootMotionBoneName;
		private GUIContent[] m_RootMotionBoneList;
		private ExposeTransformEditor m_ExposeTransformEditor;
		private bool m_AvatarCopyIsUpToDate;
		private bool m_CanMultiEditTransformList;
		private static ModelImporterRigEditor.Styles styles;
		private ModelImporter singleImporter
		{
			get
			{
				return base.targets[0] as ModelImporter;
			}
		}
		private ModelImporterAnimationType animationType
		{
			get
			{
				return (ModelImporterAnimationType)this.m_AnimationType.intValue;
			}
			set
			{
				this.m_AnimationType.intValue = (int)value;
			}
		}
		public int rootIndex
		{
			get;
			set;
		}
		public bool isLocked
		{
			get
			{
				InspectorWindow[] allInspectorWindows = InspectorWindow.GetAllInspectorWindows();
				for (int i = 0; i < allInspectorWindows.Length; i++)
				{
					InspectorWindow inspectorWindow = allInspectorWindows[i];
					ActiveEditorTracker tracker = inspectorWindow.GetTracker();
					Editor[] activeEditors = tracker.activeEditors;
					for (int j = 0; j < activeEditors.Length; j++)
					{
						Editor x = activeEditors[j];
						if (x == this)
						{
							return inspectorWindow.isLocked;
						}
					}
				}
				return false;
			}
		}
		public void OnEnable()
		{
			this.m_AnimationType = base.serializedObject.FindProperty("m_AnimationType");
			this.m_AvatarSource = base.serializedObject.FindProperty("m_LastHumanDescriptionAvatarSource");
			this.m_OptimizeGameObjects = base.serializedObject.FindProperty("m_OptimizeGameObjects");
			this.m_RootMotionBoneName = base.serializedObject.FindProperty("m_HumanDescription.m_RootMotionBoneName");
			this.m_ExposeTransformEditor = new ExposeTransformEditor();
			string[] transformPaths = this.singleImporter.transformPaths;
			this.m_RootMotionBoneList = new GUIContent[transformPaths.Length];
			for (int i = 0; i < transformPaths.Length; i++)
			{
				this.m_RootMotionBoneList[i] = new GUIContent(transformPaths[i]);
			}
			if (this.m_RootMotionBoneList.Length > 0)
			{
				this.m_RootMotionBoneList[0] = new GUIContent("None");
			}
			this.rootIndex = ArrayUtility.FindIndex<GUIContent>(this.m_RootMotionBoneList, (GUIContent content) => FileUtil.GetLastPathNameComponent(content.text) == this.m_RootMotionBoneName.stringValue);
			this.rootIndex = ((this.rootIndex >= 1) ? this.rootIndex : 0);
			this.m_CopyAvatar = base.serializedObject.FindProperty("m_CopyAvatar");
			this.m_LegacyGenerateAnimations = base.serializedObject.FindProperty("m_LegacyGenerateAnimations");
			this.m_AnimationCompression = base.serializedObject.FindProperty("m_AnimationCompression");
			this.m_ExposeTransformEditor.OnEnable(this.singleImporter.transformPaths, base.serializedObject);
			this.m_CanMultiEditTransformList = this.CanMultiEditTransformList();
			this.CheckIfAvatarCopyIsUpToDate();
		}
		private bool CanMultiEditTransformList()
		{
			string[] transformPaths = this.singleImporter.transformPaths;
			for (int i = 1; i < base.targets.Length; i++)
			{
				ModelImporter modelImporter = base.targets[i] as ModelImporter;
				if (!ArrayUtility.ArrayEquals<string>(transformPaths, modelImporter.transformPaths))
				{
					return false;
				}
			}
			return true;
		}
		private void CheckIfAvatarCopyIsUpToDate()
		{
			if ((this.animationType != ModelImporterAnimationType.Human && this.animationType != ModelImporterAnimationType.Generic) || this.m_AvatarSource.objectReferenceValue == null)
			{
				this.m_AvatarCopyIsUpToDate = true;
				return;
			}
			string assetPath = AssetDatabase.GetAssetPath(this.m_AvatarSource.objectReferenceValue);
			ModelImporter otherImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
			this.m_AvatarCopyIsUpToDate = ModelImporterRigEditor.DoesHumanDescriptionMatch(this.singleImporter, otherImporter);
		}
		internal override void ResetValues()
		{
			base.ResetValues();
			this.m_Avatar = (AssetDatabase.LoadAssetAtPath((this.target as ModelImporter).assetPath, typeof(Avatar)) as Avatar);
		}
		private void LegacyGUI()
		{
			EditorGUILayout.Popup(this.m_LegacyGenerateAnimations, ModelImporterRigEditor.styles.AnimationsOpt, ModelImporterRigEditor.styles.AnimLabel, new GUILayoutOption[0]);
			if (this.m_LegacyGenerateAnimations.intValue == 1 || this.m_LegacyGenerateAnimations.intValue == 2 || this.m_LegacyGenerateAnimations.intValue == 3)
			{
				EditorGUILayout.HelpBox("The animation import setting \"" + ModelImporterRigEditor.styles.AnimationsOpt[this.m_LegacyGenerateAnimations.intValue].text + "\" is deprecated.", MessageType.Warning);
			}
		}
		private void AvatarSourceGUI()
		{
			EditorGUI.BeginChangeCheck();
			int num = (!this.m_CopyAvatar.boolValue) ? 0 : 1;
			EditorGUI.showMixedValue = this.m_CopyAvatar.hasMultipleDifferentValues;
			num = EditorGUILayout.Popup(ModelImporterRigEditor.styles.AvatarDefinition, num, ModelImporterRigEditor.styles.AvatarDefinitionOpt, new GUILayoutOption[0]);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				this.m_CopyAvatar.boolValue = (num == 1);
			}
		}
		private void GenericGUI()
		{
			this.AvatarSourceGUI();
			if (!this.m_CopyAvatar.hasMultipleDifferentValues)
			{
				if (!this.m_CopyAvatar.boolValue)
				{
					EditorGUI.BeginDisabledGroup(!this.m_CanMultiEditTransformList);
					EditorGUI.BeginChangeCheck();
					this.rootIndex = EditorGUILayout.Popup(ModelImporterRigEditor.styles.RootNode, this.rootIndex, this.m_RootMotionBoneList, new GUILayoutOption[0]);
					EditorGUI.EndDisabledGroup();
					if (EditorGUI.EndChangeCheck())
					{
						if (this.rootIndex > 0 && this.rootIndex < this.m_RootMotionBoneList.Length)
						{
							this.m_RootMotionBoneName.stringValue = FileUtil.GetLastPathNameComponent(this.m_RootMotionBoneList[this.rootIndex].text);
						}
						else
						{
							this.m_RootMotionBoneName.stringValue = string.Empty;
						}
					}
				}
				else
				{
					this.CopyAvatarGUI();
				}
			}
		}
		private void HumanoidGUI()
		{
			this.AvatarSourceGUI();
			if (!this.m_CopyAvatar.hasMultipleDifferentValues)
			{
				if (!this.m_CopyAvatar.boolValue)
				{
					this.ConfigureAvatarGUI();
				}
				else
				{
					this.CopyAvatarGUI();
				}
			}
			EditorGUILayout.Space();
		}
		private void ConfigureAvatarGUI()
		{
			if (base.targets.Length > 1)
			{
				GUILayout.Label("Can't configure avatar in multi-editing mode", EditorStyles.helpBox, new GUILayoutOption[0]);
				return;
			}
			if (this.singleImporter.transformPaths.Length <= HumanTrait.RequiredBoneCount)
			{
				GUILayout.Label("Not enough bones to create human avatar (requires " + HumanTrait.RequiredBoneCount + ")", EditorStyles.helpBox, new GUILayoutOption[0]);
				return;
			}
			GUIContent content;
			if (this.m_Avatar && !this.HasModified())
			{
				if (this.m_Avatar.isHuman)
				{
					content = ModelImporterRigEditor.styles.avatarValid;
				}
				else
				{
					content = ModelImporterRigEditor.styles.avatarInvalid;
				}
			}
			else
			{
				content = ModelImporterRigEditor.styles.avatarPending;
				GUILayout.Label("The avatar can be configured after settings have been applied.", EditorStyles.helpBox, new GUILayoutOption[0]);
			}
			Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
			GUI.Label(new Rect(controlRect.xMax - 75f - 18f, controlRect.y, 18f, controlRect.height), content, EditorStyles.label);
			EditorGUI.BeginDisabledGroup(this.m_Avatar == null);
			if (GUI.Button(new Rect(controlRect.xMax - 75f, controlRect.y + 1f, 75f, controlRect.height - 1f), ModelImporterRigEditor.styles.configureAvatar, EditorStyles.miniButton))
			{
				if (!this.isLocked)
				{
					if (EditorApplication.SaveCurrentSceneIfUserWantsTo())
					{
						Selection.activeObject = this.m_Avatar;
						AvatarEditor.s_EditImmediatelyOnNextOpen = true;
					}
				}
				else
				{
					Debug.Log("Cannot configure avatar, inspector is locked");
				}
			}
			EditorGUI.EndDisabledGroup();
		}
		private void CheckAvatar(Avatar sourceAvatar)
		{
			if (sourceAvatar != null)
			{
				if (sourceAvatar.isHuman && this.animationType != ModelImporterAnimationType.Human)
				{
					if (EditorUtility.DisplayDialog("Asigning an Humanoid Avatar on a Generic Rig", "Do you want to change Animation Type to Humanoid ?", "Yes", "No"))
					{
						this.animationType = ModelImporterAnimationType.Human;
					}
					else
					{
						this.m_AvatarSource.objectReferenceValue = null;
					}
				}
				else
				{
					if (!sourceAvatar.isHuman && this.animationType != ModelImporterAnimationType.Generic)
					{
						if (EditorUtility.DisplayDialog("Asigning an Generic Avatar on a Humanoid Rig", "Do you want to change Animation Type to Generic ?", "Yes", "No"))
						{
							this.animationType = ModelImporterAnimationType.Generic;
						}
						else
						{
							this.m_AvatarSource.objectReferenceValue = null;
						}
					}
				}
			}
		}
		private void CopyAvatarGUI()
		{
			GUILayout.Label("If you have already created an Avatar for another model with a rig identical to this one, you can copy its Avatar definition.\nWith this option, this model will not create any avatar but only import animations.", EditorStyles.helpBox, new GUILayoutOption[0]);
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(this.m_AvatarSource, GUIContent.Temp("Source"), new GUILayoutOption[0]);
			Avatar avatar = this.m_AvatarSource.objectReferenceValue as Avatar;
			if (EditorGUI.EndChangeCheck())
			{
				this.CheckAvatar(avatar);
				AvatarSetupTool.ClearAll(base.serializedObject);
				if (avatar != null)
				{
					this.CopyHumanDescriptionFromOtherModel(avatar);
				}
				this.m_AvatarCopyIsUpToDate = true;
			}
			if (avatar != null && !this.m_AvatarSource.hasMultipleDifferentValues && !this.m_AvatarCopyIsUpToDate && GUILayout.Button(ModelImporterRigEditor.styles.UpdateMuscleDefinitionFromSource, EditorStyles.miniButton, new GUILayoutOption[0]))
			{
				AvatarSetupTool.ClearAll(base.serializedObject);
				this.CopyHumanDescriptionFromOtherModel(avatar);
				this.m_AvatarCopyIsUpToDate = true;
			}
			EditorGUILayout.EndHorizontal();
		}
		private void ShowUpdateReferenceClip()
		{
			if (base.targets.Length > 1 || this.animationType != ModelImporterAnimationType.Human || this.m_CopyAvatar.boolValue)
			{
				return;
			}
			string[] array = new string[0];
			ModelImporter modelImporter = this.target as ModelImporter;
			if (modelImporter.referencedClips.Length > 0)
			{
				string[] referencedClips = modelImporter.referencedClips;
				for (int i = 0; i < referencedClips.Length; i++)
				{
					string guid = referencedClips[i];
					ArrayUtility.Add<string>(ref array, AssetDatabase.GUIDToAssetPath(guid));
				}
			}
			if (array.Length > 0 && GUILayout.Button(ModelImporterRigEditor.styles.UpdateReferenceClips, new GUILayoutOption[]
			{
				GUILayout.Width(150f)
			}))
			{
				string[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					string otherModelImporterPath = array2[j];
					this.SetupReferencedClip(otherModelImporterPath);
				}
				try
				{
					AssetDatabase.StartAssetEditing();
					string[] array3 = array;
					for (int k = 0; k < array3.Length; k++)
					{
						string path = array3[k];
						AssetDatabase.ImportAsset(path);
					}
				}
				finally
				{
					AssetDatabase.StopAssetEditing();
				}
			}
		}
		public override void OnInspectorGUI()
		{
			if (ModelImporterRigEditor.styles == null)
			{
				ModelImporterRigEditor.styles = new ModelImporterRigEditor.Styles();
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Popup(this.m_AnimationType, ModelImporterRigEditor.styles.AnimationTypeOpt, ModelImporterRigEditor.styles.AnimationType, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.m_AvatarSource.objectReferenceValue = null;
				if (this.animationType == ModelImporterAnimationType.Legacy)
				{
					this.m_AnimationCompression.intValue = 1;
				}
				else
				{
					if (this.animationType == ModelImporterAnimationType.Generic || this.animationType == ModelImporterAnimationType.Human)
					{
						this.m_AnimationCompression.intValue = 3;
					}
				}
			}
			EditorGUILayout.Space();
			if (!this.m_AnimationType.hasMultipleDifferentValues)
			{
				if (this.animationType == ModelImporterAnimationType.Human)
				{
					this.HumanoidGUI();
				}
				else
				{
					if (this.animationType == ModelImporterAnimationType.Generic)
					{
						this.GenericGUI();
					}
					else
					{
						if (this.animationType == ModelImporterAnimationType.Legacy)
						{
							this.LegacyGUI();
						}
					}
				}
			}
			if (this.m_Avatar && this.m_Avatar.isValid && this.m_Avatar.isHuman)
			{
				this.ShowUpdateReferenceClip();
			}
			bool flag = true;
			if (this.animationType != ModelImporterAnimationType.Human && this.animationType != ModelImporterAnimationType.Generic)
			{
				flag = false;
			}
			if (this.m_CopyAvatar.boolValue)
			{
				flag = false;
			}
			if (flag)
			{
				EditorGUILayout.PropertyField(this.m_OptimizeGameObjects, new GUILayoutOption[0]);
				if (this.m_OptimizeGameObjects.boolValue && base.serializedObject.targetObjects.Length == 1)
				{
					EditorGUILayout.Space();
					EditorGUI.BeginDisabledGroup(!this.m_CanMultiEditTransformList);
					this.m_ExposeTransformEditor.OnGUI();
					EditorGUI.EndDisabledGroup();
				}
			}
			base.ApplyRevertGUI();
		}
		private static SerializedObject GetModelImporterSerializedObject(string assetPath)
		{
			ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
			if (modelImporter == null)
			{
				return null;
			}
			return new SerializedObject(modelImporter);
		}
		private static bool DoesHumanDescriptionMatch(ModelImporter importer, ModelImporter otherImporter)
		{
			SerializedObject serializedObject = new SerializedObject(new UnityEngine.Object[]
			{
				importer,
				otherImporter
			});
			SerializedProperty serializedProperty = serializedObject.FindProperty("m_HumanDescription");
			bool result = !serializedProperty.hasMultipleDifferentValues;
			serializedObject.Dispose();
			return result;
		}
		private static void CopyHumanDescriptionToDestination(SerializedObject sourceObject, SerializedObject targetObject)
		{
			targetObject.CopyFromSerializedProperty(sourceObject.FindProperty("m_HumanDescription"));
		}
		private void CopyHumanDescriptionFromOtherModel(Avatar sourceAvatar)
		{
			string assetPath = AssetDatabase.GetAssetPath(sourceAvatar);
			SerializedObject modelImporterSerializedObject = ModelImporterRigEditor.GetModelImporterSerializedObject(assetPath);
			ModelImporterRigEditor.CopyHumanDescriptionToDestination(modelImporterSerializedObject, base.serializedObject);
			modelImporterSerializedObject.Dispose();
		}
		private void SetupReferencedClip(string otherModelImporterPath)
		{
			SerializedObject modelImporterSerializedObject = ModelImporterRigEditor.GetModelImporterSerializedObject(otherModelImporterPath);
			if (modelImporterSerializedObject != null)
			{
				modelImporterSerializedObject.CopyFromSerializedProperty(base.serializedObject.FindProperty("m_AnimationType"));
				SerializedProperty serializedProperty = modelImporterSerializedObject.FindProperty("m_CopyAvatar");
				if (serializedProperty != null)
				{
					serializedProperty.boolValue = true;
				}
				SerializedProperty serializedProperty2 = modelImporterSerializedObject.FindProperty("m_LastHumanDescriptionAvatarSource");
				if (serializedProperty2 != null)
				{
					serializedProperty2.objectReferenceValue = this.m_Avatar;
				}
				ModelImporterRigEditor.CopyHumanDescriptionToDestination(base.serializedObject, modelImporterSerializedObject);
				modelImporterSerializedObject.ApplyModifiedProperties();
				modelImporterSerializedObject.Dispose();
			}
		}
		internal override void Apply()
		{
			ModelImporterRigEditor.MappingRelevantSettings[] array = new ModelImporterRigEditor.MappingRelevantSettings[base.targets.Length];
			for (int i = 0; i < base.targets.Length; i++)
			{
				SerializedObject serializedObject = new SerializedObject(base.targets[i]);
				SerializedProperty serializedProperty = serializedObject.FindProperty("m_AnimationType");
				SerializedProperty serializedProperty2 = serializedObject.FindProperty("m_CopyAvatar");
				array[i].humanoid = (serializedProperty.intValue == 3);
				array[i].hasNoAnimation = (serializedProperty.intValue == 0);
				array[i].copyAvatar = serializedProperty2.boolValue;
			}
			ModelImporterRigEditor.MappingRelevantSettings[] array2 = new ModelImporterRigEditor.MappingRelevantSettings[base.targets.Length];
			Array.Copy(array, array2, base.targets.Length);
			for (int j = 0; j < base.targets.Length; j++)
			{
				if (!this.m_AnimationType.hasMultipleDifferentValues)
				{
					array2[j].humanoid = (this.m_AnimationType.intValue == 3);
				}
				if (!this.m_CopyAvatar.hasMultipleDifferentValues)
				{
					array2[j].copyAvatar = this.m_CopyAvatar.boolValue;
				}
			}
			base.serializedObject.ApplyModifiedProperties();
			for (int k = 0; k < base.targets.Length; k++)
			{
				if (array[k].usesOwnAvatar && !array2[k].usesOwnAvatar)
				{
					SerializedObject serializedObject2 = new SerializedObject(base.targets[k]);
					AvatarSetupTool.ClearAll(serializedObject2);
					serializedObject2.ApplyModifiedProperties();
				}
				if (!array[k].usesOwnAvatar && array2[k].usesOwnAvatar)
				{
					ModelImporter modelImporter = base.targets[k] as ModelImporter;
					if (array[k].hasNoAnimation)
					{
						AssetDatabase.ImportAsset(modelImporter.assetPath);
					}
					SerializedObject serializedObject3 = new SerializedObject(base.targets[k]);
					GameObject gameObject = AssetDatabase.LoadMainAssetAtPath(modelImporter.assetPath) as GameObject;
					Animator component = gameObject.GetComponent<Animator>();
					bool flag = component && !component.hasTransformHierarchy;
					if (flag)
					{
						gameObject = (UnityEngine.Object.Instantiate(gameObject) as GameObject);
						AnimatorUtility.DeoptimizeTransformHierarchy(gameObject);
					}
					AvatarSetupTool.AutoSetupOnInstance(gameObject, serializedObject3);
					if (flag)
					{
						UnityEngine.Object.DestroyImmediate(gameObject);
					}
					serializedObject3.ApplyModifiedProperties();
				}
			}
		}
	}
}
