using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GurbaniDesktopApp
{

   

    public partial class FormDisplay : Form
    {
        public Int32 count = 0;

        private string text1;
        private string text2;
        private string text3;
        private string templatePath;
        private bool _isSubtitle;

        private string txtDetails;
        private DispMode _displayMode;
        private Color _displayBackColor;
        private Color _borderColor;

        private float _maxGurbaniFontSize;
        private float _maxPunjabiFontSize;
        private float _maxEnglishFontSize;

        public FormDisplay()
        {
            InitializeComponent();
            count++;
        }

        public FormDisplay(DisplaySettings objDS)
        {
            try
            {
                InitializeComponent();
                DisplayMode = objDS.DisplayMode;
                GurbaniColor = objDS.GurbaniColor;
                PunjabiColor = objDS.PunjabiTranslationColor;
                EnglishColor = objDS.EnglishTranslationColor;
                MaxGurbaniFontSize = objDS.MaxGurbaniFontSize;
                MaxPunjabiFontSize = objDS.MaxPunjabiFontSize;
                MaxEnglishFontSize = objDS.MaxEnglishFontSize;
                GurbaniFontName = objDS.GurbaniFontName;
                EnglishFontName = objDS.EnglishFontName;
                DisplayBackColor = objDS.DisplayBackColor;
                BorderColor = objDS.BorderColor;
                TemplatePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + @"\GurbaniApp\Templates\" + objDS.TemplateName.ToString() + ".jpg";
                if (objDS.TemplateName == BGTemplates.NONE)
                {
                    lblTopBorder.Visible = true;
                    lblDownBorder.Visible = true;
                }
                else
                {
                    lblTopBorder.Visible = false;
                    lblDownBorder.Visible = false;
                }


            }
            catch (Exception ex)
            {

            }
        }

        public bool IsSubtitle 
        {
            get
            {
                return this._isSubtitle;
            }
            set
            {
                this._isSubtitle = value;
            }
        }
        public string TemplatePath
        {
            get
            {
                return this.templatePath;
            }
            set
            {
                if (this.templatePath != value)
                {
                    this.templatePath = value;
                    SetBackground();
                }
            }
        }

        public string Text1
        {
            get
            {
                return this.text1;
            }
            set
            {
                this.text1 = value;
                label1.Text = text1;
                //label4.Text = text1;
            }
        }
        public string Text2
        {
            get
            {
                return this.text2;
            }
            set
            {
                this.text2 = value;
                //label2.Text = text2;
            }
        }
        public string Text3
        {
            get
            {
                return this.text3;
            }
            set
            {
                this.text3 = value;
                label3.Text = text3;
            }
        }

        public string DetailsText
        {
            get
            {
                return this.txtDetails;
            }
            set
            {
                this.txtDetails = value;
                lblDetails.Text = txtDetails;
            }
        }

        public DispMode DisplayMode
        {
            get
            {
                return this._displayMode;
            }
            set
            {
                //if (value == DispMode.TVSubTitles && this._displayMode != value)
                //{
                //    MaxGurbaniFontSize = 32;
                //    MaxEnglishFontSize = 30;
                //    tlpDisplay.BackColor = DisplayBackColor;
                //    this.BackColor = DisplayBackColor;
                //    GurbaniColor = Color.White;
                //    EnglishColor = Color.White;
                //}
                //else if (value == DispMode.Projector && this._displayMode != value)
                //{
                //    MaxGurbaniFontSize = 72;
                //    MaxEnglishFontSize = 52;
                //    tlpDisplay.BackColor = DisplayBackColor;
                //    this.BackColor = DisplayBackColor;
                //    GurbaniColor = Color.Maroon;
                //    EnglishColor = Color.Navy;

                //}
                this._displayMode = value;
            }
        }

        public Color DisplayBackColor
        {
            get
            {
                return this._displayBackColor;
            }
            set
            {
                this._displayBackColor = value;
                tlpDisplay.BackColor = _displayBackColor;
                //this.BackColor = _displayBackColor;
            }
        }

        public Color BorderColor
        {
            get
            {
                return this._borderColor;
            }
            set
            {
                this._displayBackColor = value;
                lblDownBorder.BackColor = value;
                lblTopBorder.BackColor = value;
            }
        }

        [Description("Maximum font size for Gurbani on the slide")]
        public float MaxGurbaniFontSize
        {
            get { return _maxGurbaniFontSize; }
            set { _maxGurbaniFontSize = value; }
        }

        [Description("Maximum font size for Punjabi Translation on the slide")]
        public float MaxPunjabiFontSize
        {
            get { return _maxPunjabiFontSize; }
            set { _maxPunjabiFontSize = value; }
        }

        [Description("Maximum font size for English Translation on the slide")]
        public float MaxEnglishFontSize
        {
            get { return _maxEnglishFontSize; }
            set { _maxEnglishFontSize = value; }
        }

        public Color GurbaniColor { get; set; }
        public Color PunjabiColor { get; set; }
        public Color EnglishColor { get; set; }
        public GurbaniFonts GurbaniFontName { get; set; }

        public EnglishFonts EnglishFontName { get; set; }
        public TransLanguage TranslationLanguage { get; set; }

        private void Form2_Load(object sender, EventArgs e)
         {
            label1.Font = new System.Drawing.Font(GurbaniFontName.ToString(), MaxGurbaniFontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label2.Font = new System.Drawing.Font("GurbaniLipi", MaxPunjabiFontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //label3.Font = new System.Drawing.Font(EnglishFontName.ToString(), MaxEnglishFontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //lblTopBorder.BackColor = BorderColor;
            //lblDownBorder.BackColor = BorderColor;

            if (this.TranslationLanguage == TransLanguage.English)
            {
                label3.Font = new System.Drawing.Font(EnglishFontName.ToString(), MaxEnglishFontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
            else // (this.TranslationLanguage == TransLanguage.Punjabi)
            {
                label3.Font = new System.Drawing.Font("GurbaniAkharHeavy", MaxEnglishFontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }

            
            label1.Text = Text1;
            label2.Text = Text2;
            label3.Text = Text3;
            this.label1_TextChanged(label1, e);
            this.label3_TextChanged(label3, e);
            SetBackground();
        }

       

        private void label1_TextChanged(object sender, EventArgs e)
        {
            label1.ForeColor = GurbaniColor;
            label1.Font = new System.Drawing.Font(GurbaniFontName.ToString(), MaxGurbaniFontSize, label1.Font.Style);
            Size sz = TextRenderer.MeasureText(label1.Text, label1.Font, label1.Size, TextFormatFlags.WordBreak);
            while (sz.Width > label1.Size.Width || sz.Height > label1.Size.Height)
            {
                DecreaseFontSize(label1);
                sz = TextRenderer.MeasureText(label1.Text, label1.Font, label1.Size, TextFormatFlags.WordBreak);
            }
        }

       

        private void SetBackground()
        {
            if (!this.IsSubtitle)
            {
                try
                {                 
                    this.tlpDisplay.BackgroundImage = Image.FromFile(templatePath);
                   this.tlpDisplay.BackgroundImageLayout = ImageLayout.Stretch;
                    lblTopBorder.Visible = false;
                    lblDownBorder.Visible = false;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                    this.tlpDisplay.BackgroundImage = null;
                    lblTopBorder.Visible = true;
                    lblDownBorder.Visible = true;
                }
            }
            else
            {
                this.tlpDisplay.BackgroundImage = null;
                this.BackColor = this.DisplayBackColor;
                //this.tlpDisplay.BackColor = Color.Transparent;
                //this.label1.ForeColor = Color.Red;
                //this.label1.BackColor = Color.Transparent;
                //this.pnlMain.BackColor = Color.Transparent;
            }
        }

        public void DecreaseFontSize(Label lbl)
        {
            try
            {
                float CurrentSize = lbl.Font.Size;
                float step = 2f;
                if (CurrentSize >= 52)
                    step = 2f;
                if (CurrentSize >= 40 && CurrentSize < 52)
                    step = 2f;
                if (CurrentSize >= 28 && CurrentSize < 40)
                    step = 1f;
                if (CurrentSize < 28)
                    step = 0.5f;
                lbl.Font = new System.Drawing.Font(lbl.Font.Name, lbl.Font.Size - step, lbl.Font.Style);
            }
            catch
            {
                lbl.Text = "";
            }
        }

        public void DecreaseFontSize2(Label lbl)
        {
            try
            {
                float CurrentSize = lbl.Font.Size;
                float step = 4f;
                if (CurrentSize >= 40)
                    step = 4f;
                if (CurrentSize >= 28 && CurrentSize < 52)
                    step = 2f;
                if (CurrentSize < 28)
                    step = 1f;
                lbl.Font = new System.Drawing.Font(lbl.Font.Name, lbl.Font.Size - step, lbl.Font.Style);
            }
            catch
            {
                lbl.Text = "";
            }
        }

        private void label3_TextChanged(object sender, EventArgs e)
        {
            if (this.TranslationLanguage == TransLanguage.English)
            {
                label3.Font = new System.Drawing.Font(EnglishFontName.ToString(), MaxEnglishFontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
            else if (this.TranslationLanguage == TransLanguage.Punjabi)
            {
                label3.Font = new System.Drawing.Font("GurbaniAkharHeavy", MaxEnglishFontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
            label3.ForeColor = EnglishColor;
            Size sz = TextRenderer.MeasureText(label3.Text, label3.Font, label3.Size, TextFormatFlags.WordBreak);
            while (sz.Width > label3.Size.Width || sz.Height > label3.Size.Height)
            {
                DecreaseFontSize2(label3);
                sz = TextRenderer.MeasureText(label3.Text, label3.Font, label3.Size, TextFormatFlags.WordBreak);
            }
        }

        

      
        private void lblDetails_TextChanged(object sender, EventArgs e)
        {
            // Size sz = TextRenderer.MeasureText(lblDetails.Text, lblDetails.Font, lblDetails.Size, TextFormatFlags.WordBreak);
            //while (sz.Width > lblDetails.Size.Width || sz.Height > lblDetails.Size.Height)
            //{
            //    DecreaseFontSize(lblDetails);
            //    sz = TextRenderer.MeasureText(lblDetails.Text, lblDetails.Font, lblDetails.Size, TextFormatFlags.WordBreak);
            //}
        }

        private void lblDetails_Paint(object sender, PaintEventArgs e)
        {
            Size sz = TextRenderer.MeasureText(lblDetails.Text, lblDetails.Font, lblDetails.Size, TextFormatFlags.WordBreak);
            while (sz.Width > lblDetails.Size.Width || sz.Height > lblDetails.Size.Height)
            {
                DecreaseFontSize(lblDetails);
                sz = TextRenderer.MeasureText(lblDetails.Text, lblDetails.Font, lblDetails.Size, TextFormatFlags.WordBreak);
            }
        }

       

     
    }
}
