using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace yaf
{
	/// <summary>
	/// Summary description for attachments.
	/// </summary>
	public class attachments : BasePage
	{
		private DataRow forum, topic;
		protected HyperLink HomeLink, CategoryLink, ForumLink, TopicLink, ThisLink;
		protected Repeater List;
		protected Button Back, Upload;
		protected HtmlInputFile File;

		private void Page_Load(object sender, System.EventArgs e)
		{
			using(DataTable dt = DB.forum_list(PageForumID))
				forum = dt.Rows[0];
			topic = DB.topic_info(PageTopicID);

			if(!IsPostBack) 
			{
				if(!ForumModeratorAccess && !ForumUploadAccess)
					Data.AccessDenied();

				if(!ForumReadAccess)
					Data.AccessDenied();

				if((bool)topic["IsLocked"]) 
					Data.AccessDenied(/*"The topic is closed."*/);

				if((bool)forum["Locked"]) 
					Data.AccessDenied(/*"The forum is closed."*/);

				// Check that non-moderators only edit messages they have written
				if(!ForumModeratorAccess) 
					using(DataTable dt = DB.message_list(Request.QueryString["m"])) 
						if((int)dt.Rows[0]["UserID"] != PageUserID) 
							Data.AccessDenied(/*"You didn't post this message."*/);
		
				HomeLink.NavigateUrl = BaseDir;
				HomeLink.Text = ForumName;
				CategoryLink.NavigateUrl = String.Format("default.aspx?c={0}",PageCategoryID);
				CategoryLink.Text = PageCategoryName;
				ForumLink.NavigateUrl = String.Format("topics.aspx?f={0}",forum["ForumID"]);
				ForumLink.Text = (string)forum["Name"];
				TopicLink.NavigateUrl = String.Format("posts.aspx?t={0}",topic["TopicID"]);
				TopicLink.Text = (string)topic["Topic"];
				ThisLink.NavigateUrl = Request.RawUrl;
				ThisLink.Text = GetText("TITLE");

				Back.Text = GetText("BACK");
				Upload.Text = GetText("UPLOAD");

				BindData();
			}
		}

		private void BindData() 
		{
			List.DataSource = DB.attachment_list(Request.QueryString["m"]);
			DataBind();
		}

		protected void Delete_Load(object sender, System.EventArgs e) 
		{
			((LinkButton)sender).Attributes["onclick"] = String.Format("return confirm('{0}')",GetText("ASK_DELETE"));
		}

		private void Back_Click(object sender, System.EventArgs e) 
		{
			Response.Redirect(String.Format("posts.aspx?m={0}#{0}",Request.QueryString["m"]));
		}

		private void List_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
		{
			switch(e.CommandName) 
			{
				case "delete":
					DB.attachment_delete(e.CommandArgument);
					BindData();
					break;
			}
		}

		private void Upload_Click(object sender, System.EventArgs e) 
		{
			try 
			{
				CheckValidFile(File);
				SaveAttachment(Request.QueryString["m"],File);
				BindData();
			}
			catch(Exception x) 
			{
				AddLoadMessage(x.Message);
				return;
			}
		}

		private void CheckValidFile(HtmlInputFile file) 
		{
			if(file.PostedFile==null || file.PostedFile.FileName.Trim().Length==0 || file.PostedFile.ContentLength==0)
				return;

			string filename = file.PostedFile.FileName;
			int pos = filename.LastIndexOfAny(new char[]{'/','\\'});
			if(pos>=0)
				filename = filename.Substring(pos+1);
			pos = filename.LastIndexOf('.');
			if(pos>=0) 
			{
				switch(filename.Substring(pos+1).ToLower()) 
				{
					default:
						break;
					case "asp":
					case "aspx":
					case "ascx":
					case "config":
					case "php":
					case "php3":
					case "js":
					case "vb":
					case "vbs":
						throw new Exception(String.Format(GetText("fileerror"),filename));
				}
			}
		}

		private void SaveAttachment(object messageID,HtmlInputFile file) 
		{
			if(file.PostedFile==null || file.PostedFile.FileName.Trim().Length==0 || file.PostedFile.ContentLength==0)
				return;

			string sUpDir = Request.MapPath(System.Configuration.ConfigurationSettings.AppSettings["uploaddir"]);

			string filename = file.PostedFile.FileName;
			int pos = filename.LastIndexOfAny(new char[]{'/','\\'});
			if(pos>=0)
				filename = filename.Substring(pos+1);

			file.PostedFile.SaveAs(sUpDir + filename);
			DB.attachment_save(messageID,filename,file.PostedFile.ContentLength);
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			Back.Click +=new EventHandler(Back_Click);
			Upload.Click += new EventHandler(Upload_Click);
			List.ItemCommand += new RepeaterCommandEventHandler(List_ItemCommand);
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
