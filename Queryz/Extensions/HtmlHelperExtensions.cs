namespace Queryz.Extensions;

public static class HtmlHelperExtensions
{
    public static Queryz<TModel> Queryz<TModel>(this IHtmlHelper<TModel> html) where TModel : class => new(html);
}

public class Queryz<TModel>
    where TModel : class
{
    private readonly IHtmlHelper<TModel> html;

    internal Queryz(IHtmlHelper<TModel> html)
    {
        this.html = html;
    }

    public async Task<IHtmlContent> RolesCheckBoxListAsync(
        IRoleService roleService,
        string name,
        IEnumerable<string> selectedRoleIds,
        object labelHtmlAttributes = null,
        object checkboxHtmlAttributes = null,
        bool inputInsideLabel = true,
        bool wrapInDiv = true,
        object wrapperHtmlAttributes = null)
    {
        var selectList = (await roleService.GetRoles())
            .ToSelectList(value => value.Id, text => text.Name);

        return html.CheckBoxList(
            name,
            selectList,
            selectedRoleIds,
            labelHtmlAttributes: labelHtmlAttributes,
            checkboxHtmlAttributes: checkboxHtmlAttributes,
            inputInsideLabel: inputInsideLabel,
            wrapInDiv: wrapInDiv,
            wrapperHtmlAttributes: wrapperHtmlAttributes);
    }
}