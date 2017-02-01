﻿using Microsoft.Templates.Core;
using Microsoft.Templates.Wizard.Dialog;
using Microsoft.Templates.Wizard.Host;
using Microsoft.Templates.Wizard.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Templates.Wizard.Steps.PagesStep
{
    public class PagesTemplatesDialogViewModel : ObservableBase
    {
        private bool _isValid = true;

        private readonly WizardContext _context;
        private readonly PagesTemplatesDialog _dialog;
        private readonly IEnumerable<string> _selectedNames;

        public PagesTemplatesDialogViewModel(WizardContext context, PagesTemplatesDialog dialog, IEnumerable<string> selectedNames)
        {
            _context = context;
            _dialog = dialog;
            _selectedNames = selectedNames;
        }

        public ICommand OkCommand => new RelayCommand(SaveAndClose, IsValid);
        public ICommand CancelCommand => new RelayCommand(_dialog.Close);

        public ObservableCollection<TemplateViewModel> Templates { get; } = new ObservableCollection<TemplateViewModel>();

        private TemplateViewModel _templateSelected;
        public TemplateViewModel TemplateSelected
        {
            get { return _templateSelected; }
            set
            {
                SetProperty(ref _templateSelected, value);
                if (value != null)
                {
                    ItemName = Naming.Infer(_selectedNames, value.Name);
                }
            }
        }

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                SetProperty(ref _itemName, value);

                var validationResult = Naming.Validate(_selectedNames, value);

                HandleValidation(validationResult);

                OnPropertyChanged("OkCommand");
            }
        }

         //TODO: MAKE THIS METHOD TRULY ASYNC
        public async Task LoadDataAsync()
        {
            Templates.Clear();

            var projectTemplates = _context.TemplatesRepository
                                                    .GetAll()
                                                    .Where(f => f.GetTemplateType() == TemplateType.Page &&
                                                    f.GetFramework() == _context.SelectedFrameworkType.Name)
                                                    .Select(t => new TemplateViewModel(t))
                                                    .OrderBy(t => t.Order)
                                                    .ToList();

            Templates.AddRange(projectTemplates);

            TemplateSelected = projectTemplates.FirstOrDefault();

            await Task.FromResult(true);
        }

        private void SaveAndClose()
        {
            var genInfo = new GenInfo
            {
                Name = ItemName,
                Template = TemplateSelected.Info
            };

            _dialog.DialogResult = true;
            _dialog.Result = genInfo;

            _dialog.Close();
        }

        private void HandleValidation(ValidationResult validationResult)
        {
            _isValid = validationResult.IsValid;

            if (!validationResult.IsValid)
            {
                var message = PagesStepResources.ResourceManager.GetString($"ValidationError_{validationResult.ErrorType}");
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "UndefinedError";
                }
                throw new Exception(message);
            }
        }

        private bool IsValid() => _isValid;
    }
}
