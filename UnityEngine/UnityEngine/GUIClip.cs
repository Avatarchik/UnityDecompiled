using System;
using System.Runtime.CompilerServices;
namespace UnityEngine
{
	internal sealed class GUIClip
	{
		public static extern bool enabled
		{
			[WrapperlessIcall]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		public static extern Rect topmostRect
		{
			[WrapperlessIcall]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		public static extern Rect visibleRect
		{
			[WrapperlessIcall]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		internal static void Push(Rect screenRect, Vector2 scrollOffset, Vector2 renderOffset, bool resetOffset)
		{
			GUIClip.INTERNAL_CALL_Push(ref screenRect, ref scrollOffset, ref renderOffset, resetOffset);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_Push(ref Rect screenRect, ref Vector2 scrollOffset, ref Vector2 renderOffset, bool resetOffset);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void Pop();
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Rect GetTopRect();
		public static Vector2 Unclip(Vector2 pos)
		{
			GUIClip.Unclip_Vector2(ref pos);
			return pos;
		}
		private static void Unclip_Vector2(ref Vector2 pos)
		{
			GUIClip.INTERNAL_CALL_Unclip_Vector2(ref pos);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_Unclip_Vector2(ref Vector2 pos);
		public static Rect Unclip(Rect rect)
		{
			GUIClip.Unclip_Rect(ref rect);
			return rect;
		}
		private static void Unclip_Rect(ref Rect rect)
		{
			GUIClip.INTERNAL_CALL_Unclip_Rect(ref rect);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_Unclip_Rect(ref Rect rect);
		public static Vector2 Clip(Vector2 absolutePos)
		{
			GUIClip.Clip_Vector2(ref absolutePos);
			return absolutePos;
		}
		private static void Clip_Vector2(ref Vector2 absolutePos)
		{
			GUIClip.INTERNAL_CALL_Clip_Vector2(ref absolutePos);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_Clip_Vector2(ref Vector2 absolutePos);
		public static Rect Clip(Rect absoluteRect)
		{
			GUIClip.Internal_Clip_Rect(ref absoluteRect);
			return absoluteRect;
		}
		private static void Internal_Clip_Rect(ref Rect absoluteRect)
		{
			GUIClip.INTERNAL_CALL_Internal_Clip_Rect(ref absoluteRect);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_Internal_Clip_Rect(ref Rect absoluteRect);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void Reapply();
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Matrix4x4 GetMatrix();
		internal static void SetMatrix(Matrix4x4 m)
		{
			GUIClip.INTERNAL_CALL_SetMatrix(ref m);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_SetMatrix(ref Matrix4x4 m);
		public static Vector2 GetAbsoluteMousePosition()
		{
			Vector2 result;
			GUIClip.Internal_GetAbsoluteMousePosition(out result);
			return result;
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_GetAbsoluteMousePosition(out Vector2 output);
	}
}
