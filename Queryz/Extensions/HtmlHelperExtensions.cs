using System.Collections.Generic;
using Extenso.AspNetCore.Mvc;
using Extenso.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Queryz.Data.Domain;

namespace Queryz.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static Queryz<TModel> Queryz<TModel>(this IHtmlHelper<TModel> html) where TModel : class
        {
            return new Queryz<TModel>(html);
        }
    }

    public class Queryz<TModel>
        where TModel : class
    {
        private readonly IHtmlHelper<TModel> html;

        internal Queryz(IHtmlHelper<TModel> html)
        {
            this.html = html;
        }

        public IHtmlContent RolesCheckBoxList(
            RoleManager<ApplicationRole> roleManager,
            string name,
            IEnumerable<string> selectedRoleIds,
            object labelHtmlAttributes = null,
            object checkboxHtmlAttributes = null,
            bool inputInsideLabel = true,
            bool wrapInDiv = true,
            object wrapperHtmlAttributes = null)
        {
            var selectList = roleManager.Roles
                .ToSelectList(
                    value => value.Id,
                    text => text.Name);

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
}