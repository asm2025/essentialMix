namespace asm.Windows.Html
{
	/// <summary>
	/// Represent HTML TAG with name and list of attributes. If End is true
	/// than this is the end of an HTML Tag
	/// </summary>
	public class Tag : Part
	{
		private string _name;

		public Tag(string name, AttributeList attrList)
			: base(PartType.Tag) { Set(name, false, attrList); }

		public override string ToString() { return $"TAG: {_name};{End}\n{AttrList}"; }

		public bool End { get; set; }

		public string Name
		{
			get => _name;
			set
			{
				if (value.Length > 0)
				{
					// check for end attribute </xxx> or <xxx />
					if (value[0] == '/' || value[value.Length - 1] == '/')
					{
						int start = value[0] == '/' ? 1 : 0;
						End = true;
						_name = value.Substring(start, value.Length - 1);
						return;
					}
				}

				_name = value;
			}
		}

		public AttributeList AttrList { get; set; }

		private void Set(string name, bool end, AttributeList attrList)
		{
			End = end;
			Name = name; // name can re-set end attribute ==> Name = xxx!!
			AttrList = attrList != null ? new AttributeList(attrList) : null;
		}
	}
}