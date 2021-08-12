namespace TestApp
{
	internal class Student
	{
		public int Id { get; set; }
		public int External { get; set; }

		public string Name { get; set; }

		public double Grade { get; set; }

		/// <inheritdoc />
		public override string ToString() { return $"{Id:D5} {Name} [{Grade:F2}]"; }

		public void MethodWithInputs(int input1, string input2)
		{
		}

		public void MethodWithArrayInputs(int[] input1, string[] input2)
		{
		}

		public void MethodWithInputs(int input1, int[] input2)
		{
		}
	}
}