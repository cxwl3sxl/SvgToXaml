using System;
using System.ComponentModel;
using SvgConverter;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using Path = System.IO.Path;

namespace Svg2Xaml
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ConvertedSvgData _convertedSvgData;
        private string _currentTempSvgFile;

        void DeleteTempFile()
        {
            if (!string.IsNullOrEmpty(_currentTempSvgFile) && File.Exists(_currentTempSvgFile))
            {
                File.Delete(_currentTempSvgFile);
            }
        }

        private void PasteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetText();
            if (string.IsNullOrEmpty(text)) return;

            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(text);
            }
            catch
            {
                return;
            }

            if (text.StartsWith("<svg"))
            {
                text =
                    $"<?xml version=\"1.0\" standalone=\"no\" ?><!DOCTYPE svg PUBLIC \" -//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\" >{text}";
            }
            try
            {
                xml.LoadXml(text);
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = new UTF8Encoding(false),
                    NewLineChars = Environment.NewLine
                };
                var sb = new StringBuilder();
                var xw = XmlWriter.Create(sb, settings);
                xml.Save(xw);
                xw.Close();

                SvgView.Text = sb.ToString();
            }
            catch
            {
                return;
            }

            DeleteTempFile();
            _currentTempSvgFile = $"{Path.GetTempFileName()}.svg";
            File.WriteAllText(_currentTempSvgFile, text);
            try
            {
                _convertedSvgData = ConverterLogic.ConvertSvg(_currentTempSvgFile, ResultMode.DrawingImage);
                ViewImage.Source = _convertedSvgData.ConvertedObj as ImageSource;
                XamlView.Text = _convertedSvgData.Xaml;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
        }

        private void CopyButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(XamlView.Text);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            DeleteTempFile();
        }
    }
}
