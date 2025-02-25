using System;
using Gtk;
using Mono.Addins;
using Mono.Addins.Setup;
using Pinta.Core;
using Pinta.Resources;

namespace Pinta.Gui.Addins
{
	internal class AddinListView : Adw.Bin
	{
		private Gio.ListStore model;
		private Gtk.SingleSelection selection_model;
		private Gtk.SignalListItemFactory factory;
		private Gtk.ListView list_view;

		private Adw.StatusPage empty_list_page;
		private Gtk.ScrolledWindow list_view_scroll;
		private Adw.ViewStack list_view_stack;

		private AddinInfoView info_view;

		/// <summary>
		/// Event raised when addins are installed or uninstalled.
		/// </summary>
		public event EventHandler? OnAddinChanged;

		public AddinListView ()
		{
			model = Gio.ListStore.New (AddinListViewItem.GetGType ());

			selection_model = Gtk.SingleSelection.New (model);
			selection_model.OnSelectionChanged ((_, _) => HandleSelectionChanged ());
			selection_model.Autoselect = true;

			factory = Gtk.SignalListItemFactory.New ();
			factory.OnSetup += (factory, args) => {
				var item = (Gtk.ListItem) args.Object;
				item.SetChild (new AddinListViewItemWidget ());
			};
			factory.OnBind += (factory, args) => {
				var list_item = (Gtk.ListItem) args.Object;
				var model_item = (AddinListViewItem) list_item.GetItem ()!;
				var widget = (AddinListViewItemWidget) list_item.GetChild ()!;
				widget.Update (model_item);
			};

			// TODO - have an option to group by category like the old GTK2 addin dialog.
			list_view = ListView.New (selection_model, factory);

			list_view_scroll = Gtk.ScrolledWindow.New ();
			list_view_scroll.SetChild (list_view);
			list_view_scroll.SetSizeRequest (300, 400);
			list_view_scroll.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);

			empty_list_page = new Adw.StatusPage () {
				IconName = StandardIcons.SystemSearch,
				Title = Translations.GetString ("No Items Found")
			};
			empty_list_page.AddCssClass (AdwaitaStyles.Compact);

			list_view_stack = Adw.ViewStack.New ();
			list_view_stack.Add (list_view_scroll);
			list_view_stack.Add (empty_list_page);

			info_view = new AddinInfoView ();
			info_view.OnAddinChanged += (o, e) => OnAddinChanged?.Invoke (o, e);

			var flap = Adw.Flap.New ();
			flap.FoldPolicy = Adw.FlapFoldPolicy.Never;
			flap.Locked = true;
			flap.Content = list_view_stack;
			flap.Separator = Gtk.Separator.New (Orientation.Vertical);
			flap.FlapPosition = PackType.End;
			flap.SetFlap (info_view);
			SetChild (flap);
		}

		public void Clear ()
		{
			model.RemoveAll ();
			list_view_stack.VisibleChild = empty_list_page;
		}

		public void AddAddin (SetupService service, AddinHeader info, Addin addin, AddinStatus status)
		{
			list_view_stack.VisibleChild = list_view_scroll;

			model.Append (new AddinListViewItem (service, info, addin, status));

			// Adding items may not cause a selection-changed signal, as mentioned in the SelectionModel docs
			if (model.NItems == 1)
				HandleSelectionChanged ();
		}

		public void AddAddinRepositoryEntry (SetupService service, AddinHeader info, AddinRepositoryEntry addin, AddinStatus status)
		{
			list_view_stack.VisibleChild = list_view_scroll;

			model.Append (new AddinListViewItem (service, info, addin, status));

			// Adding items may not cause a selection-changed signal, as mentioned in the SelectionModel docs
			if (model.NItems == 1)
				HandleSelectionChanged ();
		}

		private void HandleSelectionChanged ()
		{
			if (model.GetObject (selection_model.Selected) is AddinListViewItem item)
				info_view.Update (item);
			else
				info_view.Update (null);
		}
	}

	[Flags]
	internal enum AddinStatus
	{
		NotInstalled = 0,
		Installed = 1,
		Disabled = 2,
		HasUpdate = 4
	}
}
