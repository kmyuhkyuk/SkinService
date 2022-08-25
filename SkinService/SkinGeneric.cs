using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal sealed class SkinGeneric<T, U>
{
	public T Action
	{
		get
		{
			return this.gparam_0;
		}
	}

	public U skinids
	{
		get
		{
			return this.gparam_1;
		}
	}

	public SkinGeneric(T Action, U skinids)
	{
		this.gparam_0 = Action;
		this.gparam_1 = skinids;
	}

	public override bool Equals(object value)
	{
		SkinGeneric<T, U> @class = value as SkinGeneric<T, U>;
		return @class != null && EqualityComparer<T>.Default.Equals(this.gparam_0, @class.gparam_0) && EqualityComparer<U>.Default.Equals(this.gparam_1, @class.gparam_1);
	}

	public override int GetHashCode()
	{
		return (332215222 + EqualityComparer<T>.Default.GetHashCode(this.gparam_0)) * -1521134296 + EqualityComparer<U>.Default.GetHashCode(this.gparam_1);
	}

	public override string ToString()
	{
		IFormatProvider provider = null;
		string format = "{{ Action = {0}, skinids = {1} }}";
		object[] array = new object[2];
		int num = 0;
		T t = this.gparam_0;
		ref T ptr = ref t;
		T t2 = default(T);
		object obj;
		if (t2 == null)
		{
			t2 = t;
			ptr = ref t2;
			if (t2 == null)
			{
				obj = null;
				goto IL_46;
			}
		}
		obj = ptr.ToString();
	IL_46:
		array[num] = obj;
		int num2 = 1;
		U u = this.gparam_1;
		ref U ptr2 = ref u;
		U u2 = default(U);
		object obj2;
		if (u2 == null)
		{
			u2 = u;
			ptr2 = ref u2;
			if (u2 == null)
			{
				obj2 = null;
				goto IL_81;
			}
		}
		obj2 = ptr2.ToString();
	IL_81:
		array[num2] = obj2;
		return string.Format(provider, format, array);
	}

	private readonly T gparam_0;

	private readonly U gparam_1;
}
