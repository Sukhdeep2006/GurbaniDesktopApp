using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Office.Core;
//using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace GurbaniDesktopApp
{
    public partial class FormConsole : Form
    {
        public DataSet DSMain;
        public DataSet dsSearch;
        public DataSet dsKosh;
        string QuerySQL = string.Empty;
        private static Logger objLogger = null;
        DisplaySettings objDisSettings;
        string CurrentDir = string.Empty;
        string AppDataDir = string.Empty;

        FormDisplay objForm;
        FormDisplay objSubtitle;
        FormDisplay objPreview;

        string pptpath = string.Empty;
        //Process.Start(path);

        //PowerPoint.Application app;
        //PowerPoint.Presentations pres;
        //PowerPoint.Presentation file;
        
        bool IsFormLoad = true;

        System.Data.DataTable dtHistory;

        public FormConsole()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            objLogger = Logger.InstanceCreate();
            try
            {               
                Cursor.Current = Cursors.WaitCursor;

                CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
                AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + @"\GurbaniApp";
                
                DirectoryInfo dirInfo = new DirectoryInfo(Path.GetFullPath(AppDataDir));
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                //ckbPreview.Visible = false;
                lsbSearch.DataSource = null;
                lsbSearch.Visible = false;
                LoadSearchOptions();
                SetDefaultSearchOptions();

                QuerySQL = GetDefaultQuerySQL();
                DSMain = GetDataBase(QuerySQL);

                InitialiseSettings();
                objForm = new FormDisplay(objDisSettings);
                objSubtitle = new FormDisplay(objDisSettings);
                objForm.Name = "Gurbani-Projector";
                objSubtitle.Name = "Gurbani-SubTitle";
                objForm.Text = "Gurbani-Projector";
                objSubtitle.Text = "Gurbani-SubTitle";

                objPreview = new FormDisplay(objDisSettings);

                dtHistory = new System.Data.DataTable();
                dtHistory.Columns.Add("VerseID", typeof(Int32));
                dtHistory.Columns.Add("Gurmukhi", typeof(string));

                lsbHistory.Font = new System.Drawing.Font("GurbaniWebThick", 13f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                lsbSearch.Font = new System.Drawing.Font("GurbaniWebThick", 13f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                lsbDisplay1.Font = new System.Drawing.Font("GurbaniWebThick", 13f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                lsbDisplay2.Font = new System.Drawing.Font("GurbaniWebThick", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                lblKoshMeaning.Font = new System.Drawing.Font("GurbaniLipi", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                txtKoshSearch.Font = new System.Drawing.Font("GurbaniLipi", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                lsbKoshSearch.Font = new System.Drawing.Font("GurbaniWebThick", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                
                txtSearch.Focus();
                timer1.Start();
                Cursor.Current = Cursors.Default;
                IsFormLoad = false;
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                WriteLog(LogMessageType.Exception, "Form1_Load", ex.ToString());
            }
        }

        private void InitialiseSettings()
        {
            try
            {
                objDisSettings = new DisplaySettings();
                DisplaySettings obj = LoadFromXMLString();
                if (obj != null)
                {
                    objDisSettings.DisplayMode = obj.DisplayMode;
                    objDisSettings.DisplayEnglishTranslation = obj.DisplayEnglishTranslation;
                    objDisSettings.DisplayPunjabiTranslation = obj.DisplayPunjabiTranslation;
                    objDisSettings.GurbaniColor = obj.GurbaniColor;
                    objDisSettings.BorderColor = obj.BorderColor;
                    objDisSettings.EnglishTranslationColor = obj.EnglishTranslationColor;
                    objDisSettings.PunjabiTranslationColor = obj.PunjabiTranslationColor;
                    objDisSettings.MaxEnglishFontSize = obj.MaxEnglishFontSize;
                    objDisSettings.MaxGurbaniFontSize = obj.MaxGurbaniFontSize;
                    objDisSettings.MaxPunjabiFontSize = obj.MaxPunjabiFontSize;
                    objDisSettings.GurbaniFontName = obj.GurbaniFontName;
                    objDisSettings.TranslationLanguage = obj.TranslationLanguage;
                    objDisSettings.EnglishFontName = obj.EnglishFontName;                    
                    objDisSettings.DisplayBackColor = obj.DisplayBackColor;
                    objDisSettings.PreviewEnabled = obj.PreviewEnabled;
                    objDisSettings.SlideType = obj.SlideType;
                    objDisSettings.AppDirectory = AppDataDir.ToString();
                    //objDisSettings.TemplateName = AppDataDir.ToString() + "\\Templates\\BlueWhale.jpg";
                    objDisSettings.TemplateName = obj.TemplateName;
                    objDisSettings.SubTitleOpacity = obj.SubTitleOpacity;
                    objDisSettings.ProjectorScreen = obj.ProjectorScreen;
                    
                }
                propDisplaySettings.SelectedObject = objDisSettings;

            }
            catch (Exception e)
            {
                WriteLog(LogMessageType.Exception, "InitialiseSettings", e.ToString());
            }
        }

        private void LoadSearchOptions()
        {
            string SearchSQL = @"select RaagID, RaagWithPage, RaagGurmukhi, RaagUniCode from raag where ENDID > 0;
                                Select UniqueID as SourceID, SourceGurmukhi, SourceUnicode, SourceEnglish from Source ;
                                Select WriterID, WriterEnglish, WriterGurmukhi from Writer;";

            dsSearch = GetDataBase(SearchSQL);

            cmbSearchType.DataSource = Enum.GetValues(typeof(SearchType));
            cmbFilterType.DataSource = Enum.GetValues(typeof(FilterType));
        }

        private void SetDefaultSearchOptions()
        {
            cmbSearchType.SelectedItem = SearchType.First_Letter_Anywhere;
            cmbFilterType.SelectedItem = FilterType.None;
            ClearSearch();

        }

        private string GetDefaultQuerySQL()
        {
            // Highest ID : 402609
            // C:\\Users\\Sukhdeep.Singh1\\Documents\\Visual Studio 2015\\Projects\\GurbaniDesktopApp\\GurbaniDesktopApp\\bin\\Debug\

            //return @" SELECT VRS.ID as VerseID, VRS.ID || "" "" ||  VRS.Gurmukhi as Gurmukhi, VRS.English, VRS.Punjabi,VRS.Transliteration, VRS.FirstLetterStr,VRS.MainLetters, VRS.FirstLetterEng, SBD.ShabadID, 
            //                                    SRC.SourceGurmukhi, SRC.SourceEnglish , WTR.WriterEnglish, WTR.WriterGurmukhi, RG.RaagGurmukhi, RG.RaagEnglish, VRS.PageNo,
            //VRS.RaagID, VRS.WriterID, SRC.UniqueID as SourceID
            //                    from Verse VRS
            //                    left Join Shabad SBD on VRS.ID = SBD.VerseID
            //                    left Join Source SRC on VRS.SOURCEID = SRC.SOURCEID
            //                    left Join Writer WTR on VRS.WRITERID = WTR.WRITERID
            //                    left Join RAAG RG on VRS.RaagID = RG.RAAGID";

            return @" SELECT VRS.ID as VerseID, VRS.Gurmukhi as Gurmukhi, VRS.English, VRS.Punjabi,VRS.Transliteration, VRS.FirstLetterStr,VRS.MainLetters, VRS.FirstLetterEng, SBD.ShabadID, 
                                SRC.SourceGurmukhi, SRC.SourceEnglish , WTR.WriterEnglish, WTR.WriterGurmukhi, RG.RaagGurmukhi, RG.RaagEnglish, VRS.PageNo,
								VRS.RaagID, VRS.WriterID, SRC.UniqueID as SourceID
                            from Verse VRS
                            left Join Shabad SBD on VRS.ID = SBD.VerseID
                            left Join Source SRC on VRS.SOURCEID = SRC.SOURCEID
                            left Join Writer WTR on VRS.WRITERID = WTR.WRITERID
                            left Join RAAG RG on VRS.RaagID = RG.RAAGID";

        }

        private void SetFiletredQuerySQL(string filter)
        {
            QuerySQL = QuerySQL + " " + filter;
        }

        public DataSet GetDataBase(string SQL)
        {
            try
            {
                //string connString = "Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "gdadts;Version=3;";
                string connString = "Data Source=" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + @"\GurbaniApp\" + "gdadts;Version=3;";
                DataSet dsObj = new DataSet();
                SQLiteConnection m_dbConnection = new SQLiteConnection(connString);
                m_dbConnection.Open();
                //WriteLog(LogMessageType.Debug, "GetDataBase", connString.ToString());
                //WriteLog(LogMessageType.Debug, "GetDataBase", SQL.ToString());
                SQLiteDataAdapter da = new SQLiteDataAdapter(SQL, m_dbConnection);
                da.Fill(dsObj);
                m_dbConnection.Close();

                return dsObj;
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to load database file.", "ERROR");
                WriteLog(LogMessageType.Exception, "GetDataBase", e.ToString());
                return null;
            }
        }

        protected static void WriteLog(LogMessageType level, string funcName, string message)
        {
            string source = "FormConsole." + funcName;
            
            objLogger.LogMessage(level, source, message);
            //if ((int)level >= (int)LogMessageType.Error)
            //    objLogger.LogMessage(LogMessageType.Error, source, message);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtSearch.ForeColor = System.Drawing.SystemColors.ControlText;
                if (txtSearch.Text.ToString().Trim().Length > 0 && (txtSearch.Text.ToString() != "tweIp kro..." && txtSearch.Text.ToString() != "Type here..."))
                {
                    if (DSMain != null && DSMain.Tables.Count > 0 && DSMain.Tables[0].Rows.Count > 0)
                    {
                        System.Data.DataTable tbl = DSMain.Tables[0];
                        string searchString = string.Empty;
                        string filterString = string.Empty;
                        string asciiString = string.Empty;

                        foreach (char ch in txtSearch.Text)
                        {
                            int i = (int)ch;
                            string cc = i.ToString().Length > 2 ? i.ToString() : "0" + i.ToString();
                            asciiString += "," + cc;
                        }

                        #region Search

                        if (cmbSearchType.SelectedIndex >= 0)
                        {
                            int SearchIndex = Convert.ToInt32(cmbSearchType.SelectedItem);

                            switch (SearchIndex)
                            {
                                case (int)SearchType.First_Letter_English:
                                    searchString = "FirstLetterEng Like '%" + txtSearch.Text.ToString().Trim() + "%'";
                                    break;

                                case (int)SearchType.First_Letter_Start:
                                    searchString = "firstLetterStr Like '" + asciiString.ToString().Trim() + "%'";
                                    break;

                                case (int)SearchType.First_Letter_Anywhere:
                                    searchString = "firstLetterStr Like '%" + asciiString.ToString().Trim() + "%'";
                                    break;

                                case (int)SearchType.Main_Letters_Start:
                                    searchString = "MainLetters Like '" + txtSearch.Text.ToString() + "%'";
                                    break;

                                case (int)SearchType.Main_Letters_Anywhere:
                                    searchString = "MainLetters Like '%" + txtSearch.Text.ToString() + "%'";
                                    break;

                                case (int)SearchType.Full_Word_Gurmukhi:
                                    searchString = "Gurmukhi Like '%" + txtSearch.Text.ToString() + "%'";
                                    break;

                                case (int)SearchType.Full_Word_English:
                                    searchString = "Transliteration Like '%" + txtSearch.Text.ToString() + "%'";
                                    break;

                                case (int)SearchType.Page_Number:
                                    searchString = "PageNo = " + txtSearch.Text.ToString() + "";
                                    break;

                                default:
                                    searchString = "firstLetterStr Like '%" + asciiString.ToString().Trim() + "%'";
                                    break;
                            }
                        }

                        #endregion Search

                        #region Filter
                        if (cmbFilterType.SelectedIndex >= 0)
                        {
                            int filterIndex = Convert.ToInt32(cmbFilterType.SelectedItem);

                            switch (filterIndex)
                            {
                                case (int)FilterType.Filter_By_Raag:
                                    searchString += " and RaagID = " + Convert.ToInt32(cmbFilterValues.SelectedValue).ToString();
                                    break;

                                case (int)FilterType.Filter_By_Source:
                                    searchString += " and SourceID = " + Convert.ToInt32(cmbFilterValues.SelectedValue).ToString();
                                    break;

                                case (int)FilterType.Filter_By_Writer:
                                    searchString += " and WriterID = " + Convert.ToInt32(cmbFilterValues.SelectedValue).ToString();
                                    break;

                                default:
                                    break;

                            }
                        }
                        #endregion Filter

                        LoadSearchCombo(tbl, searchString);
                    }
                }
                else
                {
                    lsbSearch.DataSource = null;
                    lsbSearch.Visible = false;
                }

            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "textBox1_TextChanged", ex.ToString());
            }
        }

        public void LoadSearchCombo(System.Data.DataTable tbl, string searchString)
        {
            try
            {
                if (searchString != "")
                {
                    int count = (Convert.ToInt32(cmbSearchType.SelectedItem) == (int)SearchType.Page_Number
                                || Convert.ToInt32(cmbSearchType.SelectedItem) == (int)SearchType.Compiled_Banis)
                                ? 2200 : 200;
                    IEnumerable<DataRow> sortedRows = new DataView(tbl, searchString, "VerseID",
                        DataViewRowState.CurrentRows).Cast<DataRowView>().Take(count).Select(r => r.Row);

                    if (sortedRows != null && sortedRows.Any())
                   {
                        lsbSearch.DataSource = sortedRows.CopyToDataTable();
                        lsbSearch.ValueMember = "VerseID";
                        lsbSearch.DisplayMember = "Gurmukhi";
                        int fontSize = (int)lsbSearch.Font.Size;
                        int boxHeight = (int)groupBox2.Height - 30;
                        int itemCount = lsbSearch.Items.Count;
                        lsbSearch.Height = (2 * fontSize * itemCount + 30 > boxHeight ? boxHeight : 2 * fontSize * itemCount + 30) < 100 ? 100 :
                                            (2 * fontSize * itemCount + 30 > boxHeight ? boxHeight : 2 * fontSize * itemCount + 30);
                        lsbSearch.Visible = true;
                        lsbSearch.BringToFront();
                    }
                    else
                    {
                        lsbSearch.DataSource = null;
                        lsbSearch.Visible = false;
                    }
                }
                else
                {
                    if (tbl != null && tbl.Rows.Count > 0)
                    {
                        lsbSearch.DataSource = tbl;
                        lsbSearch.ValueMember = "VerseID";
                        lsbSearch.DisplayMember = "Gurmukhi";
                        int fontSize = (int)lsbSearch.Font.Size;
                        int boxHeight = (int)lsbSearch.Height;
                        int itemCount = lsbSearch.Items.Count;
                        lsbSearch.Height = (2 * fontSize * itemCount + 30 > 500 ? 500 : 2 * fontSize * itemCount + 30) < 200 ? 200 :
                                            (2 * fontSize * itemCount + 30 > 500 ? 500 : 2 * fontSize * itemCount + 30);
                        lsbSearch.Visible = true;
                        lsbSearch.BringToFront();
                    }
                    else
                    {
                        lsbSearch.DataSource = null;
                        lsbSearch.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "textBox1_TextChanged", ex.ToString());
            }
        }

        public void ClearSearch()
        {
            txtSearch.Text = "";

            cmbFilterType.Enabled = true;
            cmbFilterValues.DataSource = null;
            txtSearch.Enabled = true;
            lsbSearch.DataSource = null;
            if (txtSearch.Font.Name.ToString() == "GurbaniLipi")
                txtSearch.Text = "tweIp kro...";
            else
                txtSearch.Text = "Type here...";
            txtSearch.ForeColor = System.Drawing.SystemColors.ControlLight;

            cmbFilterType.SelectedItem = FilterType.None;
            //txtSearch.Font = new System.Drawing.Font("GurbaniLipi", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }
              
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (lsbSearch.Items.Count > 0)
                {
                    DataRow[] dra = DSMain.Tables[0].Select("VerseID = " + lsbSearch.SelectedValue.ToString());
                    DataRow Dr = dtHistory.NewRow();
                    Dr["VerseID"] = dra[0]["VerseID"];
                    Dr["Gurmukhi"] = dra[0]["Gurmukhi"];   

                    if (Convert.ToInt32(cmbSearchType.SelectedItem) == (int)SearchType.Page_Number
                                || Convert.ToInt32(cmbSearchType.SelectedItem) == (int)SearchType.Compiled_Banis)
                    {
                        lsbDisplay1.DataSource = (System.Data.DataTable)lsbSearch.DataSource;
                        lsbDisplay1.ValueMember = "VerseID";
                        lsbDisplay1.DisplayMember = "Gurmukhi";
                        lsbDisplay1.SelectedValue = -1;
                        lsbDisplay1.SelectedValue = lsbSearch.SelectedValue;
                        lsbSearch.DataSource = null;
                        //tabControl.SelectedTab = tabPageBroadcast;
                        lsbDisplay1.Focus();
                    }
                    else
                    {
                        dtHistory.Rows.InsertAt(Dr, 0);
                        string ShabadID = dra[0]["ShabadID"].ToString();
                        dra = DSMain.Tables[0].Select("ShabadID = " + ShabadID);
                        if (dra != null && dra.Any())
                        {
                            lsbDisplay1.DataSource = dra.CopyToDataTable();
                            lsbDisplay1.ValueMember = "VerseID";
                            lsbDisplay1.DisplayMember = "Gurmukhi";
                            lsbDisplay1.SelectedValue = -1;
                            lsbDisplay1.SelectedValue = lsbSearch.SelectedValue;
                            lsbSearch.Visible = true;
                            lsbSearch.BringToFront();
                            lsbDisplay1.Focus();
                            //tabControl.SelectedTab = tabPageBroadcast;
                        }
                        else
                        {
                            lsbSearch.DataSource = null;
                            //listBox1.Visible = false;
                        }
                    }
                    SetDefaultSearchOptions();
                    lsbHistory.DataSource = dtHistory;
                    lsbHistory.ValueMember = "VerseID";
                    lsbHistory.DisplayMember = "Gurmukhi";
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "listBox1_DoubleClick", ex.ToString());
            }

        }

        public void DisplayForm2(string VerseIDs)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                VerseIDs = VerseIDs.TrimEnd(',');
                DataRow[] DRows = DSMain.Tables[0].Select("VerseID in (" + VerseIDs.ToString() + ")", "VerseID");
                if (objForm.IsDisposed == true)
                {
                    objForm = new FormDisplay(objDisSettings);
                    objForm.Name = "Gurbani-Projector";
                    objForm.Text = "Gurbani-Projector";

                }

                if (objSubtitle.IsDisposed == true)
                {
                    objSubtitle = new FormDisplay(objDisSettings);
                    objSubtitle.Name = "Gurbani-SubTitle";
                    objSubtitle.Text = "Gurbani-SubTitle";
                }

                if (objPreview.IsDisposed == true)
                {
                    objPreview = new FormDisplay();
                }

                //string ttt = "VerseID = " + VerseIDs.ToString() + " and  ShabadID = " + DRows[0]["ShabadID"].ToString();
                string Gurmukhi = string.Empty;
                string Punjabi = string.Empty;
                string English = string.Empty;
                string DetailsText = string.Empty;
                try
                {
                    //DetailsText = DRows[0]["RaagGurmukhi"].ToString()
                    //            + ", " + DRows[0]["WriterGurmukhi"].ToString()
                    //            + ", " + DRows[0]["SourceGurmukhi"].ToString()
                    //            + (DRows[0]["PageNo"].ToString() == "" ? " " : ", AMg " + DRows[0]["PageNo"].ToString());

                    DetailsText = (DRows[0]["RaagGurmukhi"].ToString() == "" ? " " : DRows[0]["RaagGurmukhi"].ToString()) +
                                    (DRows[0]["WriterGurmukhi"].ToString() == "" ? " " : ", " + DRows[0]["WriterGurmukhi"].ToString()) +
                                    (DRows[0]["SourceGurmukhi"].ToString() == "" ? " " : ", " + DRows[0]["SourceGurmukhi"].ToString()) +
                                    (DRows[0]["PageNo"].ToString() == "" ? " " : ", AMg " + DRows[0]["PageNo"].ToString());
                }
                catch
                {
                    DetailsText = "";
                }
                foreach (DataRow Dr in DRows)
                {
                    Gurmukhi += Dr["Gurmukhi"].ToString() + " ";
                    Punjabi += Dr["Punjabi"].ToString() + " ";
                    English += Dr["English"].ToString() + " ";
                }

                if (Gurmukhi == "" && English == "")
                {
                    Gurmukhi = "vwihgurU ]";
                    English = "Waheguru !!";
                }

                //txtGurbani.Text = Gurmukhi;
                //txtGurbani.Tag = Gurmukhi;
                //txtEnglish.Text = English;
                //txtEnglish.Tag = English;
                //txtPunjabi.Text = Punjabi;
                //txtPunjabi.Tag = Punjabi;

                Screen[] screens = Screen.AllScreens;
                int i = 0;
                i = (int)objDisSettings.ProjectorScreen >= Screen.AllScreens.Length ? Screen.AllScreens.Length - 1 : (int)objDisSettings.ProjectorScreen;

                if (objDisSettings.DisplayMode == DispMode.Projector || objDisSettings.DisplayMode == DispMode.Both)
                {
                    objForm.Size = Screen.AllScreens[i].WorkingArea.Size;
                    objForm.Location = Screen.AllScreens[i].WorkingArea.Location;
                }


                objSubtitle.Size = screens[0].WorkingArea.Size;
                objSubtitle.Height = (int)((screens[0].WorkingArea.Height) / 5);
                int xx = (Int32)((screens[0].WorkingArea.Height) * 0.8);
                objSubtitle.Location = new System.Drawing.Point(0, xx);

                objSubtitle.MaxGurbaniFontSize = 34f;
                objSubtitle.MaxEnglishFontSize = 28f;
                objSubtitle.MaxPunjabiFontSize = 24f;
                objSubtitle.GurbaniColor = Color.White;
                objSubtitle.TranslationLanguage = objDisSettings.TranslationLanguage;
                objSubtitle.EnglishColor = Color.White;
                objSubtitle.PunjabiColor = Color.White;
                objSubtitle.GurbaniFontName = objDisSettings.GurbaniFontName;
                objSubtitle.EnglishFontName = objDisSettings.EnglishFontName;
                objSubtitle.DisplayBackColor = Color.Navy;
                objSubtitle.BackColor = Color.Navy;
                objSubtitle.BorderColor = Color.Navy;
                objSubtitle.Opacity = (float)objDisSettings.SubTitleOpacity / 10;
                objSubtitle.DisplayMode = DispMode.TVSubTitles;
                objSubtitle.Text1 = Gurmukhi;

                objSubtitle.IsSubtitle = true;
                objSubtitle.TemplatePath = string.Empty;
                //objForm.Text2 = Punjabi;   


                if (objDisSettings.TranslationLanguage == TransLanguage.English)
                {
                    objSubtitle.Text3 = English;
                }
                else if (objDisSettings.TranslationLanguage == TransLanguage.Punjabi)
                {
                    objSubtitle.Text3 = Punjabi;
                }
                else
                {
                    objSubtitle.Text3 = "";
                }

                objForm.MaxGurbaniFontSize = objDisSettings.MaxGurbaniFontSize;
                objForm.MaxEnglishFontSize = objDisSettings.MaxEnglishFontSize;
                objForm.MaxPunjabiFontSize = objDisSettings.MaxPunjabiFontSize;
                objForm.GurbaniColor = objDisSettings.GurbaniColor;
                objForm.TranslationLanguage = objDisSettings.TranslationLanguage;
                objForm.EnglishColor = objDisSettings.EnglishTranslationColor;
                objForm.PunjabiColor = objDisSettings.PunjabiTranslationColor;
                objForm.GurbaniFontName = objDisSettings.GurbaniFontName;
                objForm.EnglishFontName = objDisSettings.EnglishFontName;
                objForm.DisplayBackColor = objDisSettings.DisplayBackColor;
                objForm.DisplayMode = objDisSettings.DisplayMode;
                objForm.BorderColor = objDisSettings.BorderColor;
                objForm.Text1 = Gurmukhi;
                objForm.DetailsText = DetailsText;
                //objForm.IsSubtitle = false;
                objForm.TemplatePath = AppDataDir + @"\Templates\" + objDisSettings.TemplateName.ToString() +".jpg" ;
                objForm.IsSubtitle = false;
                //objForm.Text2 = Punjabi;   

                if (objDisSettings.TranslationLanguage == TransLanguage.English)
                {
                    objForm.Text3 = English;
                }
                else if (objDisSettings.TranslationLanguage == TransLanguage.Punjabi)
                {
                    objForm.Text3 = Punjabi;
                }
                else
                {
                    objForm.Text3 = "";
                }

                if (objDisSettings.PreviewEnabled == true)
                {
                    RefreshPreview();

                    objPreview.TopLevel = false;
                    pnlPreview.Controls.Add(objPreview);
                    objPreview.Dock = DockStyle.Fill;
                    objPreview.Show();

                    objPreview.GurbaniColor = objDisSettings.GurbaniColor;
                    objPreview.TranslationLanguage = objDisSettings.TranslationLanguage;
                    objPreview.EnglishColor = objDisSettings.EnglishTranslationColor;
                    objPreview.PunjabiColor = objDisSettings.PunjabiTranslationColor;
                    objPreview.GurbaniFontName = objDisSettings.GurbaniFontName;
                    objPreview.EnglishFontName = objDisSettings.EnglishFontName;
                    objPreview.DisplayBackColor = objDisSettings.DisplayBackColor;
                    objPreview.DisplayMode = objDisSettings.DisplayMode;
                    objPreview.BorderColor = objDisSettings.BorderColor;
                    objPreview.TemplatePath =  AppDataDir + @"\Templates\" + objDisSettings.TemplateName.ToString() + ".jpg";

                    //objPreview.Text2 = Punjabi;               
                    //objPreview.Text3 = English;
                    //objPreview.MaxGurbaniFontSize = (float)(objDisSettings.MaxGurbaniFontSize * 0.6);
                    //objPreview.MaxEnglishFontSize = (float)(objDisSettings.MaxEnglishFontSize * 0.6);
                    //objPreview.MaxPunjabiFontSize = (float)(objDisSettings.MaxPunjabiFontSize * 0.6);
                    
                    objPreview.Text1 = Gurmukhi;
                    objPreview.DetailsText = DetailsText;
                    
                    if (objDisSettings.TranslationLanguage == TransLanguage.English)
                    {
                        objPreview.Text3 = English;
                    }
                    else if (objDisSettings.TranslationLanguage == TransLanguage.Punjabi)
                    {
                        objPreview.Text3 = Punjabi;
                    }
                    else
                    {
                        objPreview.Text3 = "";
                    }

                    objPreview.Show();
                }
                RefreshPreview();

                if (objDisSettings.DisplayMode == DispMode.Projector)
                {
                    objForm.Show();
                    objSubtitle.Hide();
                }

                if (objDisSettings.DisplayMode == DispMode.TVSubTitles)
                {
                    objForm.Hide();
                    objSubtitle.Show();
                }
                if (objDisSettings.DisplayMode == DispMode.Both)
                {
                    objForm.Show();
                    objSubtitle.Show();
                }

                Cursor.Current = Cursors.Default;
                this.BringToFront();

                #region PowerPoint

                /*
                Type officeType = Type.GetTypeFromProgID("Powerpoint.Application");

                if (objDisSettings.DisplayMode == DispMode.Projector)
                {
                    if (officeType != null && objDisSettings.SlideType == SlideTypes.Powerpoint)
                    {
                        if (!DisplayPowerPoint(Gurmukhi, Punjabi, English))
                        { objForm.Show(); }
                    }
                    else
                    {
                        objForm.Show();

                        try {

                            if (file != null)
                            {
                                file.Close();
                            }

                            //if (app.ActivePresentation.SlideShowWindow.Active == MsoTriState.msoTrue)
                            //{  app.ActivePresentation.SlideShowWindow.Presentation.Close(); }

                        }
                        catch (Exception ee)
                        {
                            Cursor.Current = Cursors.Default;
                            WriteLog(LogMessageType.Exception, "DisplayForm2", ee.ToString());
                        }
                    }
                    objSubtitle.Hide();
                }


                if (objDisSettings.DisplayMode == DispMode.TVSubTitles)
                {
                    objForm.Hide();
                    try
                    {

                        if (file != null)
                        {
                            file.Close();
                        }

                        //if (app.ActivePresentation.SlideShowWindow.Active == MsoTriState.msoTrue)
                        //{  app.ActivePresentation.SlideShowWindow.Presentation.Close(); }

                    }
                    catch (Exception ee)
                    {
                        Cursor.Current = Cursors.Default;
                        WriteLog(LogMessageType.Exception, "DisplayForm2", ee.ToString());
                    }
                    objSubtitle.Show();
                }
                if (objDisSettings.DisplayMode == DispMode.Both)
                {
                    if (officeType != null && objDisSettings.SlideType == SlideTypes.Powerpoint)
                    {
                        if (!DisplayPowerPoint(Gurmukhi, Punjabi, English))
                        { objForm.Show(); }
                    }
                    else
                    {
                        objForm.Show();
                    }

                    objSubtitle.Show();
                }
                
                Cursor.Current = Cursors.Default;
                this.BringToFront();
                */

  #endregion PowerPoint
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                WriteLog(LogMessageType.Exception, "DisplayForm2", ex.ToString());
            }
        }


        /*
        private bool DisplayPowerPoint(string gurbani, string punjabi, string english)
        {
            bool IsSuccess = true;
            string pptpath = AppDataDir + "\\PPT\\BlueWhale.pptx";
            try
            {

                app = new PowerPoint.Application();
                pres = app.Presentations;

               
               //string[] allfiles = Directory.GetFiles(pptpath, "*.pptx", SearchOption.AllDirectories);
               // foreach (var pptfile in allfiles)
               // {
               //     FileInfo info = new FileInfo(pptfile);
               //     file = pres.Open(info.FullName, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
               //     file.SaveCopyAs(pptpath + info.Name +".jpg", Microsoft.Office.Interop.PowerPoint.PpSaveAsFileType.ppSaveAsJPG, MsoTriState.msoTrue);

               // }
                

                
                if (file == null || app.SlideShowWindows.Count == 0)
                {
                    file = pres.Open(pptpath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
                    //file.SaveCopyAs(@"C:\Users\Sukhdeep.Singh1\AppData\Roaming\GurbaniApp\presentation1.jpg", Microsoft.Office.Interop.PowerPoint.PpSaveAsFileType.ppSaveAsJPG, MsoTriState.msoTrue);

                }
                //SlideShowSettings.SlideShowName  

                if (Convert.ToInt16(file.Slides[1].Shapes.Count) > 0)
                {
                    PowerPoint.Shape shape1 = file.Slides[1].Shapes[1];
                    if (shape1.HasTextFrame == MsoTriState.msoTrue)
                    {
                        shape1.TextFrame2.TextRange.Text = gurbani;
                        shape1.TextFrame2.AutoSize = MsoAutoSize.msoAutoSizeTextToFitShape;
                    }
                }
                else
                {
                    IsSuccess = false;
                }

                if (Convert.ToInt16(file.Slides[1].Shapes.Count) > 1)
                {
                    PowerPoint.Shape shape2 = file.Slides[1].Shapes[2];
                    if (shape2.HasTextFrame == MsoTriState.msoTrue)
                    {
                        shape2.TextFrame2.TextRange.Text = english;
                        shape2.TextFrame2.AutoSize = MsoAutoSize.msoAutoSizeTextToFitShape;
                    }
                }
                if (Convert.ToInt16(file.Slides[1].Shapes.Count) > 2)
                {
                    PowerPoint.Shape shape3 = file.Slides[1].Shapes[3];
                    if (shape3.HasTextFrame == MsoTriState.msoTrue)
                    {
                        shape3.TextFrame2.TextRange.Text = punjabi;
                        shape3.TextFrame2.AutoSize = MsoAutoSize.msoAutoSizeTextToFitShape;
                    }
                }
  
                file.SlideShowSettings.ShowPresenterView = MsoTriState.msoFalse;
                file.SlideShowSettings.Run();
                //file.SlideShowWindow.Activate();
                
                objForm.Hide();
                this.BringToFront();
                Cursor.Current = Cursors.Default;

            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(ex.Message.ToString());

                IsSuccess = false;

            }
            try
            {
                if (file != null && !IsSuccess)
                {
                    file.Close();
                    
                }
            }
            catch
            { }
            return IsSuccess;
        }
    */

        private void CloseSlideShow()
        {
            DisplayForm2("-1");
            objForm.Hide();
            objSubtitle.Hide();
            
            objForm.Close();
            objSubtitle.Close();

            HidePreview();
            //if (file != null)
            //{
            //    file.Close();
            //}
        }

        private void btnDown1_Click(object sender, EventArgs e)
        {
            try
            {
                if (lsbDisplay1.Items.Count > 0)
                {
                    lsbDisplay2.DataSource = (System.Data.DataTable)lsbDisplay1.DataSource;
                    lsbDisplay2.ValueMember = "VerseID";
                    lsbDisplay2.DisplayMember = "Gurmukhi";
                    lsbDisplay2.SelectedValue = lsbDisplay1.SelectedValue;
                }
                lsbDisplay1.DataSource = null;
                if (cbDisplay1.Checked == true)
                {
                    cbDisplay2.Checked = true;
                }
                //WinGetHandle("GurbaniConsole");
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "btnDown1_Click", ex.ToString());
            }

        }

        private void lsbDisplay1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbDisplay1.Checked == true)
                {
                    if (lsbDisplay1.SelectedIndex >= 0)
                    {
                        string selectedIDs = string.Empty;
                        foreach (Object selecteditem in lsbDisplay1.SelectedItems)
                        {
                            selectedIDs += ((DataRowView)selecteditem).Row[0].ToString() + ",";
                        }
                        DisplayForm2(selectedIDs);

                        int visibleItems = lsbDisplay1.ClientSize.Height / lsbDisplay1.ItemHeight;
                        if (lsbDisplay1.TopIndex + visibleItems - 2 < lsbDisplay1.SelectedIndex)
                        {
                            lsbDisplay1.TopIndex = lsbDisplay1.SelectedIndex - 1;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "lsbDisplay1_SelectedIndexChanged", ex.ToString());
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbDisplay1.Checked == true)
                {
                    if (lsbDisplay1.SelectedIndex >= 0)
                    {
                        pnlDisplay1.BackColor = System.Drawing.Color.Red;
                        string selectedIDs = string.Empty;
                        foreach (Object selecteditem in lsbDisplay1.SelectedItems)
                        {
                            selectedIDs += ((DataRowView)selecteditem).Row[0].ToString() + ",";
                            //Process(strItem);
                        }
                        DisplayForm2(selectedIDs);
                        cbDisplay2.Checked = false;
                    }
                    else if (lsbDisplay2.SelectedIndex >= 0)
                    {
                        cbDisplay1.Checked = false;
                    }
                    else
                    {
                        pnlDisplay1.BackColor = System.Drawing.Color.Red;
                        DisplayForm2("-1");
                        cbDisplay2.Checked = false;
                    }
                }
                else
                {
                    pnlDisplay1.BackColor = System.Drawing.SystemColors.ActiveCaption;
                    if (cbDisplay2.Checked == false)
                    {
                        CloseSlideShow();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "checkBox1_CheckedChanged", ex.ToString());
            }
        }

        private void btnHisClear_Click(object sender, EventArgs e)
        {
            try
            {
                lsbHistory.DataSource = null;
                dtHistory = new System.Data.DataTable();
                dtHistory.Columns.Add("VerseID", typeof(Int32));
                dtHistory.Columns.Add("Gurmukhi", typeof(string));
            }

            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "btnHisClear_Click", ex.ToString());
            }
        }

        private void lsbHistory_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                DataRow[] dra = DSMain.Tables[0].Select("VerseID = " + lsbHistory.SelectedValue.ToString());
                string ShabadID = dra[0]["ShabadID"].ToString();
                dra = DSMain.Tables[0].Select("ShabadID = " + ShabadID);
                if (dra != null && dra.Any())
                {
                    lsbDisplay1.DataSource = dra.CopyToDataTable();
                    lsbDisplay1.SelectedValue = -1;
                    lsbDisplay1.ValueMember = "VerseID";
                    lsbDisplay1.DisplayMember = "Gurmukhi";
                    lsbDisplay1.SelectedValue = lsbHistory.SelectedValue;
                }
                else
                {
                    lsbSearch.DataSource = null;
                }
                tabControl.SelectedTab = tabPageSearch;
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "lsbHistory_DoubleClick", ex.ToString());
            }
        }

        private void cbDisplay2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbDisplay2.Checked == true)
                {
                    if (lsbDisplay2.SelectedIndex >= 0)
                    {
                        pnlDisplay2.BackColor = System.Drawing.Color.Red;
                        cbDisplay1.Checked = false;
                        //DisplaySlideShow(Convert.ToInt32(lsbDisplay2.SelectedValue));
                        string selectedIDs = string.Empty;
                        foreach (Object selecteditem in lsbDisplay2.SelectedItems)
                        {
                            selectedIDs += ((DataRowView)selecteditem).Row[0].ToString() + ",";
                            //Process(strItem);
                        }
                        DisplayForm2(selectedIDs);
                    }
                    else
                    {
                        cbDisplay2.Checked = false;
                    }
                }
                else
                {
                    pnlDisplay2.BackColor = System.Drawing.SystemColors.ActiveCaption;
                    if (cbDisplay1.Checked == false)
                    {
                        CloseSlideShow();
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "cbDisplay2_CheckedChanged", ex.ToString());
            }
        }

        private void lsbDisplay2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbDisplay2.Checked == true)
                {
                    if (lsbDisplay2.SelectedIndex >= 0)
                    {
                        //DisplaySlideShow(Convert.ToInt32(lsbDisplay2.SelectedValue));
                        string selectedIDs = string.Empty;
                        foreach (Object selecteditem in lsbDisplay2.SelectedItems)
                        {
                            selectedIDs += ((DataRowView)selecteditem).Row[0].ToString() + ",";
                            //Process(strItem);
                        }
                        DisplayForm2(selectedIDs);

                        int visibleItems = lsbDisplay2.ClientSize.Height / lsbDisplay2.ItemHeight;
                        if (lsbDisplay2.TopIndex + visibleItems - 2 < lsbDisplay2.SelectedIndex)
                        {
                            lsbDisplay2.TopIndex = lsbDisplay2.SelectedIndex - 1;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "lsbDisplay2_SelectedIndexChanged", ex.ToString());
            }
        }

        private void btnClearDisplay2_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbDisplay2.Checked == true)
                {
                    cbDisplay2.Checked = false;
                }
                lsbDisplay2.DataSource = null;
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "btnClearDisplay2_Click", ex.ToString());
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            try
            {
                if (txtSearch.Text == "tweIp kro..." || txtSearch.Text == "Type here...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = System.Drawing.SystemColors.ControlText;
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "txtSearch_Enter", ex.ToString());
            }
        }

        private void cmbSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int filterIndex = Convert.ToInt32(cmbSearchType.SelectedItem);

                switch (filterIndex)
                {
                    case (int)SearchType.First_Letter_Anywhere:
                        txtSearch.Font = new System.Drawing.Font("GurbaniLipi", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        ClearSearch();
                        break;

                    case (int)SearchType.First_Letter_Start:
                        txtSearch.Font = new System.Drawing.Font("GurbaniLipi", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        ClearSearch();
                        break;

                    case (int)SearchType.First_Letter_English:
                        txtSearch.Font = new System.Drawing.Font("Arial", 23.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        ClearSearch();
                        break;

                    case (int)SearchType.Full_Word_English:
                        txtSearch.Font = new System.Drawing.Font("Arial", 23.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        ClearSearch();
                        break;

                    case (int)SearchType.Page_Number:
                        txtSearch.Font = new System.Drawing.Font("Arial", 23.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        ClearSearch();
                        break;

                    case (int)SearchType.Compiled_Banis:
                        txtSearch.Font = new System.Drawing.Font("GurbaniLipi", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        ClearSearch();
                        txtSearch.Text = "";
                        txtSearch.Enabled = false;
                        cmbFilterType.Enabled = false;
                        cmbFilterValues.Enabled = true;
                        cmbFilterValues.DataSource = Enum.GetValues(typeof(CompiledBanis));
                        cmbFilterType.SelectedItem = CompiledBanis.None;
                        break;

                    default:
                        txtSearch.Font = new System.Drawing.Font("GurbaniLipi", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        ClearSearch();
                        break;

                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "cmbSearchType_SelectedIndexChanged", ex.ToString());
            }
        }

        private void cmbFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int filterIndex = Convert.ToInt32(cmbFilterType.SelectedItem);

                switch (filterIndex)
                {
                    case (int)FilterType.Filter_By_Raag:
                        cmbFilterValues.Enabled = true;
                        cmbFilterValues.DataSource = dsSearch.Tables[0];
                        cmbFilterValues.DisplayMember = "RaagWithPage";
                        cmbFilterValues.ValueMember = "RaagID";
                        break;

                    case (int)FilterType.Filter_By_Source:
                        cmbFilterValues.Enabled = true;
                        cmbFilterValues.DataSource = dsSearch.Tables[1];
                        cmbFilterValues.DisplayMember = "SourceEnglish";
                        cmbFilterValues.ValueMember = "SourceID";
                        break;

                    case (int)FilterType.Filter_By_Writer:
                        cmbFilterValues.Enabled = true;
                        cmbFilterValues.DataSource = dsSearch.Tables[2];
                        cmbFilterValues.DisplayMember = "WriterEnglish";
                        cmbFilterValues.ValueMember = "WriterID";
                        break;

                    case (int)FilterType.None:
                        cmbFilterValues.DataSource = null;
                        cmbFilterValues.Enabled = false;
                        txtSearch.Focus();
                        break;

                    //case (int)FilterType.Filter_By_PageNumber:
                    //    cmbFilterValues.DataSource = null;
                    //    cmbFilterValues.Enabled = false;
                    //break;

                    default:
                        cmbFilterValues.DataSource = null;
                        cmbFilterValues.Enabled = false;
                        break;

                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "cmbFilterType_SelectedIndexChanged", ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {

                //objForm.Text1 =textBox2.Text.ToString();
                //objForm.Text2 = textBox1.Text.ToString();

                //Screen[] screens = Screen.AllScreens;

                //objForm.Location = Screen.AllScreens[screens.Length - 1].WorkingArea.Location;
                //objForm.Size = Screen.AllScreens[screens.Length - 1].WorkingArea.Size;
                //objForm.Show();
                //label3.Text = textBox2.Text.ToString().Length.ToString();
            }

            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "DisplayForm2", ex.ToString());
            }
        }

        private void cmbFilterValues_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int selectedIndex = Convert.ToInt32(cmbFilterValues.SelectedIndex);
                string searchString = string.Empty;
                if (DSMain != null && DSMain.Tables.Count > 0 && DSMain.Tables[0].Rows.Count > 0)
                {
                    //Asa ki var: 20757 - 21386
                    //Anand Sahib : 39128 - 39337

                    System.Data.DataTable tbl = DSMain.Tables[0];

                    if(Convert.ToInt32(cmbSearchType.SelectedIndex) == (int)SearchType.Compiled_Banis)
                    {
                        switch (selectedIndex)
                        {
                            case (int)CompiledBanis.Asa_Di_Vaar:
                                searchString = " ShabadID = 555569 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();
                                break;
                            case (int)CompiledBanis.Aarti:
                                searchString = " ShabadID = 555570 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();
                                break;

                            case (int)CompiledBanis.Anand_Sahib:
                                searchString = " VerseID >= 39128 and VerseID <= 39337 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();
                                break;

                            case (int)CompiledBanis.Japji_Sahib:
                                searchString = " VerseID >= 1 and VerseID <= 385 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();
                                break;

                            case (int)CompiledBanis.Bara_Maah_Majh_5:
                                searchString = " VerseID >= 5422 and VerseID <= 5550 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();
                                break;

                            case (int)CompiledBanis.Sukhmani_Sahib:
                                searchString = " VerseID >= 11494 and VerseID <= 13511 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();
                                break;

                            case (int)CompiledBanis.Kirtan_Sohila:
                                searchString = " ShabadID >= 49 and ShabadID <= 53 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();
                                break;

                            case (int)CompiledBanis.Laavan:
                                searchString = " ShabadID = 2897 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();
                                break;

                            case (int)CompiledBanis.Anand_Sahib_5_Pauri:
                                //searchString = "VerseID >= 39128 and VerseID <= 39337";
                                searchString = "";
                                LoadSearchCombo(Ramkali3(tbl), searchString);
                                lsbSearch.Focus();
                                break;
                            case (int)CompiledBanis.Rehras_Sahib:  

                                searchString = " ShabadID = 555568 ";
                                LoadSearchCombo(tbl, searchString);
                                lsbSearch.Focus();

                                //searchString = "";
                                //LoadSearchCombo(RehrasSahib(tbl), searchString);
                                //lsbSearch.Focus();

                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "cmbSearchType_SelectedIndexChanged", ex.ToString());
            }
        }

        private DataTable Ramkali3(DataTable tbl)
        {
            try
            {
                DataTable Dt = new DataTable();
                DataRow[] rows;

                IEnumerable<DataRow> sortedRows = new DataView(tbl, " ShabadID in (333375)", "VerseID",
                   DataViewRowState.CurrentRows).Cast<DataRowView>().Select(r => r.Row);

                if (sortedRows != null && sortedRows.Any())
                {
                    Dt = sortedRows.CopyToDataTable();
                }
                
                rows = tbl.Select(" ShabadID in (333376)", "VerseID");
                    
                foreach (DataRow row in rows)
                {

                    Dt.ImportRow(row);
                }

                rows = tbl.Select(" ShabadID in (39)", "VerseID");
                foreach (DataRow row in rows)
                {
                    Dt.ImportRow(row);
                }


                return Dt;
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "Ramkali3", ex.ToString());
                return null;
            }
        }

        private System.Data.DataTable RehrasSahib(System.Data.DataTable tbl)
        {
            try
            {
                DataTable Dt = new DataTable();
                DataRow[] rows;

                IEnumerable<DataRow> sortedRows = new DataView(tbl, " ShabadID in (40,41,42,43,44,45,46,47,48)", "VerseID",
                   DataViewRowState.CurrentRows).Cast<DataRowView>().Select(r => r.Row);

                if (sortedRows != null && sortedRows.Any())
                {
                    Dt = sortedRows.CopyToDataTable();
                }

                // 141906 // sri waheguru ji ki fateh

                
                rows = tbl.Select(" ShabadID in (12794)", "VerseID");
                int counter = 0;
                foreach (DataRow row in rows)
                {
                    if (counter < 102)
                    {
                        Dt.ImportRow(row);
                    }
                    counter++;
                }

                rows = tbl.Select(" ShabadID in (8095)", "VerseID");
                foreach (DataRow row in rows)
                {
                    Dt.ImportRow(row);
                }
                rows = tbl.Select(" ShabadID in (8096)", "VerseID");
               
                counter = 0;
                foreach (DataRow row in rows)
                {
                    if (counter < 3)
                    {
                        Dt.ImportRow(row);
                    }
                    counter++;
                }
                rows = tbl.Select(" ShabadID in (333375)", "VerseID");
                foreach (DataRow row in rows)
                {
                    Dt.ImportRow(row);
                }

                rows = tbl.Select(" ShabadID in (333376)", "VerseID");
                foreach (DataRow row in rows)
                {
                    Dt.ImportRow(row);
                }

                rows = tbl.Select(" ShabadID in (5538)", "VerseID");
                foreach (DataRow row in rows)
                {
                    Dt.ImportRow(row);
                }

                rows = tbl.Select(" ShabadID in (5539)", "VerseID");
                foreach (DataRow row in rows)
                {
                    Dt.ImportRow(row);
                }

                return Dt;
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "RehrasSahib", ex.ToString());
                return null;
            }

        }
        private void lsbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                listBox1_DoubleClick(sender, e);
            }
            if (e.KeyCode == Keys.Space)
            {
                listBox1_DoubleClick(sender, e);
            }
        }

        private void tlpMain_Paint(object sender, PaintEventArgs e)
        {

        }

        //private void ckbPreview_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (ckbPreview.Checked == true)
        //    {
        //        pnlPreview.Visible = true;



        //    }
        //    else pnlPreview.Visible = false;
        //}

        //private void ckbMultiSel2_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (ckbMultiSel2.Checked == true)
        //    {
        //        lsbDisplay2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
        //    }
        //    else
        //    {
        //        lsbDisplay2.SelectionMode = System.Windows.Forms.SelectionMode.One;
        //    }
        //}

        private void btnColorPick_Click(object sender, EventArgs e)
        {
            //ColorDialog MyDialog = new ColorDialog();
            //// Keeps the user from selecting a custom color.
            //MyDialog.AllowFullOpen = false;
            //// Allows the user to get help. (The default is false.)
            //MyDialog.ShowHelp = true;
            //// Sets the initial color select to the current text color.
            //MyDialog.Color = txtColor.BackColor;

            //// Update the text box color if the user clicks OK 
            //if (MyDialog.ShowDialog() == DialogResult.OK)
            //    txtColor.BackColor = MyDialog.Color;
        }

        private void tlpDisplaySettings_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlSettings_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //lblDateTime.Text = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
                if (Screen.AllScreens.Length >= 2)
                {
                    int i = 0;
                    i = (int)objDisSettings.ProjectorScreen >= Screen.AllScreens.Length ? Screen.AllScreens.Length - 1 : (int)objDisSettings.ProjectorScreen;
                    if (objForm.Location == Screen.AllScreens[i].WorkingArea.Location && objDisSettings.DisplayMode == DispMode.Projector)
                    {
                        objForm.Location = Screen.AllScreens[i].WorkingArea.Location;
                        objForm.Size = Screen.AllScreens[i].WorkingArea.Size;
                       // RefreshPreview();
                    }                   
                    
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "timer1_Tick", ex.ToString());
            }

        }        

        private void btnNxt1_Click(object sender, EventArgs e)
        {
            try
            {
                if (lsbDisplay1.DataSource != null && ((System.Data.DataTable)lsbDisplay1.DataSource).Rows.Count > 0)
                {

                    System.Data.DataTable dt = (System.Data.DataTable)lsbDisplay1.DataSource;
                    string ShabadID = dt.Rows[dt.Rows.Count-1]["ShabadID"].ToString();
                    Int32 ii;
                    Int32.TryParse(ShabadID, out ii);
                    ShabadID = (ii + 1).ToString();
                    DataRow[] dra;

                    dra = DSMain.Tables[0].Select("ShabadID = " + ShabadID);
                    if (dra != null && dra.Any())
                    {
                        lsbDisplay1.DataSource = dra.CopyToDataTable();
                        lsbDisplay1.ValueMember = "VerseID";
                        lsbDisplay1.DisplayMember = "Gurmukhi";
                        lsbDisplay1.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "btnNxt1_Click", ex.ToString());
            }
        }

        private void btnPrev1_Click(object sender, EventArgs e)
        {
            try
            {
                if (lsbDisplay1.DataSource != null && ((System.Data.DataTable)lsbDisplay1.DataSource).Rows.Count > 0)
                {

                    System.Data.DataTable dt = (System.Data.DataTable)lsbDisplay1.DataSource;
                    string ShabadID = dt.Rows[0]["ShabadID"].ToString();
                    Int32 ii;
                    Int32.TryParse(ShabadID, out ii);
                    ShabadID = (ii - 1).ToString();
                    DataRow[] dra;

                    dra = DSMain.Tables[0].Select("ShabadID = " + ShabadID);
                    if (dra != null && dra.Any())
                    {
                        lsbDisplay1.DataSource = dra.CopyToDataTable();
                        lsbDisplay1.ValueMember = "VerseID";
                        lsbDisplay1.DisplayMember = "Gurmukhi";
                        lsbDisplay1.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "btnPrev1_Click", ex.ToString());
            }
        }

        private void btnNext2_Click(object sender, EventArgs e)
        {
            try
            {
                if (lsbDisplay2.DataSource != null && ((System.Data.DataTable)lsbDisplay2.DataSource).Rows.Count > 0)
                {

                    System.Data.DataTable dt = (System.Data.DataTable)lsbDisplay2.DataSource;
                    string ShabadID = dt.Rows[dt.Rows.Count - 1]["ShabadID"].ToString();
                    Int32 ii;
                    Int32.TryParse(ShabadID, out ii);
                    ShabadID = (ii + 1).ToString();
                    DataRow[] dra;

                    dra = DSMain.Tables[0].Select("ShabadID = " + ShabadID);
                    if (dra != null && dra.Any())
                    {
                        lsbDisplay2.DataSource = dra.CopyToDataTable();
                        lsbDisplay2.ValueMember = "VerseID";
                        lsbDisplay2.DisplayMember = "Gurmukhi";
                        lsbDisplay2.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "btnNext2_Click", ex.ToString());
            }
        }

        private void btnPrev2_Click(object sender, EventArgs e)
        {
            try
            {
                if (lsbDisplay2.DataSource != null && ((System.Data.DataTable)lsbDisplay2.DataSource).Rows.Count > 0)
                {

                    System.Data.DataTable dt = (System.Data.DataTable)lsbDisplay2.DataSource;
                    string ShabadID = dt.Rows[0]["ShabadID"].ToString();
                    Int32 ii;
                    Int32.TryParse(ShabadID, out ii);
                    ShabadID = (ii - 1).ToString();
                    DataRow[] dra;

                    dra = DSMain.Tables[0].Select("ShabadID = " + ShabadID);
                    if (dra != null && dra.Any())
                    {
                        lsbDisplay2.DataSource = dra.CopyToDataTable();
                        lsbDisplay2.ValueMember = "VerseID";
                        lsbDisplay2.DisplayMember = "Gurmukhi";
                        lsbDisplay2.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "btnNext2_Click", ex.ToString());
            }
        }

        private void ckbPreview_CheckedChanged(object sender, EventArgs e)
        {
            //if (ckbPreview.Checked == true)
            //{
            //    this.Width = 1180;
            //    pnlPreview.Visible = true;
            //}
            //else
            //{
            //    this.Width = 470;
            //    pnlPreview.Visible = false;
            //}

            //1180, 612
        }

        private void RefreshPreview()
        {
            if (objDisSettings.PreviewEnabled)
            {
                this.Width = 1320;
                pnlPreview.Visible = true;                
            }
            else
            {
                this.Width = 925;
                pnlPreview.Visible = false;
            }
        }

        private void HidePreview()
        {
            this.Width = 925;
            pnlPreview.Visible = false;

        }

        private void FormConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SaveSettingsToXML();
                //if (file != null)
                //{
                //    file.Close();
                //}
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "GetXMLFromObject", ex.ToString());
            }
        }

        public void SaveSettingsToXML()
        {
            try
            {
                string path = AppDataDir + "/settings.xml";
                DirectoryInfo dirInfo = new DirectoryInfo(Path.GetFullPath(AppDataDir));
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }               
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(objDisSettings.GetType());
                serializer.Serialize(stringwriter, objDisSettings);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(stringwriter.ToString());
                doc.Save(path);
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "SaveSettingsToXML", ex.ToString());
            }
        }

        public static DisplaySettings LoadFromXMLString()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + @"\GurbaniApp\settings.xml";
                StringReader xmlString = new StringReader(System.IO.File.ReadAllText(path));                
                var serializer = new XmlSerializer(typeof(DisplaySettings));
                return serializer.Deserialize(xmlString) as DisplaySettings;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        private void lsbHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
               lsbHistory_DoubleClick(sender, e);
            }
            if (e.KeyCode == Keys.Space)
            {
                lsbHistory_DoubleClick(sender, e);
            }
        }

        private void lsbDisplay1_DoubleClick(object sender, EventArgs e)
        {
            if (cbDisplay1.Checked == false && lsbDisplay1.SelectedIndex >= 0)
            {
                cbDisplay1.Checked = true;
            }
        }

        private void lsbDisplay1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    lsbDisplay1_DoubleClick(sender, e);
                }
                if (e.KeyCode == Keys.Space)
                {
                    lsbDisplay1_DoubleClick(sender, e);
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "lsbDisplay1_KeyDown", ex.ToString());
            }
        }

        private void lsbDisplay2_DoubleClick(object sender, EventArgs e)
        {
            if (cbDisplay2.Checked == false)
            {
                cbDisplay2.Checked = true;
            }
        }

        private void lsbDisplay2_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    lsbDisplay2_DoubleClick(sender, e);
                }
                if (e.KeyCode == Keys.Space)
                {
                    lsbDisplay2_DoubleClick(sender, e);
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "lsbDisplay2_KeyDown", ex.ToString());
            }
        }

        private void txtKoshSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtKoshSearch.Text.ToString().Trim();
                if (searchText.Length > 1)
                {
                    string koshSearch = @" PRAGMA case_sensitive_like = true; select DictID, Word, Meaning from Dictionary where Word like '" + searchText + "%' Order by DictID COLLATE NOCASE;";
                    dsKosh = new DataSet();

                    dsKosh = GetDataBase(koshSearch);
                    if (dsKosh != null && dsKosh.Tables.Count > 0 && dsKosh.Tables[0].Rows.Count > 0)
                    {
                        lsbKoshSearch.DataSource = dsKosh.Tables[0];
                        lsbKoshSearch.ValueMember = "DictID";
                        lsbKoshSearch.DisplayMember = "Word";
                        lblKoshMeaning.Visible = false;
                        lsbKoshSearch.Visible = true;
                        lsbKoshSearch.BringToFront();
                    }
                    else
                    {
                        lsbKoshSearch.DataSource = null;
                        lsbKoshSearch.ValueMember = "DictID";
                        lsbKoshSearch.DisplayMember = "Word";
                    }

                }
                else
                {
                    lsbKoshSearch.Visible = false;
                    lsbKoshSearch.SendToBack();
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "KoshSearch", ex.ToString());
            }
        }

        private void lsbKoshSearch_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                DataRow[] dra = dsKosh.Tables[0].Select("DictID = " + lsbKoshSearch.SelectedValue.ToString());
                string KoshWord = dra[0]["Word"].ToString();
                string KoshMeaning = dra[0]["Meaning"].ToString();
                txtKoshSearch.Text = KoshWord.ToString().Trim();
                lsbKoshSearch.Visible = false;
                //txtKoshSearch.Text = "";
                lblKoshMeaning.Text = Environment.NewLine + "  " + KoshWord.ToString().Trim() + " :- " + KoshMeaning;
                lblKoshMeaning.Visible = true;
                lblKoshMeaning.BringToFront();
            }
            catch (Exception ex)
            {
                WriteLog(LogMessageType.Exception, "KoshLoad", ex.ToString());
            }
        }

        private void lsbKoshSearch_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {

        }

        private void propDisplaySettings_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //try
            //{
            //    if (cbDisplay1.Checked == true)
            //    {
            //        if (lsbDisplay1.SelectedIndex >= 0)
            //        {
            //            string selectedIDs = string.Empty;
            //            foreach (Object selecteditem in lsbDisplay1.SelectedItems)
            //            {
            //                selectedIDs += ((DataRowView)selecteditem).Row[0].ToString() + ",";
            //            }
            //            DisplayForm2(selectedIDs);


            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    WriteLog(LogMessageType.Exception, "lsbDisplay1_SelectedIndexChanged", ex.ToString());
            //}
        }
    }

    public enum SearchType
    {        
        First_Letter_Anywhere,
        First_Letter_Start,
        First_Letter_English,
        Main_Letters_Anywhere,
        Main_Letters_Start,       
        Full_Word_Gurmukhi,
        Full_Word_English,
        Page_Number,
        Compiled_Banis,
    }
    public enum FilterType
    {
        None,
        Filter_By_Source,
        Filter_By_Writer,
        Filter_By_Raag,
       // Filter_By_PageNumber,
    }
    public enum CompiledBanis
    {   
        None,
        Asa_Di_Vaar,
        Aarti,
        Anand_Sahib,
        Anand_Sahib_5_Pauri,
        Bara_Maah_Majh_5,
        Japji_Sahib,
        Kirtan_Sohila,
        Laavan,        
        Rehras_Sahib,
        Sukhmani_Sahib,
    }


}
