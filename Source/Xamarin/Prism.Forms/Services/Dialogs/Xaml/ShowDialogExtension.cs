﻿using System;
using System.Windows.Input;
using Prism.Ioc;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Prism.Services.Dialogs.Xaml
{
    [ContentProperty(nameof(Name))]
    public class ShowDialogExtension : ICommand, IMarkupExtension<ICommand>
    {
        public static Lazy<IDialogService> LazyDialogService = new Lazy<IDialogService>(() =>
        {
            return PrismApplicationBase.Current.Container.Resolve<IDialogService>();
        });

        public string Name { get; set; }

        public bool IsExecuting { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            !IsExecuting;

        public void Execute(object parameter)
        {
            IsExecuting = true;
            CanExecuteChanged(this, EventArgs.Empty);

            try
            {
                var parameters = parameter.ToNavigationParameters();
                var dialogService = LazyDialogService.Value;
                dialogService.ShowDialog(Name, parameters, DialogClosedCallback);
            }
            catch (Exception ex)
            {
                Log.Warning("Error", $"An unexpected error occurred while showing the Dialog '{Name}'.\n{ex}");
            }
        }

        private void DialogClosedCallback(IDialogResult result)
        {
            OnDialogClosed(result);

            IsExecuting = false;
            CanExecuteChanged(this, EventArgs.Empty);
        }

        protected virtual void OnDialogClosed(IDialogResult result)
        {
            if (!result.Success)
            {
                Log.Warning("Warning", $"Dialog '{Name}' closed with an error:\n{result.Exception}");
            }
        }

        public ICommand ProvideValue(IServiceProvider serviceProvider) =>
            this;

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
            ProvideValue(serviceProvider);
    }
}
