// EditWindow.Keybinds.cs
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace VisualNovelEngine.Viewer.EditWindow
{
    public partial class EditWindow
    {
        private static readonly RoutedUICommand[] _commands =
        {
            new RoutedUICommand("New", "New", typeof(EditWindow),
                new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) }),

            new RoutedUICommand("Delete", "Delete", typeof(EditWindow),
                new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) }),

            new RoutedUICommand("Next", "Next", typeof(EditWindow),
                new InputGestureCollection { new KeyGesture(Key.Right), new KeyGesture(Key.Space) }),

            new RoutedUICommand("Prev", "Prev", typeof(EditWindow),
                new InputGestureCollection { new KeyGesture(Key.Left) }),

            new RoutedUICommand("Version", "Version", typeof(EditWindow),
                new InputGestureCollection { new KeyGesture(Key.F12) }),

            new RoutedUICommand("Image", "Image", typeof(EditWindow),
                new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) }),

            new RoutedUICommand("Music", "Music", typeof(EditWindow),
                new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Control) }),

        };



        private void CanExecutePageCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(e.Command == _commands[2] || e.Command == _commands[3])
                           || !IsTextFocused();
            e.Handled = true;
        }

        private bool IsTextFocused() =>
            DialogueText.IsKeyboardFocusWithin || DialogueText.IsFocused;
    }
}
