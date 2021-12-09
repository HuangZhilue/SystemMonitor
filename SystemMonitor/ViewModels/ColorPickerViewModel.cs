using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;
using System.Windows.Media;

namespace SystemMonitor.ViewModels
{
    public class ColorPickerViewModel : BindableBase, IDialogAware
    {
        private string _title = "选择颜色";
        private SolidColorBrush selectedBrush = Brushes.White;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public SolidColorBrush SelectedBrush
        {
            get => selectedBrush;
            set => SetProperty(ref selectedBrush, value);
        }

        public DelegateCommand<string> CloseDialogCommand { get; set; }
        public DelegateCommand<SolidColorBrush> ConfirmedCommand { get; set; }

        public ColorPickerViewModel()
        {
            CloseDialogCommand = new DelegateCommand<string>(CloseDialog);
            ConfirmedCommand = new DelegateCommand<SolidColorBrush>(Confirmed);
        }

        private void Confirmed(SolidColorBrush obj)
        {
            if(obj is not null)
            {
                SelectedBrush = obj;
                CloseDialog(bool.TrueString);
            }
        }

        #region 关闭弹窗

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;
            IDialogParameters parameters = new DialogParameters();

            switch (parameter?.ToLower())
            {
                case "true":
                    result = ButtonResult.OK;
                    parameters.Add(nameof(SelectedBrush), SelectedBrush);
                    break;
                case "false":
                    result = ButtonResult.Cancel;
                    break;
                default:
                    break;
            }

            RaiseRequestClose(new DialogResult(result, parameters));
        }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters?.Count > 0 && parameters.ContainsKey(nameof(SolidColorBrush)))
            {
                SelectedBrush = parameters.GetValue<SolidColorBrush>(nameof(SolidColorBrush));
            }
            //Message = parameters.GetValue<string>("message");
            Debug.WriteLine(parameters);
        }

        #endregion
    }
}