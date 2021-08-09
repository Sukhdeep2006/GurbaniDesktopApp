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

namespace GurbaniDesktopApp
{
    public class DisplaySettings
    {
        public DisplaySettings()
        {
            this.DisplayMode = DispMode.Projector;
            this.DisplayEnglishTranslation = true;
            this.DisplayPunjabiTranslation = false;
            this.GurbaniColor = Color.Maroon;
            this.BorderColor = Color.Maroon;
            this.EnglishTranslationColor = Color.Navy;
            this.PunjabiTranslationColor = Color.Navy;
            this.TranslationLanguage = TransLanguage.English;
            this.MaxEnglishFontSize = 44f;
            this.MaxGurbaniFontSize = 58f;
            this.MaxPunjabiFontSize = 44f;
            this.SubTitleOpacity = 10;
            this.GurbaniFontName = GurbaniFonts.GurbaniAkharHeavy;
            this.EnglishFontName = EnglishFonts.Calibri;            
            this.DisplayBackColor = Color.Transparent;
            this.SlideType = SlideTypes.Default;
            this.AppDirectory = "";
            this.TemplateName = BGTemplates.Template0;
            this.PreviewEnabled = false;
            this.ProjectorScreen = (ScreenNumber)Screen.AllScreens.Length-1;
            
        }

        private bool _punjabiTranslation;
        private bool _englishTranslation;
        private Color _punjabiColor;
        private Color _englishColor;
        private Color _gurbaniColor;
        private Color _borderColor;
        private GurbaniFonts _gurbaniFontName;
        private EnglishFonts _englishFontName;
        private float _maxGurbaniFontSize;
        private float _maxPunjabiFontSize;
        private float _maxEnglishFontSize;
        private Color _displayBackColor;
        private DispMode _disMode;
        private SlideTypes _slideType;
        private bool _previewEnabled;
        private TransLanguage _translationLanguage;
        private string _appDir;
        private string filePath;
        private BGTemplates _templateName;
        private float _subtitleOpacity;
        private ScreenNumber _projectorScreen;
       

        [Description("Select the mode for the Display Screen.")]//, ReadOnly(true)]
        public DispMode DisplayMode
        {
            get { return _disMode; }
            set
            {
                _disMode = value;
                if (_disMode == DispMode.TVSubTitles)
                {
                    //MaxGurbaniFontSize = 32f;
                    //MaxEnglishFontSize = 30f;
                    //DisplayBackColor = Color.Navy;
                    //GurbaniColor = Color.White;
                    //EnglishTranslationColor = Color.White;
                    PreviewEnabled = false;
                }
                if (_disMode == DispMode.Projector)
                {
                    //MaxGurbaniFontSize = 72f;
                    //MaxEnglishFontSize = 52f;
                    //DisplayBackColor = Color.White;
                    //GurbaniColor = Color.Maroon;
                    //EnglishTranslationColor = Color.Navy;
                    PreviewEnabled = true;
                }
                if (_disMode == DispMode.Both)
                {
                    //MaxGurbaniFontSize = 72f;
                    //MaxEnglishFontSize = 52f;
                    //DisplayBackColor = Color.White;
                    //GurbaniColor = Color.Maroon;
                    //EnglishTranslationColor = Color.Navy;
                    PreviewEnabled = false;
                }
            }
        }

        [Description("Select the Screen Number for Gurbani Projector Display.")]//, ReadOnly(true)]
        public ScreenNumber ProjectorScreen
        {
            get { return _projectorScreen; }
            set
            {
                _projectorScreen = value;
            }

        }
        
                [Description("Set to control the Subtitle window opacity (1-10).")]
        public float SubTitleOpacity
        {
            get { return _subtitleOpacity; }
            set { _subtitleOpacity = (value < 1 ? 1 : value) > 10 ? 10 : (value < 1 ? 1 : value); }

        }


        [Description("Select the presentation type. Pwerpoint or Default"), Browsable(false)]
        public SlideTypes SlideType
        {
            get { return _slideType; }
            set
            {
                _slideType = value;                
            }
        }

        [Description("Select the translation language to be displayed on the Slide.")]
        public TransLanguage TranslationLanguage
        {
            get { return _translationLanguage; }
            set { _translationLanguage = value; }
        }


        [Description("Select the Backgroung color for the display Slide.")]
        public Color DisplayBackColor
        {
            get { return _displayBackColor; }
            set { _displayBackColor = value; }
        }

        [Description("Gurbani on the Slide will be displayed in Selected Color")]
        public Color GurbaniColor
        {
            get { return _gurbaniColor; }
            set { _gurbaniColor = value; }
        }

        [Description("English translation on the Slide will be displayed in Selected Color")]
        public Color EnglishTranslationColor
        {
            get { return _englishColor; }
            set { _englishColor = value; }
        }

        [Description("Display color for the top and bottom border bar.")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        [Description("Maximum font size for Gurbani on the slide")]
        public float MaxGurbaniFontSize
        {
            get { return _maxGurbaniFontSize; }
            set { _maxGurbaniFontSize = (value < 10 ? 10 : value) > 100 ? 100 : (value < 10 ? 10 : value); ; }
        }

        [Description("Maximum font size for English Translation on the slide")]
        public float MaxEnglishFontSize
        {
            get { return _maxEnglishFontSize; }
            set { _maxEnglishFontSize = (value < 10 ? 10 : value) > 100 ? 100 : (value < 10 ? 10 : value); }
        }
        

        [Description("Select font for Gurbani Display.")]
        public GurbaniFonts GurbaniFontName
        {
            get { return _gurbaniFontName; }
            set { _gurbaniFontName = value; }
        }
               
        [Description("Select font for English Translation.")]
        public EnglishFonts EnglishFontName
        {
            get { return _englishFontName; }
            set { _englishFontName = value; }
        }

        [Description("Set to true for enabling the priview of the Projector Slide.")]
        public bool PreviewEnabled
        {
            get { return _previewEnabled; }
            set { _previewEnabled = value; }
        }
                
        [Description("Switch On/Off the English translation on the Slide."), ReadOnly(true), Browsable(false)]
        public bool DisplayEnglishTranslation
        {
            get { return _englishTranslation; }
            set { _englishTranslation = value; }
        }

        [Description("Switch On/Off the punjabi translation on the Slide."), ReadOnly(true), Browsable(false)]
        public bool DisplayPunjabiTranslation
        {
            get { return _punjabiTranslation; }
            set { _punjabiTranslation = value; }
        }

        [Description("Punjabi translation on the Slide will be displayed in Selected Color"), ReadOnly(true), Browsable(false)]
        public Color PunjabiTranslationColor
        {
            get { return _punjabiColor; }
            set { _punjabiColor = value; }
        }

        [Description("Maximum font size for Punjabi Translation on the slide"), ReadOnly(true), Browsable(false)]
        public float MaxPunjabiFontSize
        {
            get { return _maxPunjabiFontSize; }
            set { _maxPunjabiFontSize = (value < 10 ? 10 : value) > 100 ? 100 : (value < 10 ? 10 : value); }
        }

        [Browsable(false)]
        public string BorderColorXML
        {
            get
            {
                return SerializeColor(BorderColor);
            }
            set
            {
                BorderColor = this.DeserializeColor(value);
            }
        }

        [Browsable(false)]
        public string GrubaniColorXML
        {
            get
            {
                return SerializeColor(GurbaniColor);
            }
            set
            {
                GurbaniColor = this.DeserializeColor(value);
            }
        }

        [Browsable(false)]
        public string DisplayBackColorXML
        {
            get
            {
                return SerializeColor(DisplayBackColor);
            }
            set
            {
                DisplayBackColor = this.DeserializeColor(value);
            }
        }

        [Browsable(false)]
        public string EnglishColorXML
        {
            get
            {
                return SerializeColor(EnglishTranslationColor);
            }
            set
            {
                EnglishTranslationColor = this.DeserializeColor(value);
            }
        }

        [Browsable(false)]
        public string PunjabiColorXML
        {
            get
            {
                return SerializeColor(PunjabiTranslationColor);
            }
            set
            {
                PunjabiTranslationColor = this.DeserializeColor(value);
            }
        }
        
        public Color DeserializeColor(string color)
        {
            byte a, r, g, b;

            string[] pieces = color.Split(new char[] { ':' });

            ColorFormat colorType = (ColorFormat)
                Enum.Parse(typeof(ColorFormat), pieces[0], true);

            switch (colorType)
            {
                case ColorFormat.NamedColor:
                    return Color.FromName(pieces[1]);

                case ColorFormat.ARGBColor:
                    a = byte.Parse(pieces[1]);
                    r = byte.Parse(pieces[2]);
                    g = byte.Parse(pieces[3]);
                    b = byte.Parse(pieces[4]);

                    return Color.FromArgb(a, r, g, b);
            }
            return Color.Empty;
        }
        public string SerializeColor(Color color)
        {
            if (color.IsNamedColor)
                return string.Format("{0}:{1}",
                    ColorFormat.NamedColor, color.Name);
            else
                return string.Format("{0}:{1}:{2}:{3}:{4}",
                    ColorFormat.ARGBColor,
                    color.A, color.R, color.G, color.B);
        }

        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Description("Set the Projector Background Template to be used from the App Directory.")]//, ReadOnly(true)]
        public BGTemplates TemplateName
        {
            get { return this._templateName; }
            set { this._templateName = value; }

        }

        [Description("Application Directory for DB, Logs and BG Templates"), ReadOnly(true)]
        public string AppDirectory
        {
            get { return _appDir; }
           set { _appDir = value; }
        }


    }
    

    public enum ColorFormat
    {
        NamedColor,
        ARGBColor
    }

    public enum ScreenNumber
    {
       Screen1 = 0,
       Screen2 = 1,
       Screen3 = 2,
       Screen4 = 3,
       Screen5 = 4 
    }

    public enum DispMode
    {
        Projector,
        TVSubTitles,
        Both,       
    }

    public enum SlideTypes
    {
        Powerpoint,
        Default,
    }

    public enum BGTemplates
    {
        NONE, Template0, Template1, Template2, Template3, Template4,
        Template5, Template6, Template7, Template8, Template9,
    }

    public enum GurbaniFonts
    {
        GurbaniAkharHeavy,
        GurbaniAkharSlim,
        GurbaniKalmi,
        GurbaniLipi,
        GurbaniRaised,
        GurbaniUbhri,
        GurbaniWebThick,
        GurbaniHindi,
    }

    public enum EnglishFonts
    {
        [Description("Arial")]
        Arial,
        [Description("Calibri")]
        Calibri,
        [Description("Century")]
        Century,
        [Description("Gisha")]
        Gisha,
        [Description("Verdana")]
        Verdana,
    }

    public enum TransLanguage
    {
        None,
        English,
        Punjabi,       
    }



}