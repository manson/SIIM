using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace SkinnableApp
{
	public partial class DemoApp : Application
	{
		public void ApplySkin(Uri skinDictionaryUri)
		{
			// Load the ResourceDictionary into memory.
			ResourceDictionary skinDict = Application.LoadComponent(skinDictionaryUri) as ResourceDictionary;

			Collection<ResourceDictionary> mergedDicts = base.Resources.MergedDictionaries;

			// Remove the existing skin dictionary, if one exists.
			// NOTE: In a real application, this logic might need
			// to be more complex, because there might be dictionaries
			// which should not be removed.
			if (mergedDicts.Count > 0)
				mergedDicts.Clear();

			// Apply the selected skin so that all elements in the
			// application will honor the new look and feel.
			mergedDicts.Add(skinDict);

		}
	}
}