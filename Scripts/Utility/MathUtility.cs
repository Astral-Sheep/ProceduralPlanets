using System;
using Godot;

namespace Com.Astral.Stars;

public static class MathUtility
{
	// Value of the golden ratio
	public const float PHI = 1.6180339887f;
	public const float INV_PHI = 0.6180339887f;

	public static T Min<T>(T pLhs, T pRhs) where T: IComparable<T>
	{
		return pLhs.CompareTo(pRhs) < 0 ? pLhs : pRhs;
	}

	public static T Max<T>(T pLhs, T pRhs) where T: IComparable<T>
	{
		return pLhs.CompareTo(pRhs) < 0 ? pRhs : pLhs;
	}

	public static Vector2 CartesianToPolar(Vector2 pVector)
	{
		return new Vector2(
			pVector.Length(),
			pVector.Angle()
		);
	}

	public static Vector2 PolarToCartesian(float pRadius, float pAngle)
	{
		return pRadius * new Vector2(
			Mathf.Cos(pAngle),
			Mathf.Sin(pAngle)
		);
	}

	public static Vector3 CartesianToSpheric(this Vector3 pVector)
	{
		float lLength = pVector.Length();
		return new Vector3(
			lLength,
			Mathf.Atan2(pVector.Z, pVector.X),
			Mathf.Asin(pVector.Y / lLength)
		);
	}

	public static Vector3 SphericToCartesian(this Vector3 pVector)
	{
		return new Vector3(
			Mathf.Cos(pVector.Y) * Mathf.Cos(pVector.Z),
			Mathf.Sin(pVector.Z),
			Mathf.Sin(pVector.Y) * Mathf.Cos(pVector.Z)
		) * pVector.X;
	}

	public static Vector3 SphericToCartesian(float pLength, float pHorizontalAngle, float pVerticalAngle)
	{
		return new Vector3(
			Mathf.Cos(pHorizontalAngle) * Mathf.Cos(pVerticalAngle),
			Mathf.Sin(pVerticalAngle),
			Mathf.Sin(pHorizontalAngle) * Mathf.Cos(pVerticalAngle)
		) * pLength;
	}

	public static Transform3D NoScale(this Transform3D pTransform)
	{
		Basis lBasis = pTransform.Basis.Scaled(pTransform.Basis.Scale.Inverse());
		pTransform.Basis = lBasis;
		return pTransform;
	}

	public static Vector3 GetForward(this Transform3D pTransform)
	{
		return -pTransform.Basis.Z;
	}

	public static Vector3 GetRight(this Transform3D pTransform)
	{
		return pTransform.Basis.X;
	}

	public static Vector3 GetUp(this Transform3D pTransform)
	{
		return pTransform.Basis.Y;
	}
}