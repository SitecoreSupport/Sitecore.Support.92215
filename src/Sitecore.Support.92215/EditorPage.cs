using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Resources.Media;
using Sitecore.Security.Accounts;
using Sitecore.SecurityModel;
using Sitecore.Shell.Applications.ContentEditor.RichTextEditor;
using Sitecore.Shell.Controls.RichTextEditor;
using Sitecore.Shell.Controls.RichTextEditor.Pipelines.LoadRichTextContent;
using Sitecore.Shell.Controls.RichTextEditor.Pipelines.SaveRichTextContent;
using Sitecore.StringExtensions;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Ajax;
using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace Sitecore.Support.Shell.Controls.RichTextEditor
{
    /// <summary>
    /// The editor page.
    /// </summary>
    public class EditorPage : System.Web.UI.Page
    {
        /// <summary>
        /// EditorStyles control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected System.Web.UI.WebControls.PlaceHolder EditorStyles;

        /// <summary>
        /// ScriptConstants control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected System.Web.UI.WebControls.PlaceHolder ScriptConstants;

        /// <summary>
        /// mainForm control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected System.Web.UI.HtmlControls.HtmlForm mainForm;

        /// <summary>
        /// ScriptManager1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected RadScriptManager ScriptManager1;

        /// <summary>
        /// EditorUpdatePanel control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected UpdatePanel EditorUpdatePanel;

        /// <summary>
        /// formDecorator control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected RadFormDecorator formDecorator;

        /// <summary>
        /// Editor control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected RadEditor Editor;

        /// <summary>
        /// EditorClientScripts control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected System.Web.UI.WebControls.PlaceHolder EditorClientScripts;

        /// <summary>
        /// OkButton control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected Sitecore.Web.UI.HtmlControls.Button OkButton;

        /// <summary>
        /// CancelButton control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected Sitecore.Web.UI.HtmlControls.Button CancelButton;

        /// <summary>
        /// Handles the Accept_ click event.
        /// </summary>
        protected void OnAccept()
        {
            string content = base.Request.Form["EditorValue"];
            SaveRichTextContentArgs saveRichTextContentArgs = new SaveRichTextContentArgs(content);
            saveRichTextContentArgs.Content = WebEditUtil.RepairLinks(saveRichTextContentArgs.Content);
            using (new LongRunningOperationWatcher(250, "saveRichTextContent", new string[0]))
            {
                CorePipeline.Run("saveRichTextContent", saveRichTextContentArgs);
            }
            RichTextEditorUrl richTextEditorUrl = RichTextEditorUrl.Parse(this.Context.Request.RawUrl);
            if (!richTextEditorUrl.ShowInFrameBasedDialog)
            {
                SheerResponse.Eval(string.Format("scRichText.saveRichText({0})", StringUtil.EscapeJavascriptString(saveRichTextContentArgs.Content)));
            }
            else
            {
                SheerResponse.SetDialogValue(saveRichTextContentArgs.Content);
            }
            SheerResponse.Eval("scCloseEditor();");
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event to initialize the page.
        /// </summary>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);
            Client.AjaxScriptManager.OnExecute += new AjaxScriptManager.ExecuteDelegate(this.AjaxScriptManager_OnExecute);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.EventArgs" /> object that contains the event data.
        /// </param>
        protected override void OnLoad(System.EventArgs e)
        {
            User user = Sitecore.Context.User;
            if (!user.IsInRole(Constants.SitecoreClientUsersRole) && !user.IsAdministrator)
            {
                WebUtil.RedirectToLoginPage();
            }
            base.OnLoad(e);
            if (base.IsPostBack)
            {
                return;
            }
            string queryString = WebUtil.GetQueryString("hdl");
            if (string.IsNullOrEmpty(queryString))
            {
                return;
            }
            EditorConfigurationResult editorConfigurationResult;
            using (new UserSwitcher(user))
            {
                using (new SecurityEnabler())
                {
                    string queryString2 = WebUtil.GetQueryString("so", Settings.HtmlEditor.DefaultProfile);
                    Assert.IsNotNull(queryString2, "source");
                    editorConfigurationResult = null;
                    Database database = Sitecore.Context.Database;
                    Assert.IsNotNull(database, "database");
                    Item item = database.GetItem(queryString2);
                    if (item != null)
                    {
                        EditorConfiguration editorConfiguration = EditorConfiguration.Create(item);
                        editorConfiguration.Language = WebUtil.GetQueryString("la");
                        editorConfigurationResult = editorConfiguration.Apply(this.Editor);
                        if (!string.IsNullOrEmpty(WebUtil.GetQueryString("sc_content")))
                        {
                            RadEditor expr_D0 = this.Editor;
                            expr_D0.DialogHandlerUrl = expr_D0.DialogHandlerUrl + "?sc_content=" + WebUtil.GetQueryString("sc_content");
                        }
                    }
                    else
                    {
                        Item item2 = database.GetItem(Settings.HtmlEditor.DefaultProfile);
                        if (item2 != null)
                        {
                            EditorConfiguration editorConfiguration2 = EditorConfiguration.Create(item2);
                            editorConfiguration2.Language = WebUtil.GetQueryString("la");
                            editorConfigurationResult = editorConfiguration2.Apply(this.Editor);
                        }
                    }
                }
            }
            this.RegisterMediaPrefixes();
            this.EditorClientScripts.Controls.Add(new System.Web.UI.LiteralControl(editorConfigurationResult.Scripts.ToString()));
            this.EditorStyles.Controls.Add(new System.Web.UI.LiteralControl(editorConfigurationResult.Styles.ToString()));
            this.RenderScriptConstants();
            this.LoadHtml();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.PreRenderComplete" /> event after the <see cref="M:System.Web.UI.Page.OnPreRenderComplete(System.EventArgs)" /> event and before the page is rendered.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnPreRenderComplete(System.EventArgs e)
        {
            base.OnPreRenderComplete(e);
            if (!base.IsPostBack)
            {
                bool arg_19_0 = Client.AjaxScriptManager.IsEvent;
            }
        }

        /// <summary>
        /// Called when [reject].
        /// </summary>
        protected void OnReject()
        {
            SheerResponse.Eval("scCloseEditor();");
        }

        /// <summary>
        /// Handles the OnExecute event of the AjaxScriptManager control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="args">
        /// The arguments.
        /// </param>
        private void AjaxScriptManager_OnExecute(object sender, AjaxCommandEventArgs args)
        {
            if (args.Name == "editorpage:accept")
            {
                this.OnAccept();
                return;
            }
            if (args.Name == "editorpage:reject")
            {
                this.OnReject();
            }
        }

        /// <summary>
        /// Populates the editor with HTML
        /// </summary>
        private void LoadHtml()
        {
            string queryString = WebUtil.GetQueryString("hdl");
            Assert.IsNotNullOrEmpty(queryString, "html value handle");
            string sessionString = WebUtil.GetSessionString(queryString);
            WebUtil.RemoveSessionValue(queryString);
            LoadRichTextContentArgs loadRichTextContentArgs = new LoadRichTextContentArgs(sessionString);
            using (new LongRunningOperationWatcher(250, "loadRichTextContent", new string[0]))
            {
                CorePipeline.Run("loadRichTextContent", loadRichTextContentArgs);
            }

            //ALEX20170905 
            string contentToDisplay = loadRichTextContentArgs.Content;
            while (contentToDisplay.Contains("RadESpellError_") & contentToDisplay.Contains("RadEWrongWord"))
            {
                // remove the string from the position "<span class="RadEWrongWord"..." until ">"
                int StartPosToDelete = contentToDisplay.IndexOf("<span class=\"RadEWrongWord\"");
                int EndPosToDelete = contentToDisplay.IndexOf(">", StartPosToDelete);
                int diff = 0;
                if (EndPosToDelete > StartPosToDelete)
                {
                    diff = EndPosToDelete - StartPosToDelete + 1;
                }

                contentToDisplay = contentToDisplay.Remove(StartPosToDelete, diff);

                // remove the next "</span>"
                StartPosToDelete = contentToDisplay.IndexOf("</span>", StartPosToDelete - 1);
                EndPosToDelete = contentToDisplay.IndexOf(">", StartPosToDelete);

                if (EndPosToDelete > StartPosToDelete)
                {
                    diff = EndPosToDelete - StartPosToDelete + 1;
                }

                contentToDisplay = contentToDisplay.Remove(StartPosToDelete, diff);
            }

            this.Editor.Content = contentToDisplay;
        }

        /// <summary>
        /// Registers the media prefixes.
        /// </summary>
        private void RegisterMediaPrefixes()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach (string current in MediaManager.Provider.Config.MediaPrefixes)
            {
                stringBuilder.Append("|" + current.Replace("\\", "\\\\").Replace("/", "\\/"));
            }
            base.ClientScript.RegisterClientScriptBlock(base.GetType(), "mediaPrefixes", "var prefixes = '" + stringBuilder + "';", true);
        }

        /// <summary>
        /// Renders the script constants.
        /// </summary>
        private void RenderScriptConstants()
        {
            string text = "\r\n            var scClientID = '{0}';\r\n            var scItemID = '{1}';\r\n            var scLanguage = '{2}';\r\n            var scDatabase = '{3}';\r\n            var scRemoveScripts = '{4}';\r\n         ".FormatWith(new object[]
            {
                this.Editor.ClientID,
                System.Web.HttpUtility.JavaScriptStringEncode(WebUtil.GetQueryString("id")),
                System.Web.HttpUtility.JavaScriptStringEncode(WebUtil.GetQueryString("la")),
                System.Web.HttpUtility.JavaScriptStringEncode(WebUtil.GetQueryString("sc_content")),
                Settings.HtmlEditor.RemoveScripts.ToString().ToLowerInvariant()
            });
            this.ScriptConstants.Controls.Add(new System.Web.UI.LiteralControl(text));
        }
    }
}
