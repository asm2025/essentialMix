using System;
using System.Threading.Tasks;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace Test.Common.Model;

[Serializable]
public class Student
{
	public int Id { get; set; }
	public int External { get; set; }

	public string Name { get; set; }

	public double Grade { get; set; }

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return $"{Id:D5} {Name} [{Grade:F2}]"; }

	public event EventHandler Happened;

	public void MethodWithInputs(int input1, string input2)
	{
	}

	public void MethodWithArrayInputs(int[] input1, string[] input2)
	{
	}

	public void MethodWithInputs(int input1, int[] input2)
	{
	}

	public void WillHappenIn(int millisecondTimeout)
	{
		if (millisecondTimeout < 1) throw new ArgumentOutOfRangeException(nameof(millisecondTimeout));
		Task.Delay(millisecondTimeout).ConfigureAwait().ContinueWith(_ => Happened?.Invoke(this, EventArgs.Empty));
	}
}