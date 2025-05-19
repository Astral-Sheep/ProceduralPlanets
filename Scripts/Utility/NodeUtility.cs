using Godot;

namespace Com.Astral.Stars;

public static class NodeUtility
{
	public static Vector3 GetUpVector(this Node3D pNode)
	{
		return pNode.GlobalTransform.Basis.Y.Normalized();
	}

	public static Vector3 GetForwardVector(this Node3D pNode)
	{
		return -pNode.GlobalTransform.Basis.Z.Normalized();
	}

	public static Vector3 GetRightVector(this Node3D pNode)
	{
		return pNode.GlobalTransform.Basis.X.Normalized();
	}
}