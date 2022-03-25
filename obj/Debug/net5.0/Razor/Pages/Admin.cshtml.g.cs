#pragma checksum "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "b8eed01b2ec2b6647cc26bc75c74326a5828eb58"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(RazorCoursework.Pages.Pages_Admin), @"mvc.1.0.razor-page", @"/Pages/Admin.cshtml")]
namespace RazorCoursework.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\_ViewImports.cshtml"
using Microsoft.AspNetCore.Identity;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\_ViewImports.cshtml"
using RazorCoursework;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\_ViewImports.cshtml"
using RazorCoursework.Data;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"b8eed01b2ec2b6647cc26bc75c74326a5828eb58", @"/Pages/Admin.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"21afe0981d49a61c87d87f76adbc6ac4b514d154", @"/Pages/_ViewImports.cshtml")]
    #nullable restore
    public class Pages_Admin : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    #nullable disable
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral(@"
<style>
    .row {
        overflow-x: auto;
    }

    td {
        padding: 5px 20px 5px 20px;
        width: 200px;
        cursor: pointer;
    }

    .review-row {
        transition: all ease-in-out 0.25s;
    }

    .review-row:hover {
        background-color: #606060;
        color: white;
    }

    table {
        width: 80%; 
        overflow: scroll; 
        margin-left: auto; 
        margin-right: auto
    }
</style>

<h2 class=""text-center"">
    Все пользователи
</h2>
<div class=""row"" style=""margin-bottom: 50px"">
    <table>
");
#nullable restore
#line 39 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml"
         if (Model.users.Count > 0)
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <tr class=\"text-center\">\r\n                <td>\r\n                    Пользователь\r\n                </td>\r\n                <td>\r\n                    Роль\r\n                </td>\r\n            </tr>\r\n");
#nullable restore
#line 49 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml"
             foreach (var user in Model.users)
            {

#line default
#line hidden
#nullable disable
            WriteLiteral("                <tr class=\"border border-dark review-row bg-themed text-center\"");
            BeginWriteAttribute("onclick", " \r\n                    onclick=\"", 1035, "\"", 1139, 3);
            WriteAttributeValue("", 1067, "location.href=\'", 1067, 15, true);
#nullable restore
#line 52 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml"
WriteAttributeValue("", 1082, Url.Page("Home", new { user = user.UserName, p = 1 }), 1082, 56, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 1138, "\'", 1138, 1, true);
            EndWriteAttribute();
            WriteLiteral(">\r\n                    <td class=\"border border-dark\">\r\n                        ");
#nullable restore
#line 54 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml"
                   Write(user.UserName);

#line default
#line hidden
#nullable disable
            WriteLiteral(" (");
#nullable restore
#line 54 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml"
                                   Write(Model.GetCreatorLikesCount(user.Id));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                        <img src=\"https://img.icons8.com/material/24/000000/like--v1.png\"\r\n                            width=\"15\" height=\"15\" />)\r\n                    </td>\r\n                    <td class=\"border border-dark\">\r\n                        ");
#nullable restore
#line 59 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml"
                   Write(Model.GetRole(user.Id));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                </tr>\r\n");
#nullable restore
#line 62 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml"
            }

#line default
#line hidden
#nullable disable
#nullable restore
#line 62 "C:\Users\Schweppes_psina\Desktop\Проекты\C# Projects\Coursework-Itransition\Pages\Admin.cshtml"
             
        }

#line default
#line hidden
#nullable disable
            WriteLiteral("    </table>\r\n</div>   \r\n");
        }
        #pragma warning restore 1998
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<RazorCoursework.Pages.AdminModel> Html { get; private set; } = default!;
        #nullable disable
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<RazorCoursework.Pages.AdminModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<RazorCoursework.Pages.AdminModel>)PageContext?.ViewData;
        public RazorCoursework.Pages.AdminModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
