using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;

namespace asm.Web.WebControls.Adapters.Bootstrap
{
	public class RadioButtonListAdapter : WebControlAdapter
	{
		/// <inheritdoc />
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
		}

		/// <inheritdoc />
		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		/// <inheritdoc />
		protected override void RenderContents(HtmlTextWriter writer)
		{
			RadioButtonList control = (RadioButtonList)Control;
			string cssClass = "form-check" + (control.RepeatDirection == RepeatDirection.Horizontal ? " form-check-inline" : string.Empty);
			writer.Indent++;

			int cnt = 0;

			foreach (ListItem item in control.Items)
			{
				string id = $"{control.ClientID}_{cnt++}";

				// container
				writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.Indent++;

				// input
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "form-check-input");
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
				writer.AddAttribute(HtmlTextWriterAttribute.Value, item.Value);
				writer.AddAttribute(HtmlTextWriterAttribute.Name, control.ClientID);
				writer.AddAttribute(HtmlTextWriterAttribute.Id, id);
				if (item.Selected) writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();

				// label
				writer.AddAttribute(HtmlTextWriterAttribute.For, id);
				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.Write(item.Value);
				writer.RenderEndTag();

				writer.Indent--;
				writer.RenderEndTag();
			}

			writer.Indent--;
		}
	}
}
