using System.Windows;
using System.Windows.Input;
using MoonspaceLabs.Shared.BusinessLogic;

namespace MD2WP.TestUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Attributes

        MD2WPClient _client;
        Settings _settings;

        #endregion

        #region Constructor(s)

        public MainWindow()
        {
            InitializeComponent();

            LoadSettings();
        }

        #endregion

        #region Event Handlers

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            _client = new MD2WPClient(
                SiteUrlTextBox.Text,
                UserNameTextBox.Text,
                PasswordTextBox.Password,
                VSTSAccountTextBox.Text,
                ProjectNameTextBox.Text,
                RepoNameTextBox.Text,
                BranchTextBox.Text,
                PATTextBox.Password,
                MetadataFileTextBox.Text,
                EmbedExternalImagesCheckBox.IsChecked.Value,
                PublishAsCommitterCheckBox.IsChecked.Value,
                true,
                false,
                false,
                PublishNewPostsAsDraftCheckBox.IsChecked.Value,
                TrackPostIdInFilename.IsChecked.Value,
                AddEditLink.IsChecked.Value,
                EditLinkText.Text,
                EditLinkStyle.Text);

            _client.PublishMarkdownFiles();

            this.Cursor = Cursors.Arrow;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            SaveSettings();
        }

        #endregion

        #region Methods

        private void LoadSettings()
        {
            _settings = Settings.GetSettings(PassKeyTextBox.Password);

            SiteUrlTextBox.Text = _settings.SiteUrl ?? "http://myblog.wordpress.com";
            UserNameTextBox.Text = _settings.UserName ?? string.Empty;
            PasswordTextBox.Password = _settings.Password ?? string.Empty;
            VSTSAccountTextBox.Text = _settings.VSTSAccount ?? string.Empty;
            ProjectNameTextBox.Text = _settings.ProjectName ?? string.Empty;
            RepoNameTextBox.Text = _settings.RepoName ?? string.Empty;
            BranchTextBox.Text = _settings.Branch ?? "master";
            PATTextBox.Password = _settings.PAT ?? string.Empty;
            MetadataFileTextBox.Text = _settings.MetadataFile ?? "/_metadata.json";
            EmbedExternalImagesCheckBox.IsChecked = _settings.EmbedExternalImages;
        }

        private void SaveSettings()
        {
            _settings.SiteUrl = SiteUrlTextBox.Text;
            _settings.UserName = UserNameTextBox.Text;
            _settings.Password = PasswordTextBox.Password;
            _settings.VSTSAccount = VSTSAccountTextBox.Text;
            _settings.ProjectName = ProjectNameTextBox.Text;
            _settings.RepoName = RepoNameTextBox.Text;
            _settings.Branch = BranchTextBox.Text;
            _settings.PAT = PATTextBox.Password;
            _settings.MetadataFile = MetadataFileTextBox.Text;
            _settings.EmbedExternalImages = EmbedExternalImagesCheckBox.IsChecked.Value;

            _settings.SaveSettings(PassKeyTextBox.Password);
        }

        #endregion
    }
}