using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisualNovelEngine.Model.Page;

namespace VisualNovelEngine.Viewer.EditWindow
{
    public partial class EditWindow
    {
        private void ExecuteNewPage(object sender, ExecutedRoutedEventArgs e)
        {
            if (PageData.IsLatestVersion)
            {

            }
        }

        private void ExecuteDeletePage(object sender, ExecutedRoutedEventArgs e)
        {
            if (PageData.IsLatestVersion)
            {

            }
        }

        private void ExecuteNextPage(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void ExecutePrevPage(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void ExecuteImageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (PageData.IsLatestVersion)
            {
                OpenImageDialogWithInitialPath();
                SetBackgroundImage(OpenImageDialogWithInitialPath());
            }
        }

        private void ExecuteMusicCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (PageData.IsLatestVersion)
            {
                OpenMusicDialogWithInitialPath();
                SetBackgroundMusic(OpenMusicDialogWithInitialPath());
            }
        }

        private void ExecuteVersionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PageData.IsLatestVersion = !PageData.IsLatestVersion;
        }
    }
}
