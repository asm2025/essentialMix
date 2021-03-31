using System;

namespace TestApp
{
	internal class Student : IComparable<Student>, IComparable, IEquatable<Student>
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public double Grade { get; set; }

		/// <inheritdoc />
		public override string ToString() { return $"{Id:D5} {Name} [{Grade:F2}]"; }

		/// <inheritdoc />
		public int CompareTo(Student other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(other, null)) return -1;
			int cmp = Grade.CompareTo(other.Grade);
			return cmp != 0
						? cmp
						: string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
		}

		/// <inheritdoc />
		int IComparable.CompareTo(object obj) { return CompareTo(obj as Student); }

		/// <inheritdoc />
		public bool Equals(Student other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && Grade.Equals(other.Grade);
		}
	}
}