using UnityEngine;
using System.Collections;

public abstract class NullBool
{
	public static implicit operator bool (NullBool n)
	{
		return n != null;
	}
}

