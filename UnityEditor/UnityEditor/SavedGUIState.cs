using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace UnityEditor
{
	internal struct SavedGUIState
	{
		internal GUILayoutUtility.LayoutCache layoutCache;
		internal IntPtr guiState;
		internal Vector2 screenManagerSize;
		internal Rect renderManagerRect;
		internal GUISkin skin;
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_SetupSavedGUIState(out IntPtr state, out Vector2 screenManagerSize, out Rect renderManagerRect);
		private static void Internal_ApplySavedGUIState(IntPtr state, Vector2 screenManagerSize, Rect renderManagerRect)
		{
			SavedGUIState.INTERNAL_CALL_Internal_ApplySavedGUIState(state, ref screenManagerSize, ref renderManagerRect);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_Internal_ApplySavedGUIState(IntPtr state, ref Vector2 screenManagerSize, ref Rect renderManagerRect);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int Internal_GetGUIDepth();
		internal static SavedGUIState Create()
		{
			SavedGUIState result = default(SavedGUIState);
			if (SavedGUIState.Internal_GetGUIDepth() > 0)
			{
				result.skin = GUI.skin;
				result.layoutCache = new GUILayoutUtility.LayoutCache(GUILayoutUtility.current);
				SavedGUIState.Internal_SetupSavedGUIState(out result.guiState, out result.screenManagerSize, out result.renderManagerRect);
			}
			return result;
		}
		internal void ApplyAndForget()
		{
			if (this.layoutCache != null)
			{
				GUILayoutUtility.current = this.layoutCache;
				GUI.skin = this.skin;
				SavedGUIState.Internal_ApplySavedGUIState(this.guiState, this.screenManagerSize, this.renderManagerRect);
				GUIClip.Reapply();
			}
		}
	}
}
