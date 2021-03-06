﻿
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ActivateUsersModule.Views
{
    [Export]
    [ViewSortHint("04")]
    public partial class ActivateUsersNavigationItemView : UserControl, IPartImportsSatisfiedNotification, IDiagnoseAllService
    {
        private static Uri ViewUri = new Uri("/ActivateUsersView", UriKind.Relative);

        [Import]
        public IRegionManager regionManager;

        public ActivateUsersNavigationItemView()
        {
            InitializeComponent();
        }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            IRegion mainContentRegion = this.regionManager.Regions[RegionNames.MainContentRegion];
            if (mainContentRegion != null && mainContentRegion.NavigationService != null)
            {
                mainContentRegion.NavigationService.Navigated += this.MainContentRegion_Navigated;
            }
        }
        public void MainContentRegion_Navigated(object sender, RegionNavigationEventArgs e)
        {
            this.UpdateNavigationButtonState(e.Uri);



        }
        private void UpdateNavigationButtonState(Uri uri)
        {
            this.navigateRadioButton.IsChecked = (uri == ViewUri);
            Uri ImageUri = (this.navigateRadioButton.IsChecked == true ? new Uri("../Images/active_radio.png", UriKind.Relative) : new Uri("../Images/in_active_radio.png", UriKind.Relative));
            this.navigateImage.Source = new BitmapImage(ImageUri);

        }



        private void navigateRadioButton_Click(object sender, RoutedEventArgs e)
        {
            this.regionManager.RequestNavigate
                   (
                    RegionNames.MainContentRegion,
                    ViewUri,
                    (NavigationResult nr) =>
                    {
                        var error = nr.Error;
                        var result = nr.Result;

                        // put a breakpoint here and checkout what NavigationResult contains
                    }
                   );
        }

        private void navigateRadioButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Uri ImageUri = new Uri("../Images/active_radio.png", UriKind.Relative);

            this.navigateImage.Source = new BitmapImage(ImageUri);
            ShortTextBlock.Foreground = (Brush)Application.Current.MainWindow.FindResource("BrushShortDiscriptionIn");

        }

        private void navigateRadioButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ShortTextBlock.Foreground = (Brush)Application.Current.MainWindow.FindResource("BrushShortDiscriptionOut");

            Uri ImageUri = (this.navigateRadioButton.IsChecked == true ? new Uri("../Images/active_radio.png", UriKind.Relative) : new Uri("../Images/In_active_radio.png", UriKind.Relative));
            this.navigateImage.Source = new BitmapImage(ImageUri);

        }
        public string getHeaderTitle()
        {
            return Constants.TEST_HEADER_TITTLE_ACTIVE_USERS;
        }
        public string getShortDescription()
        {
            return Constants.TEST_HEADER_SHORT_DESCRIPTION_ACTIVE_USERS;
        }
         
        public Image getImageIcone()
        {
            Uri ImageUri = new Uri("/ActivateUsersModule;component/Resources/active_user.png", UriKind.Relative);
            Image image = new Image();
            image.Source = new BitmapImage(ImageUri);
            return image;
        }

        public Uri getViewURI()
        {
            return ViewUri;
        }
    }
}