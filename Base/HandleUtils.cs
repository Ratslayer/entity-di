#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BB
{
	public static class HandleUtils
	{
		public static Color Color
		{
			get => Handles.color;
			set => Handles.color = value;
		}
		public static void DrawText(string text, Vector3 pos)
		{
			Handles.Label(pos, text);
		}
		public static void DrawCube(Vector3 center, Vector3 size, Quaternion rotation)
		{
			var c = center;
			var r = rotation * (size.x * 0.5f * Vector3.right);
			var u = rotation * (size.y * 0.5f * Vector3.up);
			var f = rotation * (size.z * 0.5f * Vector3.forward);

			var blb = c - u - r - f;
			var blf = c - u - r + f;
			var brb = c - u + r - f;
			var brf = c - u + r + f;
			var ulb = c + u - r - f;
			var ulf = c + u - r + f;
			var urb = c + u + r - f;
			var urf = c + u + r + f;
			DrawShape(blb, blf, brf, brb);
			DrawShape(ulb, ulf, urf, urb);
			DrawLine(blb, ulb);
			DrawLine(blf, ulf);
			DrawLine(brf, urf);
			DrawLine(brb, urb);
		}
		public static bool DrawEditableCube(
			Vector3 center,
			Vector3 size,
			Quaternion rotation,
			bool moveBottom,
			out Vector3 newCenter,
			out Vector3 newSize)
		{
			var result = false;
			newCenter = default;
			newSize = default;
			var quadSize = new Vector2(size.x, size.z);
			//draw bottom
			if (DrawEditableQuad(center, quadSize,
				rotation, true, true, false, out var c, out var s))
			{
				result = true;
				newCenter = c.SetY(center.y);
				newSize = new(s.x, size.y, s.y);
			}
			//draw top
			var topCenter = center + Vector3.up * size.y;
			if (DrawEditableQuad(topCenter, quadSize,
				rotation, true, true, true, out c, out s))
			{
				result = true;
				newCenter = c.SetY(center.y);
				newSize = new(s.x, size.y + c.y - topCenter.y, s.y);
			}
			//draw vertical segments
			var up = rotation * Vector3.up;
			DrawCube(center + size.y * 0.5f * up, size, rotation);
			return result;
		}
		public static bool DrawEditableQuad(
			Vector3 center,
			Vector2 size,
			Quaternion rotation,
			bool drawCornerHandles,
			bool drawEdgeHandles,
			bool allow3dMove,
			out Vector3 newCenter,
			out Vector2 newSize)
		{
			newCenter = default;
			newSize = default;
			var changed = false;
			var right = rotation * Vector3.right;
			var up = rotation * Vector3.up;
			var forward = rotation * Vector3.forward;
			var x = size.x * 0.5f * right;
			var z = size.y * 0.5f * forward;
			var c1 = center + x + z;
			var c2 = center + x - z;
			var c3 = center - x - z;
			var c4 = center - x + z;

			if (drawCornerHandles)
			{
				CornerMove(c1, new(1, 1), ref newCenter, ref newSize);
				CornerMove(c2, new(1, -1), ref newCenter, ref newSize);
				CornerMove(c3, new(-1, -1), ref newCenter, ref newSize);
				CornerMove(c4, new(-1, 1), ref newCenter, ref newSize);
			}
			DrawShape(c1, c2, c3, c4);
			return changed;
			void CornerMove(Vector3 point, Vector2 growDir, ref Vector3 c, ref Vector2 s)
			{
				if (!MoveDot(point, out var newPoint))
					return;
				var diff = (newPoint - point);
				if (!allow3dMove)
					diff -= up * Vector3.Dot(diff, up);
				c = center + diff * 0.5f;
				s = size
					+ new Vector2(Vector3.Dot(diff, right), Vector3.Dot(diff, forward))
					* growDir;
				changed = true;
			}
		}
		public static void DrawSegments(params Vector3[] segments)
		{
			Handles.DrawLines(segments);
		}
		public static void DrawLine(Vector3 p1, Vector3 p2, float thickness = 0)
			=> Handles.DrawLine(p1, p2, thickness);
		public static void DrawText(Vector3 position, string txt)
		{
			Handles.Label(position, txt);
		}
		public static void DrawLinesContinuous(params Vector3[] points)
		{
			Handles.DrawPolyLine(points);
		}
		public static bool MoveDot(Vector3 pos, Transform space, Action<Vector3> onMove)
		{
			if (!MoveDot(pos, out var newPos))
				return false;
			var diff = newPos - pos;
			var localDiff = space.InverseTransformDirection(diff);
			onMove(localDiff);
			return true;
		}
		public static void DrawShape(params Vector3[] vertices)
		{
			if (vertices.Length < 2)
				return;
			if (vertices.Length == 2)
			{
				DrawSegments(vertices);
				return;
			}
			var indices = new int[vertices.Length * 2];
			for (var i = 0; i < vertices.Length - 1; i++)
			{
				indices[i * 2] = i;
				indices[i * 2 + 1] = i + 1;
			}
			indices[^2] = vertices.Length - 1;
			indices[^1] = 0;
			Handles.DrawLines(vertices, indices);
		}
		public static bool DotButton(Vector3 point, float dotSize = 0.1f)
			=> Handles.Button(point, Quaternion.identity, dotSize, dotSize, Handles.DotHandleCap);
		public static Vector3 MoveDot(Vector3 point, float dotSize = 0.1f)
		{
			return Handles.FreeMoveHandle(point, dotSize, new(), Handles.DotHandleCap);
		}
		public static bool MoveDot(Vector3 point, out Vector3 newPoint, float dotSize = 0.1f)
		{
			newPoint = MoveDot(point, dotSize);
			return !newPoint.Approximately(point);
		}
		public static bool MoveHandle(Vector3 point, out Vector3 newPoint)
		{
			newPoint = Handles.PositionHandle(point, Quaternion.identity);
			return !newPoint.Approximately(point);
		}
		public static void DrawCircle(
			Vector3 pos,
			float radius,
			float thickness = 1f)
			=> Handles.DrawWireArc(
				pos, Vector3.up, Vector3.forward, 360, radius, thickness);
		public static void DrawCapsule(
			Vector3 pos, 
			float height,
			float radius, 
			float skinWidth,
			Vector3 normal)
		{
			var args = CapsuleUtils.GetVerticalCapsuleArgs(pos, height, radius, skinWidth);
			DrawCapsule(args.P1, args.P2, normal, args.Radius);
		}
		public static void DrawCapsule(
			Vector3 p1,
			Vector3 p2,
			float radius,
			float thickness = 0f)
		{
			var dir = p2 - p1;
			var tangent = Vector3.Cross(
				SceneView.lastActiveSceneView.camera.transform.forward,
				dir);
			var normal = Vector3.Cross(dir, tangent);
			DrawCapsule(p1, p2, normal, radius, thickness);
		}
		public static void DrawCapsule(
			Vector3 p1,
			Vector3 p2,
			Vector3 normal,
			float radius,
			float thickness = 0f)
		{
			var dir = p2 - p1;
			var right = Vector3.Cross(normal, dir).normalized;
			SceneGUIUtils.DrawWireArc(p1, right, normal, 180, radius, thickness);
			SceneGUIUtils.DrawWireArc(p2, right, normal, -180, radius, thickness);
			var offset = right * radius;
			Handles.DrawLine(p1 + offset, p2 + offset, thickness);
			Handles.DrawLine(p1 - offset, p2 - offset, thickness);
		}
	}
}
#endif