using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sample02_DSL
{
	public class DialogBuilder<TModel>
	{
		private List<DialogInputField<TModel>> Fields = new List<DialogInputField<TModel>>();
		private string Title;

		internal DialogBuilder()
		{
		}
		

		public DialogBuilder<TModel> AddFields(params DialogInputField<TModel>[] fields)
		{
			Fields.AddRange(fields);
			return this;
		}

		public DialogBuilder<TModel> WithTitle(string title)
		{
			Title = title;
			return this;
		}

		public Window Build(TModel model)
		{
			var window = new Window
			{
				Height = 300,
				Width = 300,
				Title = Title
			};

			var docPanel = new DockPanel() { Margin = new Thickness(10), LastChildFill = true };
			window.Content = docPanel;

			var buttonPanel = new StackPanel()
			{
				Height = 37,
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Right
			};

			docPanel.Children.Add(buttonPanel);
			buttonPanel.SetValue(DockPanel.DockProperty, Dock.Bottom);

			var okButton = new Button() {Content = "OK", Width = 75, IsDefault = true, IsCancel = true};
			okButton.Click += (sender, args) => { window.DialogResult = true; };

			var cancelButton = new Button() {Content = "Cancel", Width = 75, IsCancel = true};
			cancelButton.Click += (sender, args) => { window.DialogResult = false; };

			buttonPanel.Children.Add(okButton);
			buttonPanel.Children.Add(cancelButton);

			var fieldsPannel = new StackPanel();
			docPanel.Children.Add(fieldsPannel);
			fieldsPannel.SetValue(DockPanel.DockProperty, Dock.Top);

			foreach (var field in Fields)
			{
				fieldsPannel.Children.Add(new TextBlock() { Text = field.Name });
				var wpfField = new TextBox();
				fieldsPannel.Children.Add(wpfField);
				wpfField.SetBinding(TextBox.TextProperty, new Binding(field.PropertyPath) {Source = model, Mode = BindingMode.TwoWay});
			}

			return window;
		}
	}

	public class DialogInputField<TModel>
	{
		private readonly PropertyInfo _propertyInfo;
		private string _name;

		internal DialogInputField(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
		}

		public DialogInputField<TModel> WithName(string name)
		{
			_name = name;
			return this;
		}

		public string Name {
			get
			{
				if (!string.IsNullOrWhiteSpace(_name))
					return _name;

				if (_propertyInfo != null)
				{
					var displayName = _propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
					if (displayName != null)
						return displayName.DisplayName;

					return _propertyInfo.Name;
				}

				return "NoName";
			}
		}

		public string PropertyPath
		{
			get
			{
				if (_propertyInfo == null)
					return "";

				return _propertyInfo.Name;
			}
		}

	}
}
