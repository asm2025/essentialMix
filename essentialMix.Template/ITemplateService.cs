using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Scriban;
using Scriban.Runtime;

namespace essentialMix.Template;

public interface ITemplateService
{
	Scriban.Template RegisterOrGet([NotNull] string fileName);
	Scriban.Template RegisterOrGet([NotNull] string fileName, TimeSpan cacheTimeout);
	Scriban.Template RegisterOrGet([NotNull] string fileName, Encoding encoding);
	Scriban.Template RegisterOrGet([NotNull] string fileName, Encoding encoding, TimeSpan cacheTimeout);
	void Unregister([NotNull] string fileName);
	string Render([NotNull] string template, [NotNull] TemplateContext context);
	string Render([NotNull] string template);
	string Render([NotNull] string template, object model);
	string Render([NotNull] string template, object model, MemberRenamerDelegate memberRenamer);
	string Render([NotNull] string template, object model, MemberFilterDelegate filter);
	string Render([NotNull] string template, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter);
	ValueTask<string> RenderAsync([NotNull] string template, [NotNull] TemplateContext context, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderAsync([NotNull] string template, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderAsync([NotNull] string template, object model, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderAsync([NotNull] string template, object model, MemberRenamerDelegate memberRenamer, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderAsync([NotNull] string template, object model, MemberFilterDelegate filter, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderAsync([NotNull] string template, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter, CancellationToken token = default(CancellationToken));
	string RenderFile([NotNull] string fileName, [NotNull] TemplateContext context);
	string RenderFile([NotNull] string fileName);
	string RenderFile([NotNull] string fileName, object model);
	string RenderFile([NotNull] string fileName, object model, MemberRenamerDelegate memberRenamer);
	string RenderFile([NotNull] string fileName, object model, MemberFilterDelegate filter);
	string RenderFile([NotNull] string fileName, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter);
	ValueTask<string> RenderFileAsync([NotNull] string fileName, [NotNull] TemplateContext context, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderFileAsync([NotNull] string fileName, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderFileAsync([NotNull] string fileName, object model, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderFileAsync([NotNull] string fileName, object model, MemberRenamerDelegate memberRenamer, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderFileAsync([NotNull] string fileName, object model, MemberFilterDelegate filter, CancellationToken token = default(CancellationToken));
	ValueTask<string> RenderFileAsync([NotNull] string fileName, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter, CancellationToken token = default(CancellationToken));
}